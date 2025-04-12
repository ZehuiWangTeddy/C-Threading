namespace TaskManager.Messages;

public class ToastMessage : BaseMessage
{
    public ToastMessage(string master, string slave, string? msgContent)
    {
        Master = master;
        Slave = slave;
        MsgContent = msgContent;
    }

    public string? MsgContent { get; set; }
}