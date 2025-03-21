using TaskManager.Models.Enums;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TaskManager.Models.DBModels;

public abstract class BaseTask
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }

    [ForeignKey(typeof(ExecutionTime))]
    public int? ExecutionTimeId { get; set; }

    [OneToOne(CascadeOperations = CascadeOperation.All)]
    public ExecutionTime ExecutionTime { get; set; } 

    public PriorityType Priority { get; set; }

    public StatusType Status { get; private set; } = StatusType.Pending;

    public int? ThreadId { get; set; }

    [ForeignKey(typeof(TaskLogger))]
    public int? TaskLoggerId { get; set; }

    [OneToOne(CascadeOperations = CascadeOperation.All)]
    public TaskLogger Logger { get; set; }

    public BaseTask(string name, PriorityType priority)
    {
        Name = name;
        Priority = priority;
    }

    public abstract void Execute();

    public void SetStatus(StatusType status)
    {
        Status = status;
    }
}