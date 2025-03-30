using AbyssIrc.Server.Interfaces.Services.System;

namespace AbyssIrc.Server.Services;

public class ChannelManagerService : IChannelManagerService
{
    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}
