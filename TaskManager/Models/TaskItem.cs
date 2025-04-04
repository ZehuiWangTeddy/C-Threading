namespace TaskManager.Models
{
    public class TaskItem
    {
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
        public string Status { get; set; }
    }
}