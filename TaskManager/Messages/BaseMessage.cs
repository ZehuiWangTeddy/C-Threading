namespace TaskManager.Messages;

public class BaseMessage
{
    public string Master { get; set; } = null!;
    public string Slave { get; set; } = null!;
}