using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;

namespace TaskManager.Services
{
    public static class TaskConverter
    {
        public static BaseTask ConvertToDbTask(TaskItem taskItem, int executionTimeId)
        {
            return taskItem.TaskType switch
            {
                "Folder Watcher Task" => new FolderWatcherTask(
                    name: taskItem.Name,
                    executionTimeId: executionTimeId,
                    priority: Enum.Parse<PriorityType>(taskItem.Priority),
                    folderDirectory: taskItem.FileDirectory)
                {
                    ExecutionTime = CreateExecutionTime(taskItem),
                },

                "File Compression Task" => new FileCompressionTask(
                    name: taskItem.Name,
                    executionTimeId: executionTimeId,
                    priority: Enum.Parse<PriorityType>(taskItem.Priority),
                    fileDirectory: taskItem.FileDirectory)
                {
                    ExecutionTime = CreateExecutionTime(taskItem),
                },

                "File Backup System Task" => new FileBackupSystemTask(
                    name: taskItem.Name,
                    executionTimeId: executionTimeId,
                    priority: Enum.Parse<PriorityType>(taskItem.Priority),
                    targetDirectory: taskItem.TargetDirectory,
                    sourceDirectory: taskItem.SourceDirectory)
                {
                    ExecutionTime = CreateExecutionTime(taskItem),
                },

                "Email Notification Task" => new EmailNotificationTask(
                    name: taskItem.Name,
                    executionTimeId: executionTimeId,
                    priority: Enum.Parse<PriorityType>(taskItem.Priority),
                    senderEmail: taskItem.SenderEmail,
                    recipientEmail: taskItem.ReceiverEmail,
                    subject: taskItem.EmailSubject,
                    messageBody: taskItem.EmailBody)
                {
                    ExecutionTime = CreateExecutionTime(taskItem),
                },

                _ => throw new ArgumentException("Invalid task type")
            };
        }

        public static ExecutionTime CreateExecutionTime(TaskItem taskItem)
        {
            //Todo Can Mergers

            var recurrencePattern = Enum.Parse<RecurrencePattern>(taskItem.RecurrencePattern);
            if (recurrencePattern == RecurrencePattern.OneTime)
            {
                return new ExecutionTime(
                    onceExecutionTime: taskItem.ExecutionTime,
                    recurrencePattern: recurrencePattern,
                    intervalInMinutes: null,
                    nextExecutionTime: null);
            }
            return new ExecutionTime(
                onceExecutionTime: null,
                recurrencePattern: recurrencePattern,
                intervalInMinutes: null, 
                nextExecutionTime: taskItem.NextRunTime);
        }
    }
}