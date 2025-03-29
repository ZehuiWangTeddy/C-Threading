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
        void UpdateTaskComplished(string taskType, int taskId, StatusType status);

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

        public void SaveTask(BaseTask task)
        {
            int count = 0;
            if (task is EmailNotificationTask emailTask)
            {
                count = _sqliteConnection.Insert(emailTask);
            }
            else if (task is FileBackupSystemTask backupTask)
            {
                count = _sqliteConnection.Insert(backupTask);
            }
            else if (task is FileCompressionTask compressionTask)
            {
                count = _sqliteConnection.Insert(compressionTask);
            }
            else if (task is FolderWatcherTask folderWatcherTask)
            {
                count = _sqliteConnection.Insert(folderWatcherTask);
            }
            Debug.WriteLine($"Task saved Count: {count}");
        }

        public List<BaseTask> GetTasks(StatusType status)
        {
            var emailTasks = _sqliteConnection.Table<EmailNotificationTask>().Where(t => t.Status == status).Cast<BaseTask>().ToList();
            var backupTasks = _sqliteConnection.Table<FileBackupSystemTask>().Where(t => t.Status == status).Cast<BaseTask>().ToList();
            var compressionTasks = _sqliteConnection.Table<FileCompressionTask>().Where(t => t.Status == status).Cast<BaseTask>().ToList();
            var folderWatcherTasks = _sqliteConnection.Table<FolderWatcherTask>().Where(t => t.Status == status).Cast<BaseTask>().ToList();

            return emailTasks.Concat(backupTasks).Concat(compressionTasks).Concat(folderWatcherTasks).ToList();
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
            }
            tasks.AddRange(folderWorks);

            var backupWorks = GetFileBackupTasks();
            foreach (var work in backupWorks)
            {
                work.ExecutionTime = GetExecutionTime((int)work.ExecutionTimeId);
            }
            tasks.AddRange(backupWorks);

            var compressionWorks = GetCompressionTasks();
            foreach (var work in compressionWorks)
            {
                work.ExecutionTime = GetExecutionTime((int)work.ExecutionTimeId);
            }
            tasks.AddRange(compressionWorks);

            var notifTasks = GetEmailTasks();
            foreach (var work in notifTasks)
            {
                work.ExecutionTime = GetExecutionTime((int)work.ExecutionTimeId);
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


        public void UpdateTaskComplished(string taskType, int taskId, StatusType status)
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
            baseTask.SetStatus(status);
            if (baseTask.ExecutionTime.OnceExecutionTime != DateTime.MinValue)
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