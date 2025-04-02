namespace TaskManager.Messages
{
    public class ToastMessage:BaseMessage
    {

        public string? MsgContent { get; set; }

        public ToastMessage(string master, string slave, string? msgContent)
        {
            Master = master;
            Slave = slave;
            MsgContent = msgContent;
        }
    }
}
