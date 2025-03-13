using TaskManager.Models.Tasks;
namespace TaskManager.Models;

public class ThreadPoolManager
{
    private int MaxThreads { get; set; }
    private int ActiveThreads { get; set; }
    private List<BaseTask> TaskQueue { get; set; }

    public ThreadPoolManager(int maxThreads)
    {
        MaxThreads = maxThreads;
        TaskQueue = new List<BaseTask>();
        ActiveThreads = 0;
    }

    public void AddTaskToQueue(BaseTask task)
    {
        TaskQueue.Add(task);
    }
}