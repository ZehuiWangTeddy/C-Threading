﻿namespace TaskManager.Messages;

public class CallEventMessage : BaseMessage
{
    public enum CallbackEvent
    {
        Flush
    }

    public CallEventMessage(string master, string slave, CallbackEvent callbackType, bool cmd)
    {
        Master = master;
        Slave = slave;
        CallbackType = callbackType;
        Cmd = cmd;
    }

    public CallbackEvent CallbackType { get; set; }

    public bool Cmd { get; set; }
}