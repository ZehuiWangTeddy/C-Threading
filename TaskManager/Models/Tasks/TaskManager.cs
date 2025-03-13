namespace TaskManager.Models.Tasks;

public class TaskManager
{
    private List<Task> Tasks { get; set; }
    private ThreadPoolManager? ThreadPoolManager { get; set; }

    public TaskManager()
    {
        this.Tasks = new List<Task>();
    }

    public void AddTask(Task task)
    {
        this.Tasks.Add(task);
    }
}