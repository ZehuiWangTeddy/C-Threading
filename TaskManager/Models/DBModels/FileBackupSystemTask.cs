using TaskManager.Models.Enums;

namespace TaskManager.Models.DBModels;
using System;
using System.ComponentModel.DataAnnotations.Schema;

public class FileBackupSystemTask : BaseTask
{
    public string? TargetDirectory { get; set; } 
    public string? SourceDirectory { get; set; }  

  
    public FileBackupSystemTask() : base("", PriorityType.Medium) {}

    public FileBackupSystemTask(string name, int executionTimeId, PriorityType priority, string targetDirectory, string sourceDirectory) 
        : base(name, priority)
    {
        TargetDirectory = targetDirectory;
        SourceDirectory = sourceDirectory;
        ExecutionTimeId = executionTimeId;
    }

    public override void Execute()
    {
        Console.WriteLine($"Copying from {SourceDirectory} to {TargetDirectory}");
    }
}