using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;

namespace TaskManager.Models
{
    public class ThreadPoolManager
    {
        private readonly ConcurrentQueue<BaseTask> _taskQueue;
        private readonly ConcurrentDictionary<int, Thread> _activeThreads;
        private readonly ITaskRepository _taskRepository;
        private readonly SemaphoreSlim _threadLock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private const int MAX_QUEUE_SIZE = 50;
        private const int MAX_THREADS = 10;
        private const int CHECK_INTERVAL_MS = 1000; // 1 second

        public ThreadPoolManager(ITaskRepository taskRepository)
        {
            _taskQueue = new ConcurrentQueue<BaseTask>();
            _activeThreads = new ConcurrentDictionary<int, Thread>();
            _taskRepository = taskRepository;
            _threadLock = new SemaphoreSlim(1, 1);
            _cancellationTokenSource = new CancellationTokenSource();

            // Start the background task to process queued tasks
            Task.Run(ProcessQueuedTasks, _cancellationTokenSource.Token);
        }

        public void AddTaskToQueue(BaseTask task)
        {
            if (_taskQueue.Count >= MAX_QUEUE_SIZE)
            {
                Console.WriteLine($"Task queue is full. Task {task.Name} will be processed in the next cycle.");
                return;
            }

            _taskQueue.Enqueue(task);
            Console.WriteLine($"Added task {task.Name} to queue. Queue size: {_taskQueue.Count}");
        }

        public int GetAvailableSlots()
        {
            return MAX_QUEUE_SIZE - _taskQueue.Count;
        }

        private async Task ProcessQueuedTasks()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await _threadLock.WaitAsync();

                    // Get current time
                    var now = DateTime.Now;

                    // Process tasks that are due to run
                    while (_taskQueue.Count > 0 && _activeThreads.Count < MAX_THREADS)
                    {
                        if (_taskQueue.TryPeek(out var task))
                        {
                            var executionTime = task.ExecutionTime.NextExecutionTime ?? task.ExecutionTime.OnceExecutionTime;
                            
                            if (executionTime <= now)
                            {
                                if (_taskQueue.TryDequeue(out task))
                                {
                                    // Create and start a new thread for the task
                                    var thread = new Thread(() => ExecuteTask(task));
                                    thread.Start();
                                    _activeThreads.TryAdd(thread.ManagedThreadId, thread);
                                }
                            }
                            else
                            {
                                break; // No more tasks due to run
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing queued tasks: {ex.Message}");
                }
                finally
                {
                    _threadLock.Release();
                }

                // Wait for the next check interval
                await Task.Delay(CHECK_INTERVAL_MS, _cancellationTokenSource.Token);
            }
        }

        private void ExecuteTask(BaseTask task)
        {
            try
            {
                // Update task status and thread ID
                task.SetStatus(StatusType.Running);
                task.ThreadId = Thread.CurrentThread.ManagedThreadId;
                _taskRepository.SaveTask(task);

                // Execute the task
                task.Execute();

                // Calculate next execution time if task is recurring
                if (task.ExecutionTime.RecurrencePattern != RecurrencePattern.OneTime)
                {
                    task.ExecutionTime.CalculateNextExecutionTime();
                    task.SetStatus(StatusType.Pending); // Reset status for next execution
                }
                else
                {
                    task.SetStatus(StatusType.Completed);
                }

                _taskRepository.SaveTask(task);
                Console.WriteLine($"Task {task.Name} completed successfully on thread {Thread.CurrentThread.ManagedThreadId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing task {task.Name}: {ex.Message}");
                task.SetStatus(StatusType.Failed);
                _taskRepository.SaveTask(task);
            }
            finally
            {
                // Remove thread from active threads
                _activeThreads.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _threadLock.Dispose();
        }
    }
}