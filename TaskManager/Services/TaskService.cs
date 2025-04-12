using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;

namespace TaskManager.Services;

public class TaskService : IDisposable
{
    private readonly ITaskRepository _taskRepository;
    private readonly TaskScheduler _taskScheduler;
    private readonly ThreadPoolManager _threadPoolManager;
    private bool _disposed;

    public TaskService(ThreadPoolManager threadPoolManager, ITaskRepository taskRepository)
    {
        _threadPoolManager = threadPoolManager ?? throw new ArgumentNullException(nameof(threadPoolManager));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void AddTask(BaseTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        _taskRepository.SaveTask(task);
        Console.WriteLine($"Task added to DB: {task.Name}");
    }

    public List<BaseTask> GetTasks()
    {
        return _taskRepository.GetTasks(StatusType.Pending);
    }

    public void UpdateTaskStatus(int taskId, StatusType status)
    {
        _taskRepository.UpdateTaskStatus(taskId, status);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                // Dispose managed resources
                _threadPoolManager?.Dispose();

            // Clean up unmanaged resources and override finalizer
            _disposed = true;
        }
    }

    public int GetCompletedTasks()
    {
        return _taskRepository.GetTasks(StatusType.Completed).Count;
    }

    public int GetFailedTasks()
    {
        return _taskRepository.GetTasks(StatusType.Failed).Count;
    }
}