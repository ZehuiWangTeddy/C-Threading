using TaskManager.Models.Enums;

namespace TaskManager.Models.DBModels;

public class FileCompressionTask : BaseTask
{
    public FileCompressionTask() : base("", PriorityType.Medium)
    {
    }

    public FileCompressionTask(string name, int executionTimeId, PriorityType priority, string fileDirectory)
        : base(name, priority)
    {
        FileDirectory = fileDirectory;
        ExecutionTimeId = executionTimeId;
    }

    public string? FileDirectory { get; set; }

    public override void Execute()
    {
        var date = DateTime.Now;
        var message = $"{date}: File Compression Task: Compressing files in directory {FileDirectory}";
        Logger.AddLogMessage(message);
    }
}