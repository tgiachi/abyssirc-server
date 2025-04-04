namespace AbyssIrc.Server.Core.Data.Configs.Sections;

/// <summary>
/// Configuration for the web server integrated with the IRC server.
/// Defines settings for the HTTP/API server.
/// </summary>
public class WebServerConfig
{
    /// <summary>
    /// The host address on which the web server listens for connections.
    /// A value of "*" indicates that the server listens on all available addresses.
    /// </summary>
    public string Host { get; set; } = "*";

    /// <summary>
    /// The TCP port on which the web server listens for HTTP connections.
    /// The default value is 20001.
    /// </summary>
    public int Port { get; set; } = 20001;

    /// <summary>
    /// Indicates whether the OpenAPI (Swagger) interface is enabled.
    /// When enabled, it provides interactive documentation for the APIs.
    /// The default value is false.
    /// </summary>
    public bool IsOpenApiEnabled { get; set; } = true;

    /// <summary>
    /// Indicates whether access to the web server requires IRC operator privileges.
    /// When set to true, only IRC operators can access the APIs.
    /// The default value is true for enhanced security.
    /// </summary>
    public bool IsSecureWithOpers { get; set; } = true;
}
