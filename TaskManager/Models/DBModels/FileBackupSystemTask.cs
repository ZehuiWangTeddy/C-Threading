using TaskManager.Models.Enums;

namespace TaskManager.Models.DBModels;

public class FileBackupSystemTask : BaseTask
{
    public FileBackupSystemTask() : base("", PriorityType.Medium)
    {
    }

    public FileBackupSystemTask(string name, int executionTimeId, PriorityType priority, string targetDirectory,
        string sourceDirectory)
        : base(name, priority)
    {
        TargetDirectory = targetDirectory;
        SourceDirectory = sourceDirectory;
        ExecutionTimeId = executionTimeId;
    }

    public string? TargetDirectory { get; set; }
    public string? SourceDirectory { get; set; }

    public override void Execute()
    {
        var date = DateTime.Now;
        var message = $"{date}: File Backup Task: Copying files from {SourceDirectory} to {TargetDirectory}";
        Logger.AddLogMessage(message);
    }
}