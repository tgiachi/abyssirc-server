using AbyssIrc.Server.Core.Data.Sessions;

namespace AbyssIrc.Server.Core.Interfaces.Services.Server;

/// <summary>
/// Defines the contract for managing IRC client sessions within the server
/// </summary>
public interface ISessionManagerService
{
    /// <summary>
    /// Gets the maximum number of concurrent sessions allowed on the server
    /// </summary>
    int MaxSessions { get; }

    /// <summary>
    /// Adds a new session to the session manager
    /// </summary>
    /// <param name="id">Unique identifier for the session</param>
    /// <param name="ipEndpoint">IP endpoint of the client connection</param>
    /// <param name="session">Optional pre-configured IRC session (if null, a new session will be created)</param>
    void AddSession(string id, string ipEndpoint, IrcSession? session = null);

    /// <summary>
    /// Retrieves a specific session by its unique identifier
    /// </summary>
    /// <param name="id">Unique session identifier</param>
    /// <returns>The requested IRC session, or null if not found</returns>
    IrcSession? GetSession(string id);

    /// <summary>
    /// Retrieves all active sessions currently managed by the server
    /// </summary>
    /// <returns>A list of all active IRC sessions</returns>
    List<IrcSession> GetSessions();

    /// <summary>
    /// Finds a session by the user's nickname
    /// </summary>
    /// <param name="nickname">The nickname to search for</param>
    /// <returns>The IRC session matching the nickname, or null if not found</returns>
    IrcSession? GetSessionByNickname(string nickname);

    /// <summary>
    /// Retrieves session IDs for multiple nicknames
    /// </summary>
    /// <param name="nicknames">One or more nicknames to search for</param>
    /// <returns>A list of session IDs corresponding to the given nicknames</returns>
    List<string> GetSessionIdsByNicknames(params string[] nicknames);
}
