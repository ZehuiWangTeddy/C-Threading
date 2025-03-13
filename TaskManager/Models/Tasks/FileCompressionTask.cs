namespace TaskManager.Models.Tasks;
using System;

public class FileCompressionTask: BaseTask
{
    private string FileDirectory { get; set; }

    public FileCompressionTask(int id, string name, ExecutionTime executionTime, PriorityType priority, string fileDirectory) : base(id, name, executionTime, priority)
    {
        this.FileDirectory = fileDirectory;
    }

    public override void Execute()
    {
        Console.WriteLine($" compressing from {FileDirectory}");
    }
}