namespace TaskManager.Models.Tasks;
using System;

public class FileBackupSystemTask : BaseTask
{
    private string TargetDirectory { get; set; }
    private string SourceDirectory { get; set; }

    public FileBackupSystemTask(int id, string name, ExecutionTime executionTime, PriorityType priority, string targetDirectory, string sourceDirectory) : base(id, name, executionTime, priority)
    {
        TargetDirectory = targetDirectory;
        SourceDirectory = sourceDirectory;
    }

    public override void Execute()
    {
        Console.WriteLine($" from {SourceDirectory} to {TargetDirectory}");
    }
    
    
}