using System.Net;
using System.Text.Json.Serialization;
using AbyssIrc.Server.Core.Json.Converters;

namespace AbyssIrc.Server.Core.Data.Config.Sections;

public class NetworkBindConfig
{
    [JsonConverter(typeof(IPAddressConverter))]
    public IPAddress Host { get; set; }
    public bool UseSsl { get; set; }

    [JsonConverter(typeof(NumberRangeConverter))]
    public int[] Ports { get; set; }
}
