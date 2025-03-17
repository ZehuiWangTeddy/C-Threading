using TaskManager.Models.Enums;
namespace TaskManager.Models.DBModels;
using System;

public class FolderWatcherTask : BaseTask
{
    public string FolderDirectory { get; set; } 
    
    public FolderWatcherTask() : base("", new ExecutionTime(), PriorityType.Medium) {}

    public FolderWatcherTask(string name, ExecutionTime executionTime, PriorityType priority, string folderDirectory) 
        : base(name, executionTime, priority)
    {
        FolderDirectory = folderDirectory;
    }
    
    public override void Execute()
    {
        Console.WriteLine($"Watching folder: {FolderDirectory}");
    }
}