using TaskManager.Models;

namespace TaskManager.Messages
{
    public class DbOperationMessage : BaseMessage
    {
        public enum OperationType : byte
        {
            Update,
            Delete
        }

        public TaskItem Item { get; set; } = null!;

        public OperationType Type { get; set; }

        public object? Body { get; set; }

        public DbOperationMessage(string master, string slave, TaskItem item, OperationType type, object? body = null)
        {
            Master = master;
            Slave = slave;
            Type = type;
            Body = body;
            Item = item;
        }

    }
}
