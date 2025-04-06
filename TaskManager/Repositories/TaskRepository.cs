using SQLite;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Models;
using System.Diagnostics;

namespace TaskManager.Repositories
{
    public interface ITaskRepository
    {
        void SaveTask(BaseTask task);
        List<BaseTask> GetTasks(StatusType status);
        BaseTask? GetTaskById(int taskId);
        void UpdateTaskStatus(int taskId, StatusType status);

        //Add On Base UpdateTaskStatus --> UpdateTime
        void UpdateTaskComplished(string taskType, int taskId);

        void SaveExecutionTime(ExecutionTime executionTime);
        ExecutionTime GetExecutionTime(int id);

        List<BaseTask> GetAllTasks();
        //Fix--> T 
        void DeleteTask<T>(int taskId);

        List<EmailNotificationTask> GetEmailTasks();
        List<FileBackupSystemTask> GetFileBackupTasks();
        List<FileCompressionTask> GetCompressionTasks();
        List<FolderWatcherTask> GetFolderWatcherTasks();
    }

    public class TaskRepository : ITaskRepository
    {
        private readonly SQLiteConnection _sqliteConnection;

        public TaskRepository(string dbPath)
        {
            SQLiteOpenFlags flags = SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.SharedCache;
            _sqliteConnection = new SQLiteConnection(dbPath, flags);
            _sqliteConnection.Execute("PRAGMA foreign_keys = ON;");
            _sqliteConnection.CreateTable<ExecutionTime>();
            _sqliteConnection.CreateTable<TaskLogger>();
            _sqliteConnection.CreateTable<EmailNotificationTask>();
            _sqliteConnection.CreateTable<FileBackupSystemTask>();
            _sqliteConnection.CreateTable<FileCompressionTask>();
            _sqliteConnection.CreateTable<FolderWatcherTask>();
        }

        public void SaveExecutionTime(ExecutionTime executionTime)
        {
            _sqliteConnection.Insert(executionTime);
        }

        public ExecutionTime GetExecutionTime(int id)
        {
            return _sqliteConnection.Find<ExecutionTime>(id);
        }
        
        private TaskLogger GetTaskLogger(int? loggerId)
        {
            if (!loggerId.HasValue) return new TaskLogger();
            return _sqliteConnection.Find<TaskLogger>(loggerId.Value) ?? new TaskLogger();
        }

        public void SaveTask(BaseTask task)
        {
            int count = 0;
    
            // Save the TaskLogger first if it exists
            if (task.Logger != null)
            {
                if (task.Logger.Id == 0)
                {
                    _sqliteConnection.Insert(task.Logger);
                }
                else
                {
                    _sqliteConnection.Update(task.Logger);
                }
                task.TaskLoggerId = task.Logger.Id;
            }
    
            // For ExecutionTime (if you're not already handling this)
            if (task.ExecutionTime != null)
            {
                if (task.ExecutionTime.Id == 0)
                {
                    _sqliteConnection.Insert(task.ExecutionTime);
                }
                else
                {
                    _sqliteConnection.Update(task.ExecutionTime);
                }
                task.ExecutionTimeId = task.ExecutionTime.Id;
            }
    
            // Check if task exists (has an ID > 0) to determine insert or update
            bool isNewTask = task.Id == 0;
    
            if (task is EmailNotificationTask emailTask)
            {
                count = isNewTask ? 
                    _sqliteConnection.Insert(emailTask) : 
                    _sqliteConnection.Update(emailTask);
            }
            else if (task is FileBackupSystemTask backupTask)
            {
                count = isNewTask ? 
                    _sqliteConnection.Insert(backupTask) : 
                    _sqliteConnection.Update(backupTask);
            }
            else if (task is FileCompressionTask compressionTask)
            {
                count = isNewTask ? 
                    _sqliteConnection.Insert(compressionTask) : 
                    _sqliteConnection.Update(compressionTask);
            }
            else if (task is FolderWatcherTask folderWatcherTask)
            {
                count = isNewTask ? 
                    _sqliteConnection.Insert(folderWatcherTask) : 
                    _sqliteConnection.Update(folderWatcherTask);
            }
    
            Debug.WriteLine($"{(isNewTask ? "Inserted" : "Updated")} task ID: {task.Id}, Count: {count}");
        }
       public List<BaseTask> GetTasks(StatusType status)
{
    Debug.WriteLine("Fetching tasks...");
    int statusValue = (int)status;
    
    if (_sqliteConnection == null)
    {
        Debug.WriteLine("SQLite connection is null");
        return new List<BaseTask>();
    }

    try
    {
        var emailTasks = _sqliteConnection.Table<EmailNotificationTask>()
            .Where(t => t.StatusValue == statusValue)
            .ToList();
        
        foreach (var task in emailTasks)
        {
            if (task.ExecutionTimeId.HasValue)
            {
                task.ExecutionTime = GetExecutionTime(task.ExecutionTimeId.Value);
            }
            
            if (task.TaskLoggerId.HasValue)
            {
                task.Logger = _sqliteConnection.Find<TaskLogger>(task.TaskLoggerId.Value);
            }
        }
        Debug.WriteLine($"Email Tasks Count: {emailTasks.Count}");

        var backupTasks = _sqliteConnection.Table<FileBackupSystemTask>()
            .Where(t => t.StatusValue == statusValue)
            .ToList();
        
        foreach (var task in backupTasks)
        {
            if (task.ExecutionTimeId.HasValue)
            {
                task.ExecutionTime = GetExecutionTime(task.ExecutionTimeId.Value);
            }
            
            if (task.TaskLoggerId.HasValue)
            {
                task.Logger = _sqliteConnection.Find<TaskLogger>(task.TaskLoggerId.Value);
            }
        }
        Debug.WriteLine($"Backup Tasks Count: {backupTasks.Count}");

        var compressionTasks = _sqliteConnection.Table<FileCompressionTask>()
            .Where(t => t.StatusValue == statusValue)
            .ToList();
        
        // Load ExecutionTime for each compression task
        foreach (var task in compressionTasks)
        {
            if (task.ExecutionTimeId.HasValue)
            {
                task.ExecutionTime = GetExecutionTime(task.ExecutionTimeId.Value);
            }
            
            if (task.TaskLoggerId.HasValue)
            {
                task.Logger = _sqliteConnection.Find<TaskLogger>(task.TaskLoggerId.Value);
            }
        }
        Debug.WriteLine($"Compression Tasks Count: {compressionTasks.Count}");

        var folderWatcherTasks = _sqliteConnection.Table<FolderWatcherTask>()
            .Where(t => t.StatusValue == statusValue)
            .ToList();
        
        foreach (var task in folderWatcherTasks)
        {
            if (task.ExecutionTimeId.HasValue)
            {
                task.ExecutionTime = GetExecutionTime(task.ExecutionTimeId.Value);
            }
            
            if (task.TaskLoggerId.HasValue)
            {
                task.Logger = _sqliteConnection.Find<TaskLogger>(task.TaskLoggerId.Value);
            }
        }
        Debug.WriteLine($"Folder Watcher Tasks Count: {folderWatcherTasks.Count}");
        
        return emailTasks.Cast<BaseTask>()
            .Concat(backupTasks.Cast<BaseTask>())
            .Concat(compressionTasks.Cast<BaseTask>())
            .Concat(folderWatcherTasks.Cast<BaseTask>())
            .ToList();
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Exception in GetTasks: {ex}");
        return new List<BaseTask>();
    }
}

        public BaseTask? GetTaskById(int taskId)
        {
            var emailTask = _sqliteConnection.Table<EmailNotificationTask>().FirstOrDefault(t => t.Id == taskId);
            if (emailTask != null) return emailTask;

            var backupTask = _sqliteConnection.Table<FileBackupSystemTask>().FirstOrDefault(t => t.Id == taskId);
            if (backupTask != null) return backupTask;

            var compressionTask = _sqliteConnection.Table<FileCompressionTask>().FirstOrDefault(t => t.Id == taskId);
            if (compressionTask != null) return compressionTask;

            var folderWatcherTask = _sqliteConnection.Table<FolderWatcherTask>().FirstOrDefault(t => t.Id == taskId);
            return folderWatcherTask;
        }

        public List<BaseTask> GetAllTasks()
        {
            var tasks = new List<BaseTask>();

            var folderWorks = GetFolderWatcherTasks();
            foreach (var work in folderWorks)
            {
                work.ExecutionTime = GetExecutionTime((int)work.ExecutionTimeId);
                if (work.TaskLoggerId.HasValue)
                {
                    work.Logger = GetTaskLogger(work.TaskLoggerId);
                }
            }
            tasks.AddRange(folderWorks);

            var backupWorks = GetFileBackupTasks();
            foreach (var work in backupWorks)
            {
                work.ExecutionTime = GetExecutionTime((int)work.ExecutionTimeId);
                if (work.TaskLoggerId.HasValue)
                {
                    work.Logger = GetTaskLogger(work.TaskLoggerId);
                }
            }
            tasks.AddRange(backupWorks);

            var compressionWorks = GetCompressionTasks();
            foreach (var work in compressionWorks)
            {
                work.ExecutionTime = GetExecutionTime((int)work.ExecutionTimeId);
                if (work.TaskLoggerId.HasValue)
                {
                    work.Logger = GetTaskLogger(work.TaskLoggerId);
                }
            }
            tasks.AddRange(compressionWorks);

            var notifTasks = GetEmailTasks();
            foreach (var work in notifTasks)
            {
                work.ExecutionTime = GetExecutionTime((int)work.ExecutionTimeId);
                if (work.TaskLoggerId.HasValue)
                {
                    work.Logger = GetTaskLogger(work.TaskLoggerId);
                }
            }
            tasks.AddRange(notifTasks);
            return tasks;
        }

        public void DeleteTask<T>(int taskId)
        {
            //Maybe you can use enum&id or taskItem
            _sqliteConnection.Delete<T>(taskId);
        }

        public void UpdateTaskStatus(int taskId, StatusType status)
        {
            var task = GetTaskById(taskId);
            if (task != null)
            {
                task.SetStatus(status);
                SaveTask(task);
            }
        }

        public List<EmailNotificationTask> GetEmailTasks() => _sqliteConnection.Table<EmailNotificationTask>().ToList();
        public List<FileBackupSystemTask> GetFileBackupTasks() => _sqliteConnection.Table<FileBackupSystemTask>().ToList();
        public List<FileCompressionTask> GetCompressionTasks() => _sqliteConnection.Table<FileCompressionTask>().ToList();
        public List<FolderWatcherTask> GetFolderWatcherTasks() => _sqliteConnection.Table<FolderWatcherTask>().ToList();


        public void UpdateTaskComplished(string taskType, int taskId)
        {
            BaseTask baseTask;
            switch (taskType)
            {
                case "Folder Watcher Task":
                    baseTask = _sqliteConnection.Table<FolderWatcherTask>()
                         .FirstOrDefault(t => t.Id == taskId);
                    break;
                case "File Compression Task":
                    baseTask = _sqliteConnection.Table<FileCompressionTask>()
                         .FirstOrDefault(t => t.Id == taskId);

                    break;
                case "File Backup System Task":
                    baseTask = _sqliteConnection.Table<FileBackupSystemTask>()
                         .FirstOrDefault(t => t.Id == taskId);

                    break;
                case "Email Notification Task":
                    baseTask = _sqliteConnection.Table<EmailNotificationTask>()
                        .FirstOrDefault(t => t.Id == taskId)
                        ;
                    break;
                default:
                    throw new System.Exception("Task Type Not Found");
            }
            baseTask.ExecutionTime = GetExecutionTime((int)baseTask.ExecutionTimeId);
            if (baseTask.ExecutionTime.OnceExecutionTime != null)
            {
                baseTask.ExecutionTime.OnceExecutionTime = DateTime.Now;
            }
            else
            {
                baseTask.ExecutionTime.NextExecutionTime = DateTime.Now;
            }
            _sqliteConnection.Update(baseTask.ExecutionTime);

            int count;
            switch(taskType)
            {
                case "Folder Watcher Task":
                    count= _sqliteConnection.Update((FolderWatcherTask)baseTask);
                    break;
                case "File Compression Task":
                    count = _sqliteConnection.Update((FileCompressionTask)baseTask);
                    break;
                case "File Backup System Task":
                    count = _sqliteConnection.Update((FileBackupSystemTask)baseTask);
                    break;
                case "Email Notification Task":
                    count = _sqliteConnection.Update((EmailNotificationTask)baseTask);
                    break;
                default:
                    throw new System.Exception("Convert Error");
            }
            Debug.WriteLine($"Update Count:{count}");
        }
    }
}