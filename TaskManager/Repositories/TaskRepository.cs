using SQLite;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Models;
using SQLiteNetExtensions.Extensions;
using TaskManager.Services;

namespace TaskManager.Repositories
{
    public interface ITaskRepository
    {
        void SaveTask(BaseTask task);
        List<BaseTask> GetTasks(StatusType status);
        BaseTask? GetTaskById(int taskId);
        void UpdateTaskStatus(int taskId, StatusType status);

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

            InitializeDummyData();
        }

        private void InitializeDummyData()
        {
            // Create dummy EmailNotificationTask
            var emailExecutionTime = new ExecutionTime(
                onceExecutionTime: DateTime.Now.AddHours(1),
                recurrencePattern: RecurrencePattern.OneTime,
                nextExecutionTime: null
            );
            var emailLogger = new TaskLogger();
            var emailTask = new EmailNotificationTask("Dummy Email Task", 0, PriorityType.High, "sender@example.com", "receiver@example.com", "Test Email", "This is a test email")
            {
                ExecutionTime = emailExecutionTime,
                Logger = emailLogger
            };
            SaveTask(emailTask);

            // Create dummy FileBackupSystemTask
            var backupExecutionTime = new ExecutionTime(
                onceExecutionTime: null,
                recurrencePattern: RecurrencePattern.Daily,
                nextExecutionTime: DateTime.Now.AddDays(1)
            );
            var backupLogger = new TaskLogger();
            var backupTask = new FileBackupSystemTask("Dummy Backup Task", 0, PriorityType.Medium, "/path/to/source", "/path/to/backup")
            {
                ExecutionTime = backupExecutionTime,
                Logger = backupLogger
            };
            SaveTask(backupTask);

            // Create dummy FileCompressionTask
            var compressionExecutionTime = new ExecutionTime(
                onceExecutionTime: DateTime.Now.AddHours(2),
                recurrencePattern: RecurrencePattern.OneTime,
                nextExecutionTime: null
            );
            var compressionLogger = new TaskLogger();
            var compressionTask = new FileCompressionTask("Dummy Compression Task", 0, PriorityType.Low, "/path/to/compress")
            {
                ExecutionTime = compressionExecutionTime,
                Logger = compressionLogger
            };
            SaveTask(compressionTask);

            // Create dummy FolderWatcherTask
            var watcherExecutionTime = new ExecutionTime(
                onceExecutionTime: null,
                recurrencePattern: RecurrencePattern.Minute,
                nextExecutionTime: DateTime.Now.AddMinutes(1)
            );
            var watcherLogger = new TaskLogger();
            var watcherTask = new FolderWatcherTask("Dummy Watcher Task", 0, PriorityType.High, "/path/to/watch")
            {
                ExecutionTime = watcherExecutionTime,
                Logger = watcherLogger
            };
            SaveTask(watcherTask);

            Console.WriteLine("Dummy tasks created successfully!");
        }

        public void SaveTask(BaseTask task)
        {
            if (task is EmailNotificationTask emailTask)
            {
                _sqliteConnection.InsertWithChildren(emailTask, recursive: true);
            }
            else if (task is FileBackupSystemTask backupTask)
            {
                _sqliteConnection.InsertWithChildren(backupTask, recursive: true);
            }
            else if (task is FileCompressionTask compressionTask)
            {
                _sqliteConnection.InsertWithChildren(compressionTask, recursive: true);
            }
            else if (task is FolderWatcherTask folderWatcherTask)
            {
                _sqliteConnection.InsertWithChildren(folderWatcherTask, recursive: true);
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