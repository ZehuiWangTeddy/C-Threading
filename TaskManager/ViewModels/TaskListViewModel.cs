using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TaskManager.Messages;
using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.Views;
using static TaskManager.Messages.DbOperationMessage;

namespace TaskManager.ViewModels
{
    public class TaskListViewModel : ObservableObject
    {
        private readonly ITaskRepository _taskRepository;


        public TaskListViewModel(ITaskRepository taskRepository)
        {
            //IPlatformApplication.Current.Services.GetRequiredService<ITaskRepository>()
            _taskRepository = taskRepository;
            LoadInitialTasks();
            RegisterMessage();

        }
        /// <summary>
        /// Task Work 
        /// </summary>
        /// <param name="item"></param>
        private void StartTask(TaskItem item)
        {
            //Todo Add Task Logic

            _taskRepository.UpdateTaskComplished(item.TaskType, item.Id, StatusType.Running);
        }

        private void RegisterMessage()
        {
            WeakReferenceMessenger.Default.Register<DbOperationMessage>(this, (r, m) =>
            {
                if (m.Master == nameof(TaskListPage) && m.Slave == nameof(TaskListViewModel))
                {
                    switch (m.Type)
                    {
                        case OperationType.Delete:
                            DeleteTask(m.Item);
                            break;
                        case OperationType.Update:
                            //Task Doing
                            StartTask(m.Item);
                            break;
                    }

                    LoadInitialTasks();
                }
            });

            WeakReferenceMessenger.Default.Register<CallEventMessage>(this, (r, m) =>
            {
                if (m.Master == nameof(AddTaskPage) && m.Slave == nameof(TaskListViewModel))
                {
                    switch (m.CallbackType)
                    {
                        case CallEventMessage.CallbackEvent.Flush:
                            LoadInitialTasks();
                            break;
                    }
                }
            });
        }


        private void DeleteTask(TaskItem item)
        {
            switch (item.TaskType)
            {
                case "Folder Watcher Task":
                    _taskRepository.DeleteTask<FolderWatcherTask>(item.Id);
                    break;
                case "File Compression Task":
                    _taskRepository.DeleteTask<FileCompressionTask>(item.Id);
                    break;
                case "File Backup System Task":
                    _taskRepository.DeleteTask<FileBackupSystemTask>(item.Id);
                    break;
                case "Email Notification Task":
                    _taskRepository.DeleteTask<EmailNotificationTask>(item.Id);
                    break;
            }
        }


        private void LoadInitialTasks()
        {
            try
            {
                // load all task from DB
                var dbTasks = _taskRepository.GetAllTasks();
                Tasks?.Clear();
                foreach (var task in ConvertDbTasks(dbTasks))
                {
                    // Test Task list style
                    // task.Status = StatusType.Completed;
                    Tasks?.Add(task);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load Task Faild: {ex.Message}");
            }
        }



        private List<TaskItem> ConvertDbTasks(IEnumerable<BaseTask> dbTasks)
        {
            var result = new List<TaskItem>();

            foreach (var dbTask in dbTasks)
            {
                DateTime executionTime;
                if (dbTask.ExecutionTime.OnceExecutionTime == null)
                    executionTime = (DateTime)dbTask.ExecutionTime.NextExecutionTime;
                else
                    executionTime = (DateTime)dbTask.ExecutionTime.OnceExecutionTime;//OneTime
                var taskItem = new TaskItem
                {
                    Id = dbTask.Id,
                    Name = dbTask.Name,
                    ExecutionTime = executionTime,
                    Priority = dbTask.Priority.ToString(),
                    Status = dbTask.Status
                };

                switch (dbTask)
                {
                    case FolderWatcherTask folderTask:
                        taskItem.TaskType = "Folder Watcher Task";
                        taskItem.FileDirectory = folderTask.FolderDirectory;
                        break;

                    case FileCompressionTask compTask:
                        taskItem.TaskType = "File Compression Task";
                        taskItem.FileDirectory = compTask.FileDirectory;
                        break;

                    case FileBackupSystemTask backupTask:
                        taskItem.TaskType = "File Backup System Task";
                        taskItem.SourceDirectory = backupTask.SourceDirectory;
                        taskItem.TargetDirectory = backupTask.TargetDirectory;
                        break;

                    case EmailNotificationTask emailTask:
                        taskItem.TaskType = "Email Notification Task";
                        taskItem.SenderEmail = emailTask.SenderEmail;
                        taskItem.ReceiverEmail = emailTask.RecipientEmail;
                        taskItem.EmailSubject = emailTask.Subject;
                        taskItem.EmailBody = emailTask.MessageBody;
                        break;
                }

                result.Add(taskItem);
            }

            return result;
        }


        #region Propertys
        private ObservableCollection<TaskItem>? tasks = new();

        public ObservableCollection<TaskItem>? Tasks
        {
            get { return tasks; }
            set { SetProperty(ref tasks, value); }
        }
        #endregion

        #region Command


        #endregion

    }
}
