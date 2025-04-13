using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;
using SQLiteNetExtensions.Attributes;
using TaskManager.Models.Enums;

namespace TaskManager.Models.DBModels;

public abstract class BaseTask : INotifyPropertyChanged
{
    private DateTime? _createdAt;

    private ExecutionTime _executionTime;

    private DateTime? _lastCompletionTime;

    private TaskLogger _logger;

    private string _name;

    private int _priorityValue;

    private int _statusValue;

    private int? _threadId;

    public BaseTask(string name, PriorityType priority)
    {
        Name = name;
        Priority = priority;
        Status = StatusType.Pending;
        CreatedAt = DateTime.Now;
    }

    [PrimaryKey] [AutoIncrement] public int Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    [ForeignKey(typeof(ExecutionTime))] public int? ExecutionTimeId { get; set; }

    [OneToOne(CascadeOperations = CascadeOperation.All)]
    public ExecutionTime ExecutionTime
    {
        get => _executionTime;
        set
        {
            if (_executionTime != value)
            {
                _executionTime = value;
                OnPropertyChanged();
            }
        }
    }

    [Column("PriorityValue")]
    public int PriorityValue
    {
        get => _priorityValue;
        set
        {
            if (_priorityValue != value)
            {
                _priorityValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Priority));
            }
        }
    }

    [Column("StatusValue")]
    public int StatusValue
    {
        get => _statusValue;
        set
        {
            if (_statusValue != value)
            {
                _statusValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    [Ignore]
    public PriorityType Priority
    {
        get => (PriorityType)PriorityValue;
        set
        {
            if (PriorityValue != (int)value)
            {
                PriorityValue = (int)value;
                OnPropertyChanged();
            }
        }
    }

    [Ignore]
    public StatusType Status
    {
        get => (StatusType)StatusValue;
        set
        {
            if (StatusValue != (int)value)
            {
                StatusValue = (int)value;
                OnPropertyChanged();
            }
        }
    }

    public int? ThreadId
    {
        get => _threadId;
        set
        {
            if (_threadId != value)
            {
                _threadId = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime? CreatedAt
    {
        get => _createdAt;
        set
        {
            if (_createdAt != value)
            {
                _createdAt = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime? LastCompletionTime
    {
        get => _lastCompletionTime;
        set
        {
            if (_lastCompletionTime != value)
            {
                _lastCompletionTime = value;
                OnPropertyChanged();
            }
        }
    }

    [ForeignKey(typeof(TaskLogger))] public int? TaskLoggerId { get; set; }

    [OneToOne(CascadeOperations = CascadeOperation.All)]
    public TaskLogger Logger
    {
        get => _logger;
        set
        {
            if (_logger != value)
            {
                _logger = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime? CompletionTime { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public abstract void Execute();

    public void SetStatus(StatusType status)
    {
        Status = status;
    }
}