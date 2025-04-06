using System.ComponentModel;
using System.Runtime.CompilerServices;
using TaskManager.Models.Enums;

namespace TaskManager.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string TaskType { get; set; }     
        public DateTime ExecutionTime { get; set; } 
        public string FileDirectory { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        // public DateTime ExecutionTime { get; set; }
        public string RecurrencePattern { get; set; }
        // public int IntervalInMinutes { get; set; }
        public DateTime NextRunTime { get; set; }
        public string Priority { get; set; }
        public StatusType Status { get; set; }

        public bool IsOne { get; set; }


        private string _countdown;
        public string Countdown
        {
            get => _countdown;
            set
            {
                if (_countdown != value)
                {
                    _countdown = value;
                    OnPropertyChanged(nameof(Countdown));
                }
            }
        }

        public void UpdateCountdown()
        {
            var now = DateTime.Now;
            var timeUntilExecution = ExecutionTime - now;

            if (timeUntilExecution.TotalSeconds < 0)
            {
                Countdown = "Overdue";
                return;
            }

            if (timeUntilExecution.TotalDays >= 1)
            {
                Countdown = $"{timeUntilExecution.Days}d {timeUntilExecution.Hours}h";
            }
            else if (timeUntilExecution.TotalHours >= 1)
            {
                Countdown = $"{timeUntilExecution.Hours}h {timeUntilExecution.Minutes}m";
            }
            else if (timeUntilExecution.TotalMinutes >= 1)
            {
                Countdown = $"{timeUntilExecution.Minutes}m {timeUntilExecution.Seconds}s";
            }
            else
            {
                Countdown = $"{timeUntilExecution.Seconds}s";
            }
           
        }
    }
}