using TaskManager.Models.Enums;
namespace TaskManager.Models.DBModels;
using System;

public class EmailNotificationTask : BaseTask
{
    public string SenderEmail { get; set; }
    public string RecipientEmail { get; set; }
    public string Subject { get; set; }
    public string MessageBody { get; set; }
    
    public EmailNotificationTask() : base("", PriorityType.Medium) {}

    public EmailNotificationTask(string name, int executionTimeId, PriorityType priority, string senderEmail, string recipientEmail, string subject, string messageBody) 
        : base(name, priority)
    {
        SenderEmail = senderEmail;
        RecipientEmail = recipientEmail;
        Subject = subject;
        MessageBody = messageBody;
        ExecutionTimeId = executionTimeId;
    }

    public override void Execute()
    {
        var message = $"Email Notification Task: Sending email from {SenderEmail} to {RecipientEmail}\nSubject: {Subject}\nMessage: {MessageBody}";
        Logger.AddLogMessage(message);
        Console.WriteLine(message);
    }
}