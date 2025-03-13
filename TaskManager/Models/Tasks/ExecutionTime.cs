namespace TaskManager.Models.Tasks;

public class ExecutionTime
{
   private DateTime OnceExecutionTime { get; set; }
   private RecurrencePattern RecurrencePattern { get; set; }
   private int IntervalInMinutes { get; set; }
   private DateTime NextExecutionTime { get; set; }


   public ExecutionTime(DateTime onceExecutionTime, RecurrencePattern recurrencePattern, int intervalInMinutes, DateTime nextExecutionTime)
   {
      OnceExecutionTime = onceExecutionTime;
      RecurrencePattern = recurrencePattern;
      IntervalInMinutes = intervalInMinutes;
      NextExecutionTime = nextExecutionTime;
   }
}