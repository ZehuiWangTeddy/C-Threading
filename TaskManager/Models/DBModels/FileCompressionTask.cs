using TaskManager.Models.Enums;

namespace TaskManager.Models.DBModels;
using System;
using System.ComponentModel.DataAnnotations.Schema;

public class FileCompressionTask : BaseTask
{
    public string FileDirectory { get; set; } 
    public FileCompressionTask() : base("", new ExecutionTime(), PriorityType.Medium) {}

    public FileCompressionTask(string name, ExecutionTime executionTime, PriorityType priority, string fileDirectory) 
        : base(name, executionTime, priority)
    {
        FileDirectory = fileDirectory;
    }

    public override void Execute()
    {
        Console.WriteLine($"Compressing files from {FileDirectory}");
    }
}