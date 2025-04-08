using System;
using TaskManager.Models.DBModels;

namespace TaskManager.Models
{
    /// <summary>
    /// Interface for notifying subscribers about task updates
    /// </summary>
    public interface ITaskUpdateNotifier
    {
        /// <summary>
        /// Event that is raised when a task is updated
        /// </summary>
        event EventHandler<TaskUpdateEventArgs> TaskUpdated;
        
        /// <summary>
        /// Event that is raised when a task's status changes
        /// </summary>
        event EventHandler<TaskUpdateEventArgs> TaskStatusChanged;
        
        /// <summary>
        /// Event that is raised when a log is added to a task
        /// </summary>
        event EventHandler<TaskUpdateEventArgs> TaskLogAdded;
        
        /// <summary>
        /// Event that is raised when a task's execution time changes
        /// </summary>
        event EventHandler<TaskUpdateEventArgs> TaskExecutionTimeChanged;
        
        /// <summary>
        /// Event that is raised when a task is created
        /// </summary>
        event EventHandler<TaskUpdateEventArgs> TaskCreated;
        
        /// <summary>
        /// Event that is raised when a task is deleted
        /// </summary>
        event EventHandler<TaskUpdateEventArgs> TaskDeleted;
        
        /// <summary>
        /// Notifies subscribers that a task has been updated
        /// </summary>
        /// <param name="task">The updated task</param>
        void NotifyTaskUpdated(BaseTask task);
        
        /// <summary>
        /// Notifies subscribers that a task's status has changed
        /// </summary>
        /// <param name="task">The task with changed status</param>
        void NotifyStatusChanged(BaseTask task);
        
        /// <summary>
        /// Notifies subscribers that a log has been added to a task
        /// </summary>
        /// <param name="task">The task with added log</param>
        void NotifyLogAdded(BaseTask task);
        
        /// <summary>
        /// Notifies subscribers that a task's execution time has changed
        /// </summary>
        /// <param name="task">The task with changed execution time</param>
        void NotifyExecutionTimeChanged(BaseTask task);
        
        /// <summary>
        /// Notifies subscribers that a task has been created
        /// </summary>
        /// <param name="task">The created task</param>
        void NotifyTaskCreated(BaseTask task);
        
        /// <summary>
        /// Notifies subscribers that a task has been deleted
        /// </summary>
        /// <param name="task">The deleted task</param>
        void NotifyTaskDeleted(BaseTask task);
    }

    /// <summary>
    /// Event arguments for task updates
    /// </summary>
    public class TaskUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// The updated task
        /// </summary>
        public BaseTask Task { get; }
        
        /// <summary>
        /// The type of update that occurred
        /// </summary>
        public TaskUpdateType UpdateType { get; }
        
        /// <summary>
        /// The property that was updated (if applicable)
        /// </summary>
        public string PropertyName { get; }
        
        public TaskUpdateEventArgs(BaseTask task, TaskUpdateType updateType, string propertyName = null)
        {
            Task = task;
            UpdateType = updateType;
            PropertyName = propertyName;
        }
    }
    
    /// <summary>
    /// Types of task updates
    /// </summary>
    public enum TaskUpdateType
    {
        Created,
        Updated,
        StatusChanged,
        LogAdded,
        ExecutionTimeChanged,
        Deleted
    }
} 