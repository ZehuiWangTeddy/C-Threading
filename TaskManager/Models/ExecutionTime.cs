using SQLite;
using TaskManager.Models.Enums;

namespace TaskManager.Models
{
    public class ExecutionTime
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } 
        public DateTime OnceExecutionTime { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public int IntervalInMinutes { get; set; }
        public DateTime NextExecutionTime { get; set; }
        
        public ExecutionTime() { }

        public ExecutionTime(DateTime onceExecutionTime, RecurrencePattern recurrencePattern, int intervalInMinutes, DateTime nextExecutionTime)
        {
            OnceExecutionTime = onceExecutionTime;
            RecurrencePattern = recurrencePattern;
            IntervalInMinutes = intervalInMinutes;
            NextExecutionTime = nextExecutionTime;
        }
    }
}