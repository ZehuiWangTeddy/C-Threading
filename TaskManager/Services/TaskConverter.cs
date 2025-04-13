using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;

namespace TaskManager.Services;

public static class TaskConverter
{
    public static BaseTask ConvertToDbTask(TaskItem taskItem, int executionTimeId)
    {
        var taskLogger = CreateTaskLogger();

        return taskItem.TaskType switch
        {
            "Folder Watcher Task" => new FolderWatcherTask(
                taskItem.Name,
                executionTimeId,
                Enum.Parse<PriorityType>(taskItem.Priority),
                taskItem.FileDirectory)
            {
                ExecutionTime = CreateExecutionTime(taskItem),

                Logger = taskLogger
            },

            "File Compression Task" => new FileCompressionTask(
                taskItem.Name,
                executionTimeId,
                Enum.Parse<PriorityType>(taskItem.Priority),
                taskItem.FileDirectory)
            {
                Logger = taskLogger
            },

            "File Backup System Task" => new FileBackupSystemTask(
                taskItem.Name,
                executionTimeId,
                Enum.Parse<PriorityType>(taskItem.Priority),
                taskItem.TargetDirectory,
                taskItem.SourceDirectory)
            {
                Logger = taskLogger
            },

            "Email Notification Task" => new EmailNotificationTask(
                taskItem.Name,
                executionTimeId,
                Enum.Parse<PriorityType>(taskItem.Priority),
                taskItem.SenderEmail,
                taskItem.ReceiverEmail,
                taskItem.EmailSubject,
                taskItem.EmailBody)
            {
                Logger = taskLogger
            },

            _ => throw new ArgumentException("Invalid task type")
        };
    }

    public static ExecutionTime CreateExecutionTime(TaskItem taskItem)
    {
        var recurrencePattern = Enum.Parse<RecurrencePattern>(taskItem.RecurrencePattern);
        if (recurrencePattern == RecurrencePattern.OneTime)
            return new ExecutionTime(
                taskItem.ExecutionTime,
                recurrencePattern,
                null);

        return new ExecutionTime(
            null,
            recurrencePattern,
            taskItem.NextRunTime);
    }

    public static TaskLogger CreateTaskLogger()
    {
        return new TaskLogger();
    }
}