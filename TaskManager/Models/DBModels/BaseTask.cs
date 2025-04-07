using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TaskManager.Models.Enums;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TaskManager.Models.DBModels;

public abstract class BaseTask : INotifyPropertyChanged
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    private string _name;
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

    [ForeignKey(typeof(ExecutionTime))]
    public int? ExecutionTimeId { get; set; }

    private ExecutionTime _executionTime;
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

    private int _priorityValue;
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
    
    private int _statusValue;
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

    private int? _threadId;
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
    
    private DateTime? _createdAt;
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
    
    private DateTime? _lastCompletionTime;
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

    [ForeignKey(typeof(TaskLogger))]
    public int? TaskLoggerId { get; set; }

    private TaskLogger _logger;
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public BaseTask(string name, PriorityType priority)
    {
        Name = name;
        Priority = priority;
        Status = StatusType.Pending;
        CreatedAt = DateTime.Now;
    }

    public abstract void Execute();

    public void SetStatus(StatusType status)
    {
        Status = status;
    }
}