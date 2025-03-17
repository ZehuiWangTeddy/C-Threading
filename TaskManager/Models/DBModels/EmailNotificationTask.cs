using TaskManager.Models.Enums;
namespace TaskManager.Models.DBModels;
using System;

public class EmailNotificationTask : BaseTask
{
    public string SenderEmail { get; set; }
    public string RecipientEmail { get; set; }
    public string Subject { get; set; }
    public string MessageBody { get; set; }
    
    public EmailNotificationTask() : base("", new ExecutionTime(), PriorityType.Medium) {}

    public EmailNotificationTask(string name, ExecutionTime executionTime, PriorityType priority, string senderEmail, string recipientEmail, string subject, string messageBody) 
        : base(name, executionTime, priority)
    {
        SenderEmail = senderEmail;
        RecipientEmail = recipientEmail;
        Subject = subject;
        MessageBody = messageBody;
    }

    public override void Execute()
    {
        Console.WriteLine($"Sending email from {SenderEmail} to {RecipientEmail}");
        Console.WriteLine($"Subject: {Subject}");
        Console.WriteLine($"Message: {MessageBody}");
    }
}