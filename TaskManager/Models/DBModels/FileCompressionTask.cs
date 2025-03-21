using TaskManager.Models.Enums;

namespace TaskManager.Models.DBModels;
using System;
using System.ComponentModel.DataAnnotations.Schema;

public class FileCompressionTask : BaseTask
{
    public string? FileDirectory { get; set; } 
    public FileCompressionTask() : base("", PriorityType.Medium) {}

    public FileCompressionTask(string name, int executionTimeId, PriorityType priority, string fileDirectory) 
        : base(name, priority)
    {
        FileDirectory = fileDirectory;
        ExecutionTimeId = executionTimeId;
    }

    public override void Execute()
    {
        var message = $"File Compression Task: Compressing files in directory {FileDirectory}";
        Logger.AddLogMessage(message);
        Console.WriteLine(message);
    }
}