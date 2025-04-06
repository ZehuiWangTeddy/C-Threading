using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TaskManager.Extensions;
using TaskManager.Messages;
using TaskManager.Models;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.Services;
using TaskManager.Views;

namespace TaskManager.ViewModels
{
    public class AddTaskViewModel : ObservableObject
    {
        private readonly ITaskRepository _taskRepository;
        public AddTaskViewModel(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;

            ConfirmCommand = new RelayCommand(ConfirmAsync);
            TimeChangedCommand = new RelayCommand(TimeChanged);
            
        }

     

        private void TimeChanged()
        {
            CalculateNextRunTime();
        }

        private async void ConfirmAsync()
        {
            if (ValidateInput())
            {
                await SaveTaskToDatabase();

                WeakReferenceMessenger.Default.Send(new NavigationMessage(nameof(AddTaskViewModel), nameof(AddTaskPage), true));

            }
        }

        #region Bindable Properties
        private string _selectedTaskType;
        public string SelectedTaskType
        {
            get => _selectedTaskType;
            set
            {
                SetProperty(ref _selectedTaskType, value);
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
                SetProperty(ref _taskName, value);

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
                SetProperty(ref _fileDirectory, value);
            }
        }

        private string _sourceDirectory;
        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                SetProperty(ref _sourceDirectory, value);
            }
        }

        private string _targetDirectory;
        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                SetProperty(ref _targetDirectory, value);
            }
        }

        private string _senderEmail;
        public string SenderEmail
        {
            get => _senderEmail;
            set
            {
                SetProperty(ref _senderEmail, value);
            }
        }

        private string _receiverEmail;
        public string ReceiverEmail
        {
            get => _receiverEmail;
            set
            {
                SetProperty(ref _receiverEmail, value);
            }
        }

        private string _emailSubject;
        public string EmailSubject
        {
            get => _emailSubject;
            set
            {
                SetProperty(ref _emailSubject, value);
            }
        }

        private string _emailBody;
        public string EmailBody
        {
            get => _emailBody;
            set
            {
                SetProperty(ref _emailBody, value);
            }
        }

        // Execution Time Properties
        private DateTime _executionDate = DateTime.Now;
        public DateTime ExecutionDate
        {
            get => _executionDate;
            set
            {
                SetProperty(ref _executionDate, value);
                //CalculateNextRunTime();
            }
        }

        private TimeSpan _executionTime = DateTime.Now.TimeOfDay;
        public TimeSpan ExecutionTimeSpan
        {
            get => _executionTime;
            set
            {
                SetProperty(ref _executionTime, value);
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
                SetProperty(ref _nextRunTimeSpan, value);
            }
        }

        private string _recurrencePattern = "OneTime";
        public string RecurrencePattern
        {
            get => _recurrencePattern;
            set
            {
                SetProperty(ref _recurrencePattern, value);
                UpdateTimeFieldsVisibility();
                //CalculateNextRunTime();
            }
        }

        public bool ShowNextRunTime => RecurrencePattern != "OneTime";

        private string _priority = "Medium";
        public string Priority
        {
            get => _priority;
            set
            {
                SetProperty(ref _priority, value);

            }
        }
        #endregion

        #region Database Operations
        private async Task SaveTaskToDatabase()
        {
            try
            {
                var executionTime = new ExecutionTime(
                    onceExecutionTime: RecurrencePattern == "OneTime" ? ExecutionDate.Date.Add(ExecutionTimeSpan) : null,
                    recurrencePattern: Enum.Parse<RecurrencePattern>(RecurrencePattern),
                    nextExecutionTime: NextRunDate.Date.Add(NextRunTimeSpan)
                );

                _taskRepository.SaveExecutionTime(executionTime);

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


            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ToastMessage(nameof(AddTaskViewModel), nameof(AddTaskPage), $"Save Faild: {ex.Message}"));
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
                case "Minute":
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
                WeakReferenceMessenger.Default.Send(new ToastMessage(nameof(AddTaskViewModel), nameof(AddTaskPage), "Please input task name"));
                return false;
            }

            var executionDateTime = ExecutionDate.Date.Add(ExecutionTimeSpan);
            var now = DateTime.Now.RoundToMinutes();

            if (executionDateTime <= DateTime.Now)
            {
                WeakReferenceMessenger.Default.Send(new ToastMessage(nameof(AddTaskViewModel), nameof(AddTaskPage), "Execution time cannot early than now"));
                return false;
            }

            if (ShowNextRunTime)
            {
                var nextRunDateTime = NextRunDate.Date.Add(NextRunTimeSpan);
                if (nextRunDateTime <= executionDateTime)
                {
                    WeakReferenceMessenger.Default.Send(new ToastMessage(nameof(AddTaskViewModel), nameof(AddTaskPage), "Next run time must late than execution time"));
                    return false;
                }
            }

            switch (SelectedTaskType)
            {
                case "Folder Watcher Task":
                case "File Compression Task":
                    if (string.IsNullOrWhiteSpace(FileDirectory))
                    {
                        WeakReferenceMessenger.Default.Send(new ToastMessage(nameof(AddTaskViewModel), nameof(AddTaskPage), "Input File Directory"));
                        return false;
                    }
                    break;

                case "File Backup System Task":
                    if (string.IsNullOrWhiteSpace(SourceDirectory) || string.IsNullOrWhiteSpace(TargetDirectory))
                    {
                        WeakReferenceMessenger.Default.Send(new ToastMessage(nameof(AddTaskViewModel), nameof(AddTaskPage), "Input SourceDirectory and TargetDirectory"));
                        return false;
                    }
                    break;

                case "Email Notification Task":
                    if (string.IsNullOrWhiteSpace(SenderEmail) ||
                        string.IsNullOrWhiteSpace(ReceiverEmail) ||
                        string.IsNullOrWhiteSpace(EmailSubject))
                    {
                        WeakReferenceMessenger.Default.Send(new ToastMessage(nameof(AddTaskViewModel), nameof(AddTaskPage), "Input SenderEmail, ReceiverEmail and EmailSubject"));
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
                await AppShell.Current.Navigation.PopModalAsync();

            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ToastMessage(nameof(AddTaskViewModel), nameof(AddTaskPage), ex.Message));
            }
        }
        #endregion

        private  void UpdateNextRun(DateTime nextTime)
        {
            NextRunDate = nextTime.Date;
            NextRunTimeSpan = nextTime.TimeOfDay.RoundToMinutes();
        }




        public ICommand TimeChangedCommand { get; private set; }
        public ICommand ConfirmCommand { get; private set; }
    }
}