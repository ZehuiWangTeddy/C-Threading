using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.Services;

namespace TaskManager.Models
{
    public class ThreadPoolManager : IDisposable
    {
        private readonly List<BaseTask> _taskQueue = new();
        private readonly HashSet<int> _tasksInQueue = new();
        private List<Thread> _workerThreads = new List<Thread>();
        private volatile bool _isPaused = false;
        private readonly ITaskRepository _taskRepository;
        private readonly TaskUpdateService _taskUpdateService;
        private readonly object _lock = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private volatile bool _isDisposed = false;
        
        private const int MAX_QUEUE_SIZE = 50;
        private const int MAX_THREADS = 10;
        private const int FETCH_INTERVAL_MS = 5000; 
        private const int PROCESS_INTERVAL_MS = 1000; 
        
        /// <summary>
        /// Initializes the ThreadPoolManager and starts the background task fetcher.
        /// </summary>
        public ThreadPoolManager(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _taskUpdateService = new TaskUpdateService(_taskRepository);
            
            Task.Run(FetchAndQueueTasks, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Periodically fetches new tasks from the database and adds them to the queue.
        /// </summary>
        private async Task FetchAndQueueTasks()
        {
            try
            {
                while (!_isDisposed && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var now = DateTime.Now;
                        var nextMinute = now.AddMinutes(5); 

                        var tasks = _taskRepository.GetTasks(StatusType.Pending)
                            .Where(t =>
                                t.ExecutionTime != null && 
                                ((t.ExecutionTime.NextExecutionTime != null && t.ExecutionTime.NextExecutionTime <= nextMinute) || 
                                (t.ExecutionTime.OnceExecutionTime != null && t.ExecutionTime.OnceExecutionTime <= nextMinute)))
                            .OrderBy(t => t.ExecutionTime?.NextExecutionTime ?? t.ExecutionTime?.OnceExecutionTime)
                            .Take(MAX_QUEUE_SIZE)
                            .ToList();

                        lock (_lock)
                        {
                            foreach (var task in tasks)
                            {
                                if (_tasksInQueue.Contains(task.Id))
                                {
                                    BaseTask? existingTask = GetTaskInQueue(task.Id);
                                    if (existingTask != null)
                                    {
                                       if (existingTask.ExecutionTime.OnceExecutionTime ==
                                            task.ExecutionTime.OnceExecutionTime && existingTask.ExecutionTime.NextExecutionTime == task.ExecutionTime.NextExecutionTime )
                                        {
                                            continue;
                                        }
                                       _taskQueue.Remove(task);
                                       _tasksInQueue.Remove(task.Id);
                                    } 
                                }
                                
                                if (_taskQueue.Count >= MAX_QUEUE_SIZE) 
                                    break;
                            
                                _taskQueue.Add(task);
                                _tasksInQueue.Add(task.Id); 
                                _taskUpdateService.NotifyTaskUpdated(task);
                            }
                        }
                        
                        if (!_isDisposed)
                        {
                            try
                            {
                                await Task.Delay(FETCH_INTERVAL_MS, _cancellationTokenSource.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
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
                        if (!_isDisposed)
                        {
                            await Task.Delay(1000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error in FetchAndQueueTasks: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts the worker threads that execute tasks from the queue.
        /// </summary>
        public void StartProcessingTasks()
        {
            _workerThreads.Clear();
            for (int i = 0; i < MAX_THREADS; i++)
            {
                var thread = new Thread(ProcessQueuedTasks)
                {
                    IsBackground = true,
                    Name = $"TaskProcessor-{i}"
                };
        
                _workerThreads.Add(thread);
                thread.Start();
            }
        }

        /// <summary>
        /// Worker method that continuously processes tasks from the queue.
        /// </summary>
        private void ProcessQueuedTasks()
        {
            try
            {
                
                while (!_isDisposed)
                {
                    try
                    {
                        if (_isDisposed || _cancellationTokenSource.IsCancellationRequested)
                        {
                            break;
                        }

                        if (_isPaused)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        BaseTask? taskToExecute = null;

                        lock (_lock)
                        {
                            if (_taskQueue.Count > 0)
                            {
                                var now = DateTime.Now;
                                taskToExecute = _taskQueue.FirstOrDefault(task =>
                                    (task.ExecutionTime?.NextExecutionTime ?? task.ExecutionTime?.OnceExecutionTime) <= now);

                                if (taskToExecute != null)
                                {
                                    _taskQueue.Remove(taskToExecute);
                                    _tasksInQueue.Remove(taskToExecute.Id);
                                }
                            }
                        }
                        Console.WriteLine(taskToExecute?.Id);
                        if (taskToExecute != null)
                        {
                            ExecuteTask(taskToExecute);
                        }
                        Thread.Sleep(1000);
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (ObjectDisposedException ex)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Sleep briefly after errors
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.Name} fatal error: {ex.Message}\nStack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Executes a given task and updates its status in the database.
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
                
                if (task.ExecutionTime?.RecurrencePattern != RecurrencePattern.OneTime)
                {
                    task.ExecutionTime.CalculateNextExecutionTime();
                    task.SetStatus(StatusType.Pending);
                    task.Logger.AddLogMessage($"Task completed on thread {Thread.CurrentThread.ManagedThreadId} and task will run again at {task.ExecutionTime.NextExecutionTime}");
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
                string errorMessage = $"Error executing task {task.Name} (ID: {task.Id}): {ex.Message}";
                task.Logger.AddLogMessage($"ERROR: {errorMessage}");
                task.SetStatus(StatusType.Failed);
                _taskRepository.SaveTask(task);
                _taskUpdateService.NotifyStatusChanged(task);
                _taskUpdateService.NotifyLogAdded(task);
            }
        }

        /// <summary>
        /// Stops the thread pool manager and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            Console.WriteLine("ThreadPoolManager disposing... Stack trace:");
            Console.WriteLine(Environment.StackTrace); 
            _isDisposed = true;

            try 
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested) 
                {
                    _cancellationTokenSource.Cancel();
                    Console.WriteLine("Cancellation requested");
                }
                Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during dispose: {ex.Message}");
            }
            finally
            {
                try
                {
                    _cancellationTokenSource?.Dispose();
                    Console.WriteLine("ThreadPoolManager disposed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing CancellationTokenSource: {ex.Message}");
                }
            }
        }

        public BaseTask? GetTaskInQueue(int id)
        {
            foreach (var task in _taskQueue)
            {
                if (task.Id == id) return task;
            }
            return null;
        }
        
        public void PauseProcessing()
        {
            _isPaused = true;
        }

        public void ResumeProcessing()
        {
            _isPaused = false;
        }
    }
}