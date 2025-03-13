namespace TaskManager.Models.Tasks;

public abstract class BaseTask
{
    public int Id { get; }
    public string Name { get; set; }
    public ExecutionTime ExecutionTime { get; set; }
    public PriorityType Priority { get; set; }
    public StatusType Status { get; private set; } = StatusType.Pending;
    public int? ThreadId { get; set; }
    public TaskLogger TaskLogger { get; } = new TaskLogger();


    protected BaseTask(int id, string name, ExecutionTime executionTime, PriorityType priority)
    {
        Id = id;
        Name = name;
        ExecutionTime = executionTime;
        Priority = priority;
    }

    public abstract void Execute();
}