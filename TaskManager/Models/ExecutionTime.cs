using System;
using SQLite;
using SQLiteNetExtensions.Attributes;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;

namespace TaskManager.Models
{
    public class ExecutionTime
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime? OnceExecutionTime { get; set; }
        public RecurrencePattern? RecurrencePattern { get; set; }
        public int? IntervalInMinutes { get; private set; }
        public DateTime? NextExecutionTime { get; set; }

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

            // Calculate next execution time based on the interval
            NextExecutionTime = NextExecutionTime.Value.AddMinutes(IntervalInMinutes.Value);

            // If the calculated time is in the past, adjust it to the future
            if (NextExecutionTime <= now)
            {
                var timeDiff = now - NextExecutionTime.Value;
                var intervalsToAdd = (int)Math.Ceiling(timeDiff.TotalMinutes / IntervalInMinutes.Value);
                NextExecutionTime = now.AddMinutes(intervalsToAdd * IntervalInMinutes.Value);
            }
        }
    }
}