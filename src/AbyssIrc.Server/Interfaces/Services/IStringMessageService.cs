namespace AbyssIrc.Server.Interfaces.Services;

public interface IStringMessageService
{
    string GetMessage(string key, object? context = null);
}
