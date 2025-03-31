using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Server.Data.Internal.Handlers;

namespace AbyssIrc.Server.Extensions;

public static class AbyssConfigExtension
{
    public static AbyssServerData ToServerData(this AbyssIrcConfig ircConfig)
    {
        return new AbyssServerData()
        {
            Hostname = ircConfig.Network.Host
        };
    }

    public static RplISupport ToRplSupportCommand(this AbyssIrcConfig ircConfig, string nickname)
    {
        var rplCommand = new RplISupport
        {
            Nickname = nickname,
            Network = ircConfig.Admin.NetworkName,
            ServerName = ircConfig.Network.Host,
            AwayLen = ircConfig.Limits.MaxAwayLength,
            CaseMapping = ircConfig.Limits.CaseMapping,
            ChanModes = ("beI", "kfL", "lj", "psmntirRcOAQKVCuzNSMTGZ"),
            HostLen = 64,
            ChannelLen = ircConfig.Limits.MaxChannelNameLength,
            ChanTypes = "#&+!",
            EList = ircConfig.Limits.Elist,
            Modes = ircConfig.Limits.MaxModes,
            Silence = ircConfig.Limits.MaxSilence,
            KickLen = ircConfig.Limits.MaxAwayLength,
            TopicLen = ircConfig.Limits.MaxChannelNameLength,
            Prefix = ("@+", "ov"),
            StatusMsg = ircConfig.Limits.StatusMsg
        };

        rplCommand.Parameters.Add("MAXBANS", ircConfig.Limits.MaxBansPerChannel.ToString());
        rplCommand.Parameters.Add("MAXCHANNELS", ircConfig.Limits.MaxChannelsPerUser.ToString());
        rplCommand.Parameters.Add("MAXNICKLEN", ircConfig.Limits.MaxNickLength.ToString());
        rplCommand.Parameters.Add("WHOX", string.Empty);
        rplCommand.Parameters.Add("WALLVOICES", string.Empty);
        rplCommand.Parameters.Add("USERIP", string.Empty);
        rplCommand.Parameters.Add("CPRIVMSG", string.Empty);
        rplCommand.Parameters.Add("CNOTICE", string.Empty);



        rplCommand.AddChanLimit("@", ircConfig.Limits.MaxChanJoin);

        //rplCommand.AddChanLimit("#", ircConfig.Limits.MaxChanJoin);

        return rplCommand;
    }
}
