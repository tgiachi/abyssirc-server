using AbyssIrc.Core.Interfaces.Services;

namespace AbyssIrc.Server.Interfaces.Services.System;

public interface IScriptEngineService : IAbyssStarStopService
{

    void ExecuteScript(string script);

    void ExecuteScriptFile(string scriptFile);
    void AddCallback(string name, Action<object[]> callback);

    void AddConstant(string name, object value);
}
