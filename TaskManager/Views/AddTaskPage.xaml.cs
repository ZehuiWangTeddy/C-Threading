using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TaskManager.Models;

namespace TaskManager.Views
{
    public partial class AddTaskPage : ContentPage, INotifyPropertyChanged
    {
        public event EventHandler<TaskItem> TaskAdded;

        private string _selectedTaskType;
        public string SelectedTaskType
        {
            get => _selectedTaskType;
            set
            {
                if (_selectedTaskType != value)
                {
                    _selectedTaskType = value;
                    OnPropertyChanged(nameof(SelectedTaskType));
                    UpdateDynamicFieldsVisibility();
                    Debug.WriteLine($"Selected Task Type: {value}");
                }
            }
        }

        private string _taskName;
        public string TaskName
        {
            get => _taskName;
            set
            {
                _taskName = value;
                OnPropertyChanged(nameof(TaskName));
            }
        }

        // Dynamic Fields Visibility
        public bool IsFolderWatcherVisible => SelectedTaskType == "Folder Watcher Task";
        public bool IsFileCompressionVisible => SelectedTaskType == "File Compression Task";
        public bool IsFileBackupVisible => SelectedTaskType == "File Backup System Task";
        public bool IsEmailNotificationVisible => SelectedTaskType == "Email Notification Task";

        // Task Type Specific Fields
        private string _fileDirectory;
        public string FileDirectory
        {
            get => _fileDirectory;
            set
            {
                _fileDirectory = value;
                OnPropertyChanged();
            }
        }

        private string _sourceDirectory;
        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                _sourceDirectory = value;
                OnPropertyChanged();
            }
        }

        private string _targetDirectory;
        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                _targetDirectory = value;
                OnPropertyChanged();
            }
        }

        private string _senderEmail;
        public string SenderEmail
        {
            get => _senderEmail;
            set
            {
                _senderEmail = value;
                OnPropertyChanged();
            }
        }

        private string _receiverEmail;
        public string ReceiverEmail
        {
            get => _receiverEmail;
            set
            {
                _receiverEmail = value;
                OnPropertyChanged();
            }
        }

        private string _emailSubject;
        public string EmailSubject
        {
            get => _emailSubject;
            set
            {
                _emailSubject = value;
                OnPropertyChanged();
            }
        }

        private string _emailBody;
        public string EmailBody
        {
            get => _emailBody;
            set
            {
                _emailBody = value;
                OnPropertyChanged();
            }
        }

        // Execution Time Properties
        private DateTime _executionDate = DateTime.Now;
        public DateTime ExecutionDate
        {
            get => _executionDate;
            set
            {
                _executionDate = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _executionTime = DateTime.Now.TimeOfDay;
        public TimeSpan ExecutionTime
        {
            get => _executionTime;
            set
            {
                _executionTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime _nextRunDate = DateTime.Now;
        public DateTime NextRunDate
        {
            get => _nextRunDate;
            set
            {
                _nextRunDate = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _nextRunTimeSpan = DateTime.Now.TimeOfDay;
        public TimeSpan NextRunTimeSpan
        {
            get => _nextRunTimeSpan;
            set
            {
                _nextRunTimeSpan = value;
                OnPropertyChanged();
            }
        }

        private string _recurrencePattern = "OneTime";
        public string RecurrencePattern
        {
            get => _recurrencePattern;
            set
            {
                _recurrencePattern = value;
                OnPropertyChanged();
                UpdateTimeFieldsVisibility();
            }
        }

        public bool ShowInterval => 
            RecurrencePattern == "Daily" || 
            RecurrencePattern == "Weekly" || 
            RecurrencePattern == "Monthly";

        public bool ShowNextRunTime => RecurrencePattern != "OneTime";

        private int _intervalInMinutes;
        public int IntervalInMinutes
        {
            get => _intervalInMinutes;
            set
            {
                _intervalInMinutes = value;
                OnPropertyChanged();
            }
        }

        // Priority
        private string _priority = "Medium";
        public string Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; }

        public AddTaskPage()
        {
            InitializeComponent();
            BindingContext = this;

            ConfirmCommand = new Command(() =>
            {
                if (ValidateInput())
                {
                    TaskAdded?.Invoke(this, CreateTaskItem());
                    Navigation.PopModalAsync();
                }
            });
        }

        private bool ValidateInput()
        {
            if (ShowInterval && IntervalInMinutes <= 0)
            {
                return ShowError("Interval must be greater than 0");
            }
            
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                DisplayAlert("Error", "Task name is required", "OK");
                return false;
            }

            switch (SelectedTaskType)
            {
                case "Folder Watcher Task":
                case "File Compression Task":
                    if (string.IsNullOrWhiteSpace(FileDirectory))
                        return ShowError("File directory is required");
                    break;

                case "File Backup System Task":
                    if (string.IsNullOrWhiteSpace(SourceDirectory) || 
                        string.IsNullOrWhiteSpace(TargetDirectory))
                        return ShowError("Source and Target directories are required");
                    break;

                case "Email Notification Task":
                    if (string.IsNullOrWhiteSpace(SenderEmail) || 
                        string.IsNullOrWhiteSpace(ReceiverEmail) ||
                        string.IsNullOrWhiteSpace(EmailSubject))
                        return ShowError("Email fields are required");
                    break;
            }

            return true;
        }

        private bool ShowError(string message)
        {
            DisplayAlert("Validation Error", message, "OK");
            return false;
        }

        private TaskItem CreateTaskItem()
        {
            return new TaskItem
            {
                Name = TaskName,
                TaskType = SelectedTaskType,
                FileDirectory = FileDirectory,
                SourceDirectory = SourceDirectory,
                TargetDirectory = TargetDirectory,
                SenderEmail = SenderEmail,
                ReceiverEmail = ReceiverEmail,
                EmailSubject = EmailSubject,
                EmailBody = EmailBody,
                ExecutionTime = ExecutionDate.Date.Add(ExecutionTime),
                RecurrencePattern = RecurrencePattern,
                NextRunTime = NextRunDate.Date.Add(NextRunTimeSpan),
                Priority = Priority
            };
        }

        private void UpdateDynamicFieldsVisibility()
        {
            OnPropertyChanged(nameof(IsFolderWatcherVisible));
            OnPropertyChanged(nameof(IsFileCompressionVisible));
            OnPropertyChanged(nameof(IsFileBackupVisible));
            OnPropertyChanged(nameof(IsEmailNotificationVisible));
        }

        private void UpdateTimeFieldsVisibility()
        {
            OnPropertyChanged(nameof(ShowInterval));
            OnPropertyChanged(nameof(ShowNextRunTime));
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}