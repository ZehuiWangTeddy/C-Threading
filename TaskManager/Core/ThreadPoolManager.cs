using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;

namespace TaskManager.Models
{
    public class ThreadPoolManager : IDisposable
    {
        // Task queue storing pending tasks (thread-safe with lock)
        private readonly List<BaseTask> _taskQueue = new();
        
        // Task repository to fetch and update task states
        private readonly ITaskRepository _taskRepository;
        
        // Lock object for thread synchronization
        private readonly object _lock = new();
        
        // Cancellation token for stopping background tasks
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        
        // Constants
        private const int MAX_QUEUE_SIZE = 50;
        private const int MAX_THREADS = 10;
        private const int FETCH_INTERVAL_MS = 1000; // 1 second
        
        /// <summary>
        /// Initializes the ThreadPoolManager and starts the background task fetcher.
        /// </summary>
        public ThreadPoolManager(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));

            // Start background thread to fetch tasks every second
            Task.Run(FetchAndQueueTasks, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Periodically fetches new tasks from the database and adds them to the queue.
        /// </summary>
  private async Task FetchAndQueueTasks()
{
    try
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                // Fetch tasks due for execution within the next 5 seconds
                var now = DateTime.Now;
                var nextFiveSeconds = now.AddSeconds(5000);
                
                Console.WriteLine($"Fetching tasks for {nextFiveSeconds}");

                var tasks = _taskRepository.GetTasks(StatusType.Pending)
                    // .Where(t =>
                    //      t.ExecutionTime != null && 
                    //      ((t.ExecutionTime.NextExecutionTime != null && t.ExecutionTime.NextExecutionTime <= nextFiveSeconds) || 
                    //       (t.ExecutionTime.OnceExecutionTime != null && t.ExecutionTime.OnceExecutionTime <= nextFiveSeconds)))
                    .OrderBy(t => t.ExecutionTime?.NextExecutionTime ?? t.ExecutionTime?.OnceExecutionTime)
                    .Take(MAX_QUEUE_SIZE)
                    .ToList();
                
                Console.WriteLine($"Fetched tasks: {tasks.Count}");

                lock (_lock)
                {
                    // Add new tasks to the queue if space is available
                    foreach (var task in tasks)
                    {
                        if (_taskQueue.Count >= MAX_QUEUE_SIZE) break;
                        _taskQueue.Add(task);
                        Console.WriteLine($"Queued task: {task.Name}");
                    }
                }
                
                // Create a local copy of the token to prevent race conditions
                CancellationToken token = _cancellationTokenSource.Token;
                
                // Wait before fetching again
                await Task.Delay(FETCH_INTERVAL_MS, token);
            }
            catch (OperationCanceledException)
            {
                // This is expected when cancellation occurs
                break;
            }
            catch (ObjectDisposedException)
            {
                // Token was disposed, exit loop
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tasks: {ex.Message}");
                
                // Add a delay to prevent tight loop in case of persistent errors
                await Task.Delay(1000);
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
        public void StartProcessing()
        {
            for (int i = 0; i < MAX_THREADS; i++)
            {
                var workerThread = new Thread(ProcessQueuedTasks)
                {
                    IsBackground = true // Ensure the thread stops when the app exits
                };
                workerThread.Start();
            }
        }

        /// <summary>
        /// Worker method that continuously processes tasks from the queue.
        /// </summary>
        private void ProcessQueuedTasks()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                BaseTask? taskToExecute = null;

                lock (_lock)
                {
                    // Pick a task if available and execution time has arrived
                    if (_taskQueue.Count > 0)
                    {
                        var now = DateTime.Now;
                        taskToExecute = _taskQueue.FirstOrDefault(task =>
                            (task.ExecutionTime?.NextExecutionTime ?? task.ExecutionTime?.OnceExecutionTime) <= now);

                        if (taskToExecute != null)
                        {
                            _taskQueue.Remove(taskToExecute);
                        }
                    }
                }

                if (taskToExecute != null)
                {
                    ExecuteTask(taskToExecute);
                }
                else
                {
                    // Avoid busy-waiting; sleep briefly before checking again
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Executes a given task and updates its status in the database.
        /// </summary>
        private void ExecuteTask(BaseTask task)
        {
            try
            {
                // Mark as running and save state
                task.SetStatus(StatusType.Running);
                task.ThreadId = Thread.CurrentThread.ManagedThreadId;
                _taskRepository.SaveTask(task);

                // Execute the task logic
                task.Execute();

                // Update status based on recurrence
                if (task.ExecutionTime.RecurrencePattern != RecurrencePattern.OneTime)
                {
                    task.ExecutionTime.CalculateNextExecutionTime();
                    task.SetStatus(StatusType.Pending);
                }
                else
                {
                    task.SetStatus(StatusType.Completed);
                }

                // Save updated task state
                _taskRepository.SaveTask(task);
                Console.WriteLine($"Task {task.Name} completed on thread {Thread.CurrentThread.ManagedThreadId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing task {task.Name}: {ex.Message}");
                task.SetStatus(StatusType.Failed);
                _taskRepository.SaveTask(task);
            }
        }

        /// <summary>
        /// Stops the thread pool manager and releases resources.
        /// </summary>
        public void Dispose()
        {
            try 
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested) 
                {
                    _cancellationTokenSource.Cancel();
                }
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"CancellationTokenSource was already disposed: {ex.Message}");
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing CancellationTokenSource: {ex.Message}");
                }
            }
        }
    }
}