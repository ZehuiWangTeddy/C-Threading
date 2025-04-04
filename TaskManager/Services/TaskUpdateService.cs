using System;
using System.Collections.Generic;
using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Repositories;

namespace TaskManager.Services
{
    /// <summary>
    /// Service that provides task update notifications using standard C# events
    /// </summary>
    public class TaskUpdateService : ITaskUpdateNotifier, IDisposable
    {
        private readonly ITaskRepository _taskRepository;
        private bool _disposed = false;
        
        // Dictionary to track task subscribers by ID
        private readonly Dictionary<int, List<EventHandler<TaskUpdateEventArgs>>> _taskSubscribers = new Dictionary<int, List<EventHandler<TaskUpdateEventArgs>>>();
        
        /// <summary>
        /// Event that is raised when a task is updated
        /// </summary>
        public event EventHandler<TaskUpdateEventArgs> TaskUpdated;
        
        /// <summary>
        /// Event that is raised when a task's status changes
        /// </summary>
        public event EventHandler<TaskUpdateEventArgs> TaskStatusChanged;
        
        /// <summary>
        /// Event that is raised when a log is added to a task
        /// </summary>
        public event EventHandler<TaskUpdateEventArgs> TaskLogAdded;
        
        /// <summary>
        /// Event that is raised when a task's execution time changes
        /// </summary>
        public event EventHandler<TaskUpdateEventArgs> TaskExecutionTimeChanged;
        
        /// <summary>
        /// Event that is raised when a task is created
        /// </summary>
        public event EventHandler<TaskUpdateEventArgs> TaskCreated;
        
        /// <summary>
        /// Event that is raised when a task is deleted
        /// </summary>
        public event EventHandler<TaskUpdateEventArgs> TaskDeleted;
        
        public TaskUpdateService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        }
        
        /// <summary>
        /// Notifies subscribers that a task has been updated
        /// </summary>
        public void NotifyTaskUpdated(BaseTask task)
        {
            if (_disposed) return;
            
            // Raise the general task updated event
            TaskUpdated?.Invoke(this, new TaskUpdateEventArgs(task, TaskUpdateType.Updated));
            
            // Notify task-specific subscribers
            NotifyTaskSpecificSubscribers(task);
        }
        
        /// <summary>
        /// Notifies subscribers that a task's status has changed
        /// </summary>
        public void NotifyStatusChanged(BaseTask task)
        {
            if (_disposed) return;
            
            // Raise the status changed event
            TaskStatusChanged?.Invoke(this, new TaskUpdateEventArgs(task, TaskUpdateType.StatusChanged, "Status"));
            
            // Also notify general subscribers
            NotifyTaskUpdated(task);
        }
        
        /// <summary>
        /// Notifies subscribers that a log has been added to a task
        /// </summary>
        public void NotifyLogAdded(BaseTask task)
        {
            if (_disposed) return;
            
            // Raise the log added event
            TaskLogAdded?.Invoke(this, new TaskUpdateEventArgs(task, TaskUpdateType.LogAdded, "Logger"));
            
            // Also notify general subscribers
            NotifyTaskUpdated(task);
        }
        
        /// <summary>
        /// Notifies subscribers that a task's execution time has changed
        /// </summary>
        public void NotifyExecutionTimeChanged(BaseTask task)
        {
            if (_disposed) return;
            
            // Raise the execution time changed event
            TaskExecutionTimeChanged?.Invoke(this, new TaskUpdateEventArgs(task, TaskUpdateType.ExecutionTimeChanged, "ExecutionTime"));
            
            // Also notify general subscribers
            NotifyTaskUpdated(task);
        }
        
        /// <summary>
        /// Notifies subscribers that a task has been created
        /// </summary>
        public void NotifyTaskCreated(BaseTask task)
        {
            if (_disposed) return;
            
            // Raise the task created event
            TaskCreated?.Invoke(this, new TaskUpdateEventArgs(task, TaskUpdateType.Created));
            
            // Also notify general subscribers
            NotifyTaskUpdated(task);
        }
        
        /// <summary>
        /// Notifies subscribers that a task has been deleted
        /// </summary>
        public void NotifyTaskDeleted(BaseTask task)
        {
            if (_disposed) return;
            
            // Raise the task deleted event
            TaskDeleted?.Invoke(this, new TaskUpdateEventArgs(task, TaskUpdateType.Deleted));
            
            // Clean up task-specific subscribers
            if (_taskSubscribers.ContainsKey(task.Id))
            {
                _taskSubscribers.Remove(task.Id);
            }
        }
        
        /// <summary>
        /// Subscribes to updates for a specific task
        /// </summary>
        /// <param name="taskId">The ID of the task to subscribe to</param>
        /// <param name="handler">The event handler to call when the task is updated</param>
        public void SubscribeToTask(int taskId, EventHandler<TaskUpdateEventArgs> handler)
        {
            if (_disposed) return;
            
            if (!_taskSubscribers.ContainsKey(taskId))
            {
                _taskSubscribers[taskId] = new List<EventHandler<TaskUpdateEventArgs>>();
            }
            
            _taskSubscribers[taskId].Add(handler);
        }
        
        /// <summary>
        /// Unsubscribes from updates for a specific task
        /// </summary>
        /// <param name="taskId">The ID of the task to unsubscribe from</param>
        /// <param name="handler">The event handler to remove</param>
        public void UnsubscribeFromTask(int taskId, EventHandler<TaskUpdateEventArgs> handler)
        {
            if (_disposed) return;
            
            if (_taskSubscribers.ContainsKey(taskId))
            {
                _taskSubscribers[taskId].Remove(handler);
                
                // Clean up if no subscribers left
                if (_taskSubscribers[taskId].Count == 0)
                {
                    _taskSubscribers.Remove(taskId);
                }
            }
        }
        
        /// <summary>
        /// Notifies task-specific subscribers about an update
        /// </summary>
        private void NotifyTaskSpecificSubscribers(BaseTask task)
        {
            if (_taskSubscribers.ContainsKey(task.Id))
            {
                var args = new TaskUpdateEventArgs(task, TaskUpdateType.Updated);
                
                // Create a copy of the list to avoid modification during enumeration
                var handlers = new List<EventHandler<TaskUpdateEventArgs>>(_taskSubscribers[task.Id]);
                
                foreach (var handler in handlers)
                {
                    handler(this, args);
                }
            }
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            _taskSubscribers.Clear();
        }
    }
} 