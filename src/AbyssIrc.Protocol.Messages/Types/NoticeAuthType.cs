namespace AbyssIrc.Protocol.Messages.Types;

/// <summary>
///     Types of AUTH notices commonly sent during connection
/// </summary>
public enum NoticeAuthType
{
    /// <summary>Server is looking up the client's hostname</summary>
    HostnameLookup,

    /// <summary>Server has found the client's hostname</summary>
    HostnameFound,

    /// <summary>Server is checking the client's ident</summary>
    IdentCheck,

    /// <summary>Server received no ident response</summary>
    NoIdent,

    /// <summary>Client is not authorized to connect</summary>
    Unauthorized,

    /// <summary>Other AUTH-related notice</summary>
    Other
}
