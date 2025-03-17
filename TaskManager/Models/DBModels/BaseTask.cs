using TaskManager.Models.Enums;
using SQLite;

namespace TaskManager.Models.DBModels;


public abstract class BaseTask
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } 
    
    public string Name { get; set; }
    
    [Ignore]
    public ExecutionTime ExecutionTime { get; set; }
    
    public PriorityType Priority { get; set; }
    
    public StatusType Status { get; private set; } = StatusType.Pending;
    public int? ThreadId { get; set; }
    
    [Ignore]
    public TaskLogger TaskLogger { get; } = new TaskLogger();

    protected BaseTask(string name, ExecutionTime executionTime, PriorityType priority)
    {
        Name = name;
        ExecutionTime = executionTime;
        Priority = priority;
    }

    public abstract void Execute();
    
    public void SetStatus(StatusType status)
    {
        Status = status;
        // TaskLogger.Logs.Add($"{DateTime.Now}: Task {Name} changed to {status}");
    }
}