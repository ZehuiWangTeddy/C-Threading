using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.Services;
using TaskManager.Extensions;

namespace TaskManager.Views
{
    public partial class AddTaskPage : ContentPage, INotifyPropertyChanged
    {
        private readonly ITaskRepository _taskRepository;

        public event EventHandler<TaskItem> TaskAdded;

        #region Bindable Properties
        private string _selectedTaskType;
        public string SelectedTaskType
        {
            get => _selectedTaskType;
            set
            {
                _selectedTaskType = value;
                OnPropertyChanged(nameof(SelectedTaskType));
                UpdateDynamicFieldsVisibility();
                Debug.WriteLine($"Selected Task Type: {value}");
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
                CalculateNextRunTime();
            }
        }

        private TimeSpan _executionTime = DateTime.Now.TimeOfDay;
        public TimeSpan ExecutionTimeSpan
        {
            get => _executionTime;
            set
            {
                _executionTime = value;
                OnPropertyChanged();
                CalculateNextRunTime();
            }
        }

        // public static class DateTimeExtensions
        // {
        //     public static DateTime RoundToMinutes(this DateTime dt)
        //     {
        //         return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
        //     }
        // }
        
        private DateTime _nextRunDate;
        public DateTime NextRunDate
        {
            get => _nextRunDate;
            private set
            {
                _nextRunDate = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _nextRunTimeSpan;
        public TimeSpan NextRunTimeSpan
        {
            get => _nextRunTimeSpan;
            private set
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
                CalculateNextRunTime();
            }
        }

        public bool ShowNextRunTime => RecurrencePattern != "OneTime";

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
        #endregion

        public ICommand ConfirmCommand { get; }

        public AddTaskPage(ITaskRepository taskRepository)
        {
            InitializeComponent();
            _taskRepository = taskRepository;
            BindingContext = this;

            ConfirmCommand = new Command(async () =>
            {
                if (ValidateInput())
                {
                    await SaveTaskToDatabase();
                    await Navigation.PopModalAsync();
                }
            });
        }

        #region Database Operations
        private async Task SaveTaskToDatabase()
        {
            try
            {
                // 1. Create executionTime
                var executionTime = new ExecutionTime(
                    onceExecutionTime: RecurrencePattern == "OneTime" ? ExecutionDate.Date.Add(ExecutionTimeSpan) : null,
                    recurrencePattern: Enum.Parse<RecurrencePattern>(RecurrencePattern),
                    intervalInMinutes: null,
                    nextExecutionTime: NextRunDate.Date.Add(NextRunTimeSpan)
                );

                _taskRepository.SaveExecutionTime(executionTime);

                // 2. Create Task type
                var baseTask = TaskConverter.ConvertToDbTask(new TaskItem
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
                    ExecutionTime = ExecutionDate.Date.Add(ExecutionTimeSpan),
                    RecurrencePattern = RecurrencePattern,
                    NextRunTime = NextRunDate.Date.Add(NextRunTimeSpan),
                    Priority = Priority
                }, executionTime.Id);
                
                _taskRepository.SaveTask(baseTask);
                
                TaskAdded?.Invoke(this, new TaskItem
                {
                    Name = TaskName,
                    ExecutionTime = ExecutionDate.Date.Add(ExecutionTimeSpan)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Save Faild: {ex.Message}", "OK");
            }
        }
        #endregion

        #region Time Calculation
        private void CalculateNextRunTime()
        {
            if (string.IsNullOrEmpty(RecurrencePattern)) return;

            var baseTime = ExecutionDate.Date.Add(ExecutionTimeSpan);
    
            switch (RecurrencePattern)
            {
                case "Minutely":
                    UpdateNextRun(baseTime.AddMinutes(1));
                    break;
                case "Hourly":
                    UpdateNextRun(baseTime.AddHours(1));
                    break;
                case "Daily":
                    UpdateNextRun(baseTime.AddDays(1));
                    break;
                case "Weekly":
                    UpdateNextRun(baseTime.AddDays(7));
                    break;
                case "Monthly":
                    UpdateNextRun(baseTime.AddMonths(1));
                    break;
                default:
                    NextRunDate = DateTime.MinValue;
                    NextRunTimeSpan = TimeSpan.Zero;
                    break;
            }
        }
        #endregion

        #region Validation
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TaskName))
            {
                DisplayAlert("Error", "Please input task name", "OK");
                return false;
            }
            
            var executionDateTime = ExecutionDate.Date.Add(ExecutionTimeSpan);
            var now = DateTime.Now.RoundToMinutes();
            
            if (executionDateTime <= DateTime.Now)
            {
                DisplayAlert("Error", "Execution time cannot early than now", "OK");
                return false;
            }
            
            if (ShowNextRunTime)
            {
                var nextRunDateTime = NextRunDate.Date.Add(NextRunTimeSpan);
                if (nextRunDateTime <= executionDateTime)
                {
                    DisplayAlert("Error", "Next run time must late than execution time", "OK");
                    return false;
                }
            }
            
            switch (SelectedTaskType)
            {
                case "Folder Watcher Task":
                case "File Compression Task":
                    if (string.IsNullOrWhiteSpace(FileDirectory))
                    {
                        DisplayAlert("Error", "Input File Directory ", "OK");
                        return false;
                    }
                    break;

                case "File Backup System Task":
                    if (string.IsNullOrWhiteSpace(SourceDirectory) || string.IsNullOrWhiteSpace(TargetDirectory))
                    {
                        DisplayAlert("Error", "Input SourceDirectory and TargetDirectory", "OK");
                        return false;
                    }
                    break;

                case "Email Notification Task":
                    if (string.IsNullOrWhiteSpace(SenderEmail) || 
                        string.IsNullOrWhiteSpace(ReceiverEmail) ||
                        string.IsNullOrWhiteSpace(EmailSubject))
                    {
                        DisplayAlert("Error", "Input SenderEmail, ReceiverEmail and EmailSubject", "OK");
                        return false;
                    }
                    break;
            }

            return true;
        }
        #endregion

        #region Helper Methods
        private void UpdateDynamicFieldsVisibility()
        {
            OnPropertyChanged(nameof(IsFolderWatcherVisible));
            OnPropertyChanged(nameof(IsFileCompressionVisible));
            OnPropertyChanged(nameof(IsFileBackupVisible));
            OnPropertyChanged(nameof(IsEmailNotificationVisible));
        }
        
        private void UpdateTimeFieldsVisibility()
        {
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
        #endregion

        private void UpdateNextRun(DateTime nextTime)
        {
            NextRunDate = nextTime.Date;
            NextRunTimeSpan = nextTime.TimeOfDay.RoundToMinutes();
            OnPropertyChanged(nameof(NextRunDate));
            OnPropertyChanged(nameof(NextRunTimeSpan));
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}