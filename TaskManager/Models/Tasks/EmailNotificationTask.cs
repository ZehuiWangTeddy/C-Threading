namespace TaskManager.Models.Tasks;
using System;

public class EmailNotificationTask : BaseTask
{
    private string SenderEmail { get; set; }
    private string RecipientEmail { get; set; }
    private string Subject { get; set; }
    private string MessageBody { get; set; }

    public EmailNotificationTask(int id, string name, ExecutionTime executionTime, PriorityType priority, string senderEmail, string recipientEmail, string subject, string messageBody) : base(id, name, executionTime, priority)
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