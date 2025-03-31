using System.Collections.Concurrent;
using AbyssIrc.Network.Data.Channels;
using AbyssIrc.Server.Data.Events.Channels;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Services;

public class ChannelManagerService : IChannelManagerService
{
    private readonly ILogger _logger;

    private readonly IAbyssSignalService _abyssSignalService;

    public ConcurrentDictionary<string, ChannelData> Channels { get; } = new();


    public ChannelManagerService(ILogger<ChannelManagerService> logger, IAbyssSignalService abyssSignalService)
    {
        _logger = logger;
        _abyssSignalService = abyssSignalService;
    }

    public bool IsChannelRegistered(string channelName)
    {
        return Channels.ContainsKey(channelName);
    }

    public void RegisterChannel(string channelName)
    {
        if (IsChannelRegistered(channelName))
        {
            _logger.LogWarning("Channel {ChannelName} is already registered.", channelName);
            return;
        }

        var channelData = new ChannelData(channelName);
        Channels.TryAdd(channelName, channelData);
        _logger.LogInformation("Channel {ChannelName} registered successfully.", channelName);
        _abyssSignalService.PublishAsync(new ChannelCreatedEvent(channelName));
    }

    public void AddNicknameToChannel(string channelName, string nickname)
    {
        if (!IsChannelRegistered(channelName))
        {
            _logger.LogWarning("Channel {ChannelName} is not registered.", channelName);
            return;
        }

        var channelData = Channels[channelName];
        if (channelData.IsMember(nickname))
        {
            _logger.LogWarning("Nickname {Nickname} is already in channel {ChannelName}.", nickname, channelName);
            return;
        }

        channelData.AddMember(nickname);
        _logger.LogInformation("Nickname {Nickname} added to channel {ChannelName}.", nickname, channelName);

        _abyssSignalService.PublishAsync(new NicknameJoinChannelEvent(nickname, channelName));
    }

    public void RemoveNicknameFromChannel(string channelName, string nickname)
    {
        if (!IsChannelRegistered(channelName))
        {
            _logger.LogWarning("Channel {ChannelName} is not registered.", channelName);
            return;
        }

        var channelData = Channels[channelName];
        if (!channelData.IsMember(nickname))
        {
            _logger.LogWarning("Nickname {Nickname} is not in channel {ChannelName}.", nickname, channelName);
            return;
        }

        channelData.RemoveMember(nickname);
        _logger.LogInformation("Nickname {Nickname} removed from channel {ChannelName}.", nickname, channelName);
        _abyssSignalService.PublishAsync(new NicknamePartChannelEvent(nickname, channelName));
    }

    public List<string> GetChannelNames()
    {
        return Channels.Keys.ToList();
    }

    public List<(string channelName, string topic, int memberCount)> GetChannelTopics()
    {
        return Channels
            .Where(x => !x.Value.IsSecret)
            .Select(channel => (channel.Key, channel.Value.Topic, channel.Value.MemberCount))
            .ToList();
    }

    public List<string> GetNicknamesInChannel(string channelName)
    {
        if (!IsChannelRegistered(channelName))
        {
            _logger.LogWarning("Channel {ChannelName} is not registered.", channelName);
            return new List<string>();
        }

        var channelData = Channels[channelName];
        return channelData.GetMemberList().ToList();
    }

    public ChannelData GetChannelData(string channelName)
    {
        return !IsChannelRegistered(channelName) ? null : Channels[channelName];
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}
