using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Utils.Hosts;
using AbyssIrc.Server.Data.Channels;

namespace AbyssIrc.Server.Core.Extensions;

public static class BannedExtensions
{
    public static bool IsBanned(this IEnumerable<BanEntry> bans, IrcSession session)
    {
        if (bans == null || bans.ToList().Count == 0)
        {
            return false;
        }

        return bans.Select(ban => HostMaskUtils.IsHostMaskMatch(ban.Mask, session.UserMask)).Any(isBanned => isBanned);
    }
}
