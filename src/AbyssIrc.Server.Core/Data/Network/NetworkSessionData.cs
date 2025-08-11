using AbyssIrc.Protocol.Messages.Interfaces.Commands;

namespace AbyssIrc.Server.Core.Data.Network;

public class NetworkSessionData : IDisposable
{
    public delegate void SendMessagesDelegate(string id, params IIrcCommand[] commands);

    public event SendMessagesDelegate OnSendMessages;

    public string Id { get; set; }


    public void SendMessages(params IIrcCommand[] commands)
    {
        OnSendMessages?.Invoke(Id, commands);
    }


    public void Dispose()
    {
        Id = null;
        OnSendMessages = null;
    }
}
