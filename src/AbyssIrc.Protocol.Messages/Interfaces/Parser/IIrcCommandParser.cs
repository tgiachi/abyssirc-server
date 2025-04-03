using AbyssIrc.Protocol.Messages.Interfaces.Commands;

namespace AbyssIrc.Protocol.Messages.Interfaces.Parser;

public interface IIrcCommandParser
{
    Task<List<IIrcCommand>> ParseAsync(string message);

    Task<string> SerializeAsync(IIrcCommand command);

    void RegisterCommand(IIrcCommand command);

    List<string> SanitizeMessage(string rawMessage);
}
