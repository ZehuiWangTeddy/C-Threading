using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;
using SQLiteNetExtensions.Attributes;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;

namespace TaskManager.Models
{
    public class ExecutionTime : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private DateTime? _onceExecutionTime;
        public DateTime? OnceExecutionTime 
        { 
            get => _onceExecutionTime;
            set
            {
                if (_onceExecutionTime != value)
                {
                    _onceExecutionTime = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private RecurrencePattern? _recurrencePattern;
        public RecurrencePattern? RecurrencePattern 
        { 
            get => _recurrencePattern;
            set
            {
                if (_recurrencePattern != value)
                {
                    _recurrencePattern = value;
                    OnPropertyChanged();
                    SetIntervalBasedOnPattern();
                }
            }
        }
        
        private int? _intervalInMinutes;
        public int? IntervalInMinutes 
        { 
            get => _intervalInMinutes;
            set
            {
                if (_intervalInMinutes != value)
                {
                    _intervalInMinutes = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private DateTime? _nextExecutionTime;
        public DateTime? NextExecutionTime 
        { 
            get => _nextExecutionTime;
            set
            {
                if (_nextExecutionTime != value)
                {
                    _nextExecutionTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ExecutionTime() { }

        public ExecutionTime(DateTime? onceExecutionTime, RecurrencePattern? recurrencePattern, DateTime? nextExecutionTime)
        {
            OnceExecutionTime = onceExecutionTime;
            RecurrencePattern = recurrencePattern;
            NextExecutionTime = nextExecutionTime;
            SetIntervalBasedOnPattern();
        }

        private void SetIntervalBasedOnPattern()
        {
            if (RecurrencePattern == null) return;

            IntervalInMinutes = RecurrencePattern switch
            {
                TaskManager.Models.Enums.RecurrencePattern.OneTime => null,  
                TaskManager.Models.Enums.RecurrencePattern.Minute => 1,
                TaskManager.Models.Enums.RecurrencePattern.Hourly => 60,
                TaskManager.Models.Enums.RecurrencePattern.Daily => 1440,    
                TaskManager.Models.Enums.RecurrencePattern.Weekly => 10080, 
                TaskManager.Models.Enums.RecurrencePattern.Monthly => 43200, 
                _ => null
            };
        }

        public void CalculateNextExecutionTime()
        {
            if (RecurrencePattern == null || IntervalInMinutes == null) return;

            var now = DateTime.Now;
            if (NextExecutionTime == null)
            {
                NextExecutionTime = now.AddMinutes(IntervalInMinutes.Value);
                return;
            }
            
            NextExecutionTime = NextExecutionTime.Value.AddMinutes(IntervalInMinutes.Value);

           
            if (NextExecutionTime <= now)
            {
                var timeDiff = now - NextExecutionTime.Value;
                var intervalsToAdd = (int)Math.Ceiling(timeDiff.TotalMinutes / IntervalInMinutes.Value);
                NextExecutionTime = now.AddMinutes(intervalsToAdd * IntervalInMinutes.Value);
            }
        }
        
        public override string ToString()
        {
            if (RecurrencePattern == Enums.RecurrencePattern.OneTime)
            {
                return $"{OnceExecutionTime?.ToString("g") ?? "N/A"}";
            }
            else
            {
                return $"{NextExecutionTime?.ToString("g") ?? "N/A"} ({RecurrencePattern?.ToString() ?? "None"})";
            }
        }

    }
}