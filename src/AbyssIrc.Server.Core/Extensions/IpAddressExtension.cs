using System.Net;

namespace AbyssIrc.Server.Core.Extensions;

public static class IpAddressExtension
{
    public static IPAddress ToIpAddress(this string ip)
    {
        if (ip == "*")
        {
            return IPAddress.Any;
        }

        if (ip == "::")
        {
            return IPAddress.IPv6Any;
        }

        if (IPAddress.TryParse(ip, out var ipAddress))
        {
            return ipAddress;
        }

        if (ip == "*.*.*.*")
        {
            return IPAddress.Any;
        }

        throw new FormatException($"Invalid IP address format: {ip}");
    }
}
