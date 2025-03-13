namespace TaskManager.Models.Tasks;

public class TaskLogger
{
    private List<string> Logs { get; set; } 

    public TaskLogger()
    {
        this.Logs = new List<string>();
    }
}