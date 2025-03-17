using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;

namespace TaskManager.Models
{
    public class ThreadPoolManager
    {
        private readonly int _maxThreads;
        private readonly SemaphoreSlim _semaphore;
        private readonly object _lockObj = new object();
        private readonly List<BaseTask> _taskQueue = new List<BaseTask>();

        public ThreadPoolManager(int maxThreads)
        {
            _maxThreads = maxThreads;
            _semaphore = new SemaphoreSlim(_maxThreads, _maxThreads); 
        }

        public void AddTaskToQueue(BaseTask task)
        {
            lock (_lockObj)
            {
                _taskQueue.Add(task);
            }
            ProcessTasks();
        }

        private void ProcessTasks()
        {
            while (_taskQueue.Count > 0)
            {
                BaseTask task = null;

                lock (_lockObj)
                {
                    if (_taskQueue.Count > 0)
                    {
                        task = _taskQueue[0];
                        _taskQueue.RemoveAt(0); 
                    }
                }

                if (task != null)
                {
                    ExecuteTask(task);
                }
            }
        }

        private void ExecuteTask(BaseTask task)
        {
            Task.Run(async () =>
            {
                await _semaphore.WaitAsync(); 
                try
                {
                  
                    task.Execute();
                    task.GetType().GetProperty("Status")?.SetValue(task, StatusType.Completed); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing task {task.Name}: {ex.Message}");
                    task.GetType().GetProperty("Status")?.SetValue(task, StatusType.Failed); 
                }
                finally
                {
                    _semaphore.Release(); 
                }
            });
        }
    }
}