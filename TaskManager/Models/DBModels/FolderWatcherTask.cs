using TaskManager.Models.Enums;

namespace TaskManager.Models.DBModels;

public class FolderWatcherTask : BaseTask
{
    public FolderWatcherTask() : base("", PriorityType.Medium)
    {
    }

    public FolderWatcherTask(string name, int executionTimeId, PriorityType priority, string folderDirectory)
        : base(name, priority)
    {
        FolderDirectory = folderDirectory;
        ExecutionTimeId = executionTimeId;
    }

    public string? FolderDirectory { get; set; }

    public override void Execute()
    {
        var date = DateTime.Now;
        var message = $"{date}: Folder Watcher Task: Monitoring folder {FolderDirectory} for changes";
        Logger.AddLogMessage(message);
    }
}