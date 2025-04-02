namespace TaskManager.Messages
{
    public class NavigationMessage : BaseMessage
    {
        /// <summary>
        /// true -->Goto Back Page  false -->Goto Next Page
        /// </summary>
        public bool IsBack { get; set; }

        public NavigationMessage(string master, string slave, bool isBack)
        {
            Master = master;
            Slave = slave;
            IsBack = isBack;
        }
    }
}
