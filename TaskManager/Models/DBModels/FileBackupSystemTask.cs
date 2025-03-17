using TaskManager.Models.Enums;

namespace TaskManager.Models.DBModels;
using System;
using System.ComponentModel.DataAnnotations.Schema;

public class FileBackupSystemTask : BaseTask
{
    public string TargetDirectory { get; set; } 
    public string SourceDirectory { get; set; }  

  
    public FileBackupSystemTask() : base("", new ExecutionTime(), PriorityType.Medium) {}

    public FileBackupSystemTask(string name, ExecutionTime executionTime, PriorityType priority, string targetDirectory, string sourceDirectory) 
        : base(name, executionTime, priority)
    {
        TargetDirectory = targetDirectory;
        SourceDirectory = sourceDirectory;
    }

    public override void Execute()
    {
        Console.WriteLine($"Copying from {SourceDirectory} to {TargetDirectory}");
    }
}