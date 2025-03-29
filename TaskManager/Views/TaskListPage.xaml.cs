using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using TaskManager.Models;
using TaskManager.Repositories;
using TaskManager.Services;
using System.Diagnostics;
using TaskManager.Models.DBModels;

namespace TaskManager.Views
{
    public partial class TaskListPage : ContentPage, INotifyPropertyChanged
    {
        private readonly ITaskRepository _taskRepository;
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public TaskListPage(ITaskRepository taskRepository)
        {
            InitializeComponent();
            BindingContext = this;
            _taskRepository = taskRepository;
            
            LoadInitialTasks();
        }

        private void LoadInitialTasks()
        {
            try
            {
                // load all task from DB
                var dbTasks = _taskRepository.GetAllTasks();
                
                Tasks.Clear();
                foreach (var task in ConvertDbTasks(dbTasks))
                {
                    Tasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load Task Faild: {ex.Message}");
            }
        }

        private async void OnAddTaskClicked(object sender, EventArgs e)
        {
            try
            {
                var addPage = new AddTaskPage(_taskRepository);
                
                addPage.TaskAdded += async (s, task) => 
                {
                    // reload newest task
                    var freshTasks = _taskRepository.GetAllTasks();
                    
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Tasks.Clear();
                        foreach (var t in ConvertDbTasks(freshTasks))
                        {
                            Tasks.Add(t);
                        }
                    });
                };

                await Navigation.PushModalAsync(addPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Open Page Faild: {ex.Message}");
                await DisplayAlert("Error", "Cannot open add task page", "Confirm");
            }
        }

        private void OnStartTask(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TaskItem task)
            {
                DisplayAlert("Task Start", $"Start: {task.Name}", "OK");
            }
        }

        private async void OnCancelTask(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TaskItem task)
            {
                bool confirm = await DisplayAlert("Confirm", $"Do you want to cancle {task.Name} ？", "YES", "NO");
                if (confirm)
                {
                    _taskRepository.DeleteTask(task.Id);
                    
                    Tasks.Remove(task);
                }
            }
        }
        
        private async void OnDetailsClicked(object sender, System.EventArgs e)
        {
            await DisplayAlert("On the way", "Page is cooking", "Confirm");
        }

        private List<TaskItem> ConvertDbTasks(IEnumerable<BaseTask> dbTasks)
        {
            var result = new List<TaskItem>();
            
            foreach (var dbTask in dbTasks)
            {
                var taskItem = new TaskItem
                {
                    Id = dbTask.Id,
                    Name = dbTask.Name,
                    ExecutionTime = dbTask.ExecutionTime?.OnceExecutionTime ?? DateTime.MinValue,
                    Priority = dbTask.Priority.ToString(),
                    Status = dbTask.Status.ToString()
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}