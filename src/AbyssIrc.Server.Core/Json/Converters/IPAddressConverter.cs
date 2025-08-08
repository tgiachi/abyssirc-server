namespace AbyssIrc.Server.Core.Json.Converters;

using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Custom JSON converter for IPAddress type
/// Converts IP address strings like "192.168.1.1" to/from IPAddress objects
/// </summary>
public class IPAddressConverter : JsonConverter<IPAddress>
{
    /// <summary>
    /// Reads and converts the JSON string to an IPAddress object
    /// </summary>
    public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token for IP address, got {reader.TokenType}");
        }

        string value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!IPAddress.TryParse(value, out IPAddress ipAddress))
        {
            throw new JsonException($"Invalid IP address format: {value}");
        }

        return ipAddress;
    }

    /// <summary>
    /// Writes the IPAddress back to JSON as a string
    /// </summary>
    public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
