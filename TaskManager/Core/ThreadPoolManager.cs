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
        
        // Set to track tasks currently in the queue by ID (for fast lookups)
        private readonly HashSet<int> _tasksInQueue = new();
        
        // Task repository to fetch and update task states
        private readonly ITaskRepository _taskRepository;
        
        // Lock object for thread synchronization
        private readonly object _lock = new();
        
        // Cancellation token for stopping background tasks
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        
        // Flag to indicate disposal state
        private volatile bool _isDisposed = false;
        
        // Constants
        private const int MAX_QUEUE_SIZE = 50;
        private const int MAX_THREADS = 10;
        private const int FETCH_INTERVAL_MS = 60000; // 1 minute
        
        /// <summary>
        /// Initializes the ThreadPoolManager and starts the background task fetcher.
        /// </summary>
        public ThreadPoolManager(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));

            // Start background thread to fetch tasks every minute
            Task.Run(FetchAndQueueTasks, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Periodically fetches new tasks from the database and adds them to the queue.
        /// </summary>
        private async Task FetchAndQueueTasks()
        {
            Console.WriteLine("Fetching tasks...");
            try
            {
                while (!_isDisposed && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var now = DateTime.Now;
                        var nextMinute = now.AddMinutes(1); 

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
                                // Skip tasks that are already in the queue
                                if (_tasksInQueue.Contains(task.Id))
                                {
                                    Console.WriteLine($"Task {task.Name} (ID: {task.Id}) already in queue, skipping");
                                    continue;
                                }
                                
                                if (_taskQueue.Count >= MAX_QUEUE_SIZE) 
                                    break;
                                
                                _taskQueue.Add(task);
                                _tasksInQueue.Add(task.Id); // Track the task ID
                                Console.WriteLine($"Queued task: {task.Name} (ID: {task.Id})");
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
            for (int i = 0; i < MAX_THREADS; i++)
            {
                var thread = new Thread(ProcessQueuedTasks)
                {
                    IsBackground = true // Ensure the thread stops when the app exits
                };
                thread.Start();
            }
        }

        /// <summary>
        /// Worker method that continuously processes tasks from the queue.
        /// </summary>
        private void ProcessQueuedTasks()
        {
            while (!_isDisposed)
            {
                // Check cancellation token safely
                bool isCancellationRequested = false;
                try
                {
                    if (!_isDisposed)
                    {
                        isCancellationRequested = _cancellationTokenSource.Token.IsCancellationRequested;
                    }
                }
                catch (ObjectDisposedException)
                {
                    break; // Exit the loop if the token source is disposed
                }

                if (isCancellationRequested)
                {
                    break;
                }

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
                            _tasksInQueue.Remove(taskToExecute.Id); // Remove from tracking set
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
                Console.WriteLine($"Task {task.Name} (ID: {task.Id}) completed on thread {Thread.CurrentThread.ManagedThreadId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing task {task.Name} (ID: {task.Id}): {ex.Message}");
                task.SetStatus(StatusType.Failed);
                _taskRepository.SaveTask(task);
            }
        }

        /// <summary>
        /// Stops the thread pool manager and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true; // Set this flag first to signal all threads to stop

            try 
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested) 
                {
                    _cancellationTokenSource.Cancel();
                }
                
                // Give threads a moment to recognize cancellation before disposing
                Thread.Sleep(100);
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