using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;

namespace TaskManager.Services
{
    public class TaskScheduler : IDisposable
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ThreadPoolManager _threadPoolManager;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly SemaphoreSlim _queueLock;
        private readonly HashSet<string> _processedExecutionTimes;
        private const int MAX_TASKS_TO_FETCH = 20;
        private const int CHECK_INTERVAL_MS = 1000; // 1 second

        public TaskScheduler(ITaskRepository taskRepository, ThreadPoolManager threadPoolManager)
        {
            _taskRepository = taskRepository;
            _threadPoolManager = threadPoolManager;
            _cancellationTokenSource = new CancellationTokenSource();
            _queueLock = new SemaphoreSlim(1, 1);
            _processedExecutionTimes = new HashSet<string>();
            
            // Start the background task
            Task.Run(MonitorAndScheduleTasks, _cancellationTokenSource.Token);
        }

        private async Task MonitorAndScheduleTasks()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await _queueLock.WaitAsync();
                    
                    // Get pending tasks ordered by execution time and priority
                    var pendingTasks = await GetNextTasksToSchedule();
                    
                    // Get available slots in the thread pool
                    var availableSlots = _threadPoolManager.GetAvailableSlots();
                    
                    // Schedule tasks based on available slots
                    foreach (var task in pendingTasks.Take(availableSlots))
                    {
                        // Add to thread pool queue first
                        _threadPoolManager.AddTaskToQueue(task);
                        
                        // Only mark as processed if successfully added to queue
                        var executionKey = $"{task.Id}_{task.ExecutionTime.NextExecutionTime?.Ticks ?? task.ExecutionTime.OnceExecutionTime?.Ticks ?? 0}";
                        _processedExecutionTimes.Add(executionKey);
                        
                        Console.WriteLine($"Scheduled task {task.Name} for execution at {task.ExecutionTime.NextExecutionTime ?? task.ExecutionTime.OnceExecutionTime}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in task scheduler: {ex.Message}");
                }
                finally
                {
                    _queueLock.Release();
                }

                // Wait for the next check interval
                await Task.Delay(CHECK_INTERVAL_MS, _cancellationTokenSource.Token);
            }
        }

        private async Task<List<BaseTask>> GetNextTasksToSchedule()
        {
            var now = DateTime.Now;
            var tasks = new List<BaseTask>();

            // Get tasks from each type
            var emailTasks = _taskRepository.GetEmailTasks()
                .Where(t => t.Status == StatusType.Pending && 
                           (t.ExecutionTime.NextExecutionTime <= now || 
                            t.ExecutionTime.OnceExecutionTime <= now))
                .Cast<BaseTask>();

            var backupTasks = _taskRepository.GetFileBackupTasks()
                .Where(t => t.Status == StatusType.Pending && 
                           (t.ExecutionTime.NextExecutionTime <= now || 
                            t.ExecutionTime.OnceExecutionTime <= now))
                .Cast<BaseTask>();

            var compressionTasks = _taskRepository.GetCompressionTasks()
                .Where(t => t.Status == StatusType.Pending && 
                           (t.ExecutionTime.NextExecutionTime <= now || 
                            t.ExecutionTime.OnceExecutionTime <= now))
                .Cast<BaseTask>();

            var watcherTasks = _taskRepository.GetFolderWatcherTasks()
                .Where(t => t.Status == StatusType.Pending && 
                           (t.ExecutionTime.NextExecutionTime <= now || 
                            t.ExecutionTime.OnceExecutionTime <= now))
                .Cast<BaseTask>();

            // Combine all tasks
            tasks.AddRange(emailTasks);
            tasks.AddRange(backupTasks);
            tasks.AddRange(compressionTasks);
            tasks.AddRange(watcherTasks);

            // Filter out already processed execution times
            tasks = tasks.Where(t => 
            {
                var executionKey = $"{t.Id}_{t.ExecutionTime.NextExecutionTime?.Ticks ?? t.ExecutionTime.OnceExecutionTime?.Ticks ?? 0}";
                return !_processedExecutionTimes.Contains(executionKey);
            }).ToList();

            // Order by execution time and priority
            return tasks
                .OrderBy(t => t.ExecutionTime.NextExecutionTime ?? t.ExecutionTime.OnceExecutionTime)
                .ThenByDescending(t => t.Priority)
                .Take(MAX_TASKS_TO_FETCH)
                .ToList();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _queueLock.Dispose();
        }
    }
} 