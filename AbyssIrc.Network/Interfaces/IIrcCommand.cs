namespace AbyssIrc.Network.Interfaces;

public interface IIrcCommand
{
    string Code { get; }

    string Parse(string line);

    string Write();
}
