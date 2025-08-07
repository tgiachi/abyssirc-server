using AbyssIrc.Core.Interfaces.Services;

namespace AbyssIrc.Server.Core.Interfaces.Services;

public interface IScriptEngineService: IAbyssStarStopService
{
    void AddInitScript(string script);
    void ExecuteScript(string script);
    void ExecuteScriptFile(string scriptFile);
    void AddCallback(string name, Action<object[]> callback);
    void AddConstant(string name, object value);
    void ExecuteCallback(string name, params object[] args);
    void AddScriptModule(Type type);

    string ToScriptEngineFunctionName(string name);
}
