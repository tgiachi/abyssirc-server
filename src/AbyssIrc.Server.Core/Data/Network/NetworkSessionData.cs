using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Types.Sessions;

namespace AbyssIrc.Server.Core.Data.Network;

public class NetworkSessionData : IDisposable
{
    public delegate void SendMessagesDelegate(string id, params IIrcCommand[] commands);
    public event SendMessagesDelegate OnSendMessages;

    public string Id { get; set; }
    public string Nickname { get; set; }
    public string Username { get; set; }
    public string RealName { get; set; }

    public SessionAuthStatusType AuthStatus { get; set; } = SessionAuthStatusType.None;

    public void SendMessages(params IIrcCommand[] commands)
    {
        OnSendMessages?.Invoke(Id, commands);
    }


    public void Dispose()
    {
        Id = null;
        OnSendMessages = null;
        Nickname = null;
        Username = null;
        RealName = null;
    }
}
