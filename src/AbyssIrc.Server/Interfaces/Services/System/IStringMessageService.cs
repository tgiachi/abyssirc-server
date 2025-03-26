namespace AbyssIrc.Server.Interfaces.Services.System;

public interface IStringMessageService
{
    string GetMessage(string key, object? context = null);
}
