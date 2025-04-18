using TaskManager.Models.Enums;

namespace TaskManager.Models.DBModels;

public class EmailNotificationTask : BaseTask
{
    public EmailNotificationTask() : base("", PriorityType.Medium)
    {
    }

    public EmailNotificationTask(string name, int executionTimeId, PriorityType priority, string senderEmail,
        string recipientEmail, string subject, string messageBody)
        : base(name, priority)
    {
        SenderEmail = senderEmail;
        RecipientEmail = recipientEmail;
        Subject = subject;
        MessageBody = messageBody;
        ExecutionTimeId = executionTimeId;
    }

    public string SenderEmail { get; set; }
    public string RecipientEmail { get; set; }
    public string Subject { get; set; }
    public string MessageBody { get; set; }

    public override void Execute()
    {
        var date = DateTime.Now;
        var message =
            $"{date}: Email Notification Task: Sending email from {SenderEmail} to {RecipientEmail}\nSubject: {Subject}\nMessage: {MessageBody}";
        Logger.AddLogMessage(message);
    }
}