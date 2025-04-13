using System.Collections.Concurrent;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.Services;

namespace TaskManager.Models;

public class ThreadPoolManager : IDisposable
{
    private const int MAX_QUEUE_SIZE = 50;
    private const int MAX_CONCURRENT_TASKS = 10;
    private const int FETCH_INTERVAL_MS = 5000;
    private const int PROCESS_INTERVAL_MS = 1000;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Semaphore _executionSemaphore;
    private readonly object _lock = new();
    private readonly ConcurrentQueue<BaseTask> _taskQueue = new();
    private readonly ITaskRepository _taskRepository;
    private readonly ConcurrentDictionary<int, BaseTask> _tasksInQueue = new();
    private readonly TaskUpdateService _taskUpdateService;
    private volatile bool _isDisposed;
    private volatile bool _isPaused;

    /// <summary>
    ///     Initializes the ThreadPoolManager and starts the background task fetcher.
    /// </summary>
    public ThreadPoolManager(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _taskUpdateService = new TaskUpdateService(_taskRepository);
        _executionSemaphore = new Semaphore(MAX_CONCURRENT_TASKS, MAX_CONCURRENT_TASKS);

        Task.Run(FetchAndQueueTasks, _cancellationTokenSource.Token);
        StartProcessingTasks();
    }

    /// <summary>
    ///     Disposes of the ThreadPoolManager and its resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _cancellationTokenSource.Cancel();

        try
        {
            _cancellationTokenSource.Dispose();
            _executionSemaphore.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during ThreadPoolManager disposal: {ex.Message}");
        }

        Console.WriteLine("ThreadPoolManager disposed successfully");
    }

    /// <summary>
    ///     Periodically fetches new tasks from the database and adds them to the queue.
    /// </summary>
    private async Task FetchAndQueueTasks()
    {
        try
        {
            while (!_isDisposed && !_cancellationTokenSource.Token.IsCancellationRequested)
                try
                {
                    var now = DateTime.Now;
                    var nextMinute = now.AddMinutes(5);

                    var tasks = _taskRepository.GetTasks(StatusType.Pending, StatusType.Running)
                        .Where(t =>
                            t.ExecutionTime != null &&
                            ((t.ExecutionTime.NextExecutionTime != null &&
                              t.ExecutionTime.NextExecutionTime <= nextMinute) ||
                             (t.ExecutionTime.OnceExecutionTime != null &&
                              t.ExecutionTime.OnceExecutionTime <= nextMinute)))
                        .OrderBy(t => t.ExecutionTime?.NextExecutionTime ?? t.ExecutionTime?.OnceExecutionTime)
                        .Take(MAX_QUEUE_SIZE)
                        .ToList();

                    foreach (var task in tasks)
                    {
                        if (_tasksInQueue.ContainsKey(task.Id))
                            if (_tasksInQueue.TryGetValue(task.Id, out var existingTask))
                            {
                                if (existingTask.ExecutionTime.OnceExecutionTime ==
                                    task.ExecutionTime.OnceExecutionTime &&
                                    existingTask.ExecutionTime.NextExecutionTime ==
                                    task.ExecutionTime.NextExecutionTime)
                                    continue;

                                _taskQueue.TryDequeue(out _);
                                _tasksInQueue.TryRemove(task.Id, out _);
                            }

                        if (_taskQueue.Count >= MAX_QUEUE_SIZE)
                            break;

                        _taskQueue.Enqueue(task);
                        _tasksInQueue.TryAdd(task.Id, task);
                        _taskUpdateService.NotifyTaskUpdated(task);
                    }

                    if (!_isDisposed)
                        try
                        {
                            await Task.Delay(FETCH_INTERVAL_MS, _cancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching tasks: {ex.Message}");
                    if (!_isDisposed) await Task.Delay(1000);
                }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error in FetchAndQueueTasks: {ex.Message}");
        }
    }

    /// <summary>
    ///     Starts the task processing using ThreadPool.
    /// </summary>
    public void StartProcessingTasks()
    {
        ThreadPool.QueueUserWorkItem(_ => ProcessQueuedTasks());
    }

    /// <summary>
    ///     Worker method that continuously processes tasks from the queue using ThreadPool.
    /// </summary>
    private void ProcessQueuedTasks()
    {
        try
        {
            while (!_isDisposed)
                try
                {
                    if (_isDisposed || _cancellationTokenSource.IsCancellationRequested) break;

                    if (_isPaused)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    if (_taskQueue.TryDequeue(out var taskToExecute))
                    {
                        var now = DateTime.Now;
                        if ((taskToExecute.ExecutionTime?.NextExecutionTime ??
                             taskToExecute.ExecutionTime?.OnceExecutionTime) <= now)
                        {
                            _tasksInQueue.TryRemove(taskToExecute.Id, out _);

                            // Queue the task with semaphore control
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                try
                                {
                                    // Wait for a semaphore slot
                                    _executionSemaphore.WaitOne();
                                    try
                                    {
                                        ExecuteTask(taskToExecute);
                                    }
                                    finally
                                    {
                                        // Release the semaphore slot
                                        _executionSemaphore.Release();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error executing task: {ex.Message}");
                                }
                            });
                        }
                        else
                        {
                            // Put the task back in the queue if it's not time to execute
                            _taskQueue.Enqueue(taskToExecute);
                        }
                    }

                    Thread.Sleep(PROCESS_INTERVAL_MS);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
                }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Thread {Thread.CurrentThread.Name} fatal error: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    ///     Executes a given task and updates its status in the database.
    /// </summary>
    private void ExecuteTask(BaseTask task)
    {
        try
        {
            task.SetStatus(StatusType.Running);
            task.ThreadId = Thread.CurrentThread.ManagedThreadId;
            _taskRepository.SaveTask(task);
            _taskUpdateService.NotifyStatusChanged(task);

            task.Execute();
            task.LastCompletionTime = DateTime.Now;

            if (task.ExecutionTime?.RecurrencePattern != RecurrencePattern.OneTime)
            {
                task.ExecutionTime.CalculateNextExecutionTime();
                task.Logger.AddLogMessage(
                    $"Task completed on thread {Thread.CurrentThread.ManagedThreadId} and task will run again at {task.ExecutionTime.NextExecutionTime}");
                _taskUpdateService.NotifyExecutionTimeChanged(task);
                _taskUpdateService.NotifyLogAdded(task);
            }
            else
            {
                task.SetStatus(StatusType.Completed);
            }

            _taskRepository.SaveTask(task);
            _taskUpdateService.NotifyTaskUpdated(task);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error executing task {task.Name} (ID: {task.Id}): {ex.Message}";
            task.Logger.AddLogMessage($"ERROR: {errorMessage}");
            task.SetStatus(StatusType.Failed);
            _taskRepository.SaveTask(task);
            _taskUpdateService.NotifyTaskUpdated(task);
        }
    }

    /// <summary>
    ///     Pauses the task processing.
    /// </summary>
    public void PauseProcessing()
    {
        _isPaused = true;
    }

    /// <summary>
    ///     Resumes the task processing.
    /// </summary>
    public void ResumeProcessing()
    {
        _isPaused = false;
    }
}