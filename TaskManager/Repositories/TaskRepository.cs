using SQLite;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Models;

namespace TaskManager.Repositories
{
    public interface ITaskRepository
    {
        void SaveTask(BaseTask task);
        List<BaseTask> GetTasks(StatusType status);
        BaseTask? GetTaskById(int taskId);
        void UpdateTaskStatus(int taskId, StatusType status);
        
        void SaveExecutionTime(ExecutionTime executionTime);
        ExecutionTime GetExecutionTime(int id);
        
        List<BaseTask> GetAllTasks();
        void DeleteTask(int taskId);

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
            _sqliteConnection = new SQLiteConnection(dbPath);
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
            _sqliteConnection.InsertOrReplace(executionTime);
        }

        public ExecutionTime GetExecutionTime(int id)
        {
            return _sqliteConnection.Find<ExecutionTime>(id);
        }
        
        public void SaveTask(BaseTask task)
        {
           
            if (task is EmailNotificationTask emailTask)
            {
                _sqliteConnection.InsertOrReplace(emailTask);
            }
            else if (task is FileBackupSystemTask backupTask)
            {
                _sqliteConnection.InsertOrReplace(backupTask);
            }
            else if (task is FileCompressionTask compressionTask)
            {
                _sqliteConnection.InsertOrReplace(compressionTask);
            }
            else if (task is FolderWatcherTask folderWatcherTask)
            {
                _sqliteConnection.InsertOrReplace(folderWatcherTask);
            }
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
            tasks.AddRange(_sqliteConnection.Table<FolderWatcherTask>().ToList());
            tasks.AddRange(_sqliteConnection.Table<FileCompressionTask>().ToList());
            tasks.AddRange(_sqliteConnection.Table<FileBackupSystemTask>().ToList());
            tasks.AddRange(_sqliteConnection.Table<EmailNotificationTask>().ToList());
            return tasks;
        }

        public void DeleteTask(int taskId)
        {
            _sqliteConnection.Delete<BaseTask>(taskId);
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
    }
}