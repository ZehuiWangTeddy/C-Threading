using TaskManager.Models.Enums;
namespace TaskManager.Models.DBModels;
using System;

public class FolderWatcherTask : BaseTask
{
    public string? FolderDirectory { get; set; } 
    
    public FolderWatcherTask() : base("", PriorityType.Medium) {}

    public FolderWatcherTask(string name, int executionTimeId, PriorityType priority, string folderDirectory) 
        : base(name, priority)
    {
        FolderDirectory = folderDirectory;
        ExecutionTimeId = executionTimeId;
    }
    
    public override void Execute()
    {
        DateTime date = DateTime.Now;
        var message = $"{date}: Folder Watcher Task: Monitoring folder {FolderDirectory} for changes";
        Logger.AddLogMessage(message);
    }
}