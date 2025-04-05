using AbyssIrc.Server.Core.Attributes.Scripts;
using AbyssIrc.Server.Core.Events.Channels;
using AbyssIrc.Server.Core.Events.Server;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Services;
using AbyssIrc.Signals.Interfaces.Services;

namespace AbyssIrc.Server.Modules.Scripts;

[ScriptModule("channels")]
public class ChannelsModule
{
    private readonly IChannelManagerService _channelManagerService;

    private readonly IAbyssSignalService _abyssSignalService;

    public ChannelsModule(IChannelManagerService channelManagerService, IAbyssSignalService abyssSignalService)
    {
        _channelManagerService = channelManagerService;
        _abyssSignalService = abyssSignalService;
    }


    [ScriptFunction("Join a channel")]
    public async Task Join(string nickname, string channel)
    {
        _abyssSignalService.PublishAsync(new AddUserJoinChannelEvent(nickname, channel)).ConfigureAwait(false);
    }

    [ScriptFunction("Set the topic of a channel by the server")]
    public async Task ServerTopic(string channel, string topic)
    {
        _abyssSignalService.PublishAsync(new ServerSetTopicRequestEvent(channel, topic)).ConfigureAwait(false);
    }
}
