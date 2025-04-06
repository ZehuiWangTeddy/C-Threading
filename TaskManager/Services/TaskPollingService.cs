using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;

namespace TaskManager.Services
{
    public class TaskPollingService : IDisposable
    {
        private readonly ITaskRepository _taskRepository;
        private readonly TaskUpdateService _taskUpdateService;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task _pollingTask;
        private bool _isDisposed = false;
        private readonly object _lock = new object();
        private DateTime _lastStatisticsLog = DateTime.MinValue;
        private DateTime _lastTasksLog = DateTime.MinValue;
        
        // Events for notifying subscribers about data updates
        public event EventHandler<List<BaseTask>> TasksUpdated;
        public event EventHandler<BaseTask> TaskByIdUpdated;
        public event EventHandler<TaskStatistics> StatisticsUpdated;
        
        // Statistics data
        private TaskStatistics _currentStatistics = new TaskStatistics();
        
        // Dictionary to track task polling by ID
        private readonly Dictionary<int, CancellationTokenSource> _taskPollingTokens = new Dictionary<int, CancellationTokenSource>();
        
        public TaskPollingService(ITaskRepository taskRepository, TaskUpdateService taskUpdateService)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _taskUpdateService = taskUpdateService ?? throw new ArgumentNullException(nameof(taskUpdateService));
            
            Console.WriteLine("TaskPollingService initialized");
            
            // Start the main polling task
            StartPolling();
        }
        
        public void StartPolling()
        {
            if (_pollingTask != null && !_pollingTask.IsCompleted)
                return;
                
            Console.WriteLine("Starting TaskPollingService polling tasks...");
            
            _pollingTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await PollAllTasks();
                        
                        await PollStatistics();
                        
                        await Task.Delay(1000, _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("TaskPollingService polling cancelled");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in polling task: {ex.Message}");
                        await Task.Delay(5000, _cancellationTokenSource.Token); 
                    }
                }
            }, _cancellationTokenSource.Token);
            
        }
        
        private async Task PollAllTasks()
        {
            try
            {
                var tasks = _taskRepository.GetAllTasks();
                TasksUpdated?.Invoke(this, tasks);
                
                // // Log tasks status every 5 seconds
                // if (DateTime.Now - _lastTasksLog > TimeSpan.FromSeconds(5))
                // {
                //     Console.WriteLine($"\n=== Tasks Status Update ({DateTime.Now:HH:mm:ss}) ===");
                //     Console.WriteLine($"Total Tasks: {tasks.Count}");
                //     foreach (var task in tasks)
                //     {
                //         Console.WriteLine($"Task {task.Id}: {task.Name} - Status: {task.Status}");
                //     }
                //     Console.WriteLine("=====================================\n");
                //     _lastTasksLog = DateTime.Now;
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error polling all tasks: {ex.Message}");
            }
        }
        
        public void StartPollingTaskById(int taskId)
        {
            lock (_lock)
            {
                if (_taskPollingTokens.ContainsKey(taskId))
                    return;
                
                var tokenSource = new CancellationTokenSource();
                _taskPollingTokens[taskId] = tokenSource;
                
                Task.Run(async () =>
                {
                    while (!tokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var task = _taskRepository.GetTaskById(taskId);
                            if (task != null)
                            {
                                // But status doesn't update, did not work in DB
                                TaskByIdUpdated?.Invoke(this, task);
                            }
                            
                            await Task.Delay(1000, tokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            await Task.Delay(5000, tokenSource.Token);
                        }
                    }
                }, tokenSource.Token);
            }
        }
        
        public void StopPollingTaskById(int taskId)
        {
            lock (_lock)
            {
                if (_taskPollingTokens.TryGetValue(taskId, out var tokenSource))
                {
                    Console.WriteLine($"Stopping polling for task ID: {taskId}");
                    tokenSource.Cancel();
                    _taskPollingTokens.Remove(taskId);
                }
            }
        }
        
        private async Task PollStatistics()
        {
            try
            {
                var allTasks = _taskRepository.GetAllTasks();
                
                var statistics = new TaskStatistics
                {
                    CompletedTasks = allTasks.Count(t => t.Status == StatusType.Completed),
                    FailedTasks = allTasks.Count(t => t.Status == StatusType.Failed),
                    InProgressTasks = allTasks.Count(t => t.Status == StatusType.Running),
                    PendingTasks = allTasks.Count(t => t.Status == StatusType.Pending)
                };
                
                _currentStatistics = statistics;
                StatisticsUpdated?.Invoke(this, statistics);
                
                // Log statistics every 5 seconds
                // if (DateTime.Now - _lastStatisticsLog > TimeSpan.FromSeconds(5))
                // {
                //     Console.WriteLine($"\n=== Statistics Update ({DateTime.Now:HH:mm:ss}) ===");
                //     Console.WriteLine($"Completed Tasks: {statistics.CompletedTasks}");
                //     Console.WriteLine($"Failed Tasks: {statistics.FailedTasks}");
                //     Console.WriteLine($"In Progress Tasks: {statistics.InProgressTasks}");
                //     Console.WriteLine($"Pending Tasks: {statistics.PendingTasks}");
                //     Console.WriteLine("=====================================\n");
                //     _lastStatisticsLog = DateTime.Now;
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error polling statistics: {ex.Message}");
            }
        }
        
        public TaskStatistics GetCurrentStatistics()
        {
            return _currentStatistics;
        }
        
        public void Dispose()
        {
            if (_isDisposed)
                return;
                
            _isDisposed = true;
            _cancellationTokenSource.Cancel();
            lock (_lock)
            {
                foreach (var tokenSource in _taskPollingTokens.Values)
                {
                    tokenSource.Cancel();
                }
                _taskPollingTokens.Clear();
            }
            
            try
            {
                _pollingTask?.Wait(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during TaskPollingService disposal: {ex.Message}");
            }
            
            _cancellationTokenSource.Dispose();
            Console.WriteLine("TaskPollingService disposed successfully");
        }
    }
    
    public class TaskStatistics
    {
        public int CompletedTasks { get; set; }
        public int FailedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int PendingTasks { get; set; }
    }
} 