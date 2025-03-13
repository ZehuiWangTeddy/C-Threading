namespace TaskManager.Models.Tasks;
using System;

public class FolderWatcherTask : BaseTask
{
    private string FolderDirectory { get; set; }

    public FolderWatcherTask(int id, string name, ExecutionTime executionTime, PriorityType priority,
        string folderDirectory) : base(id, name, executionTime, priority)
    {
        this.FolderDirectory = folderDirectory;
    }
    
    public override void Execute()
    {
        Console.WriteLine($" Watching {FolderDirectory}");
    }
}