using AbyssIrc.Network.Interfaces.Commands;

namespace AbyssIrc.Server.Core.Interfaces.Listener;

public class IrcCallbackListener : IIrcMessageListener
{
    private readonly Func<string, IIrcCommand, Task> _callback;

    public IrcCallbackListener(Func<string, IIrcCommand, Task> callback)
    {
        _callback = callback;
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        return _callback(id, command);
    }
}
