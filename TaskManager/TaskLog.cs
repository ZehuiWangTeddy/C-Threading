namespace TaskManager;

// Enum for Priority
public enum TaskPriority
{
    High,
    Medium,
    Low
}

// Enum for Status
public enum TaskStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

public class TaskLog
{
    public string TaskName { get; set; }  // Name of the task
    public int ThreadId { get; set; }     // ID of the thread executing the task
    public string ExecutionTime { get; set; }  // How long the task took (e.g., "00:00:03")
    public TaskPriority Priority { get; set; }  // Task priority (High, Medium, Low)
    public TaskStatus Status { get; set; }  // Task status (Pending, Running, Completed, Failed)
    public string ExecutionLog { get; set; }  // Log details of the task execution
}