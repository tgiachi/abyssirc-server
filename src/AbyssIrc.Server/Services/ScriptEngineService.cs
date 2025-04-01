using System.Reflection;
using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Core.Data.Internal.Scripts;
using AbyssIrc.Core.Events.Core;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Core.Types;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Utils.Scripts;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using HamLink.Core.Attributes.Scripts;
using Jint;
using Jint.Runtime.Interop;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Services;

public class ScriptEngineService : IScriptEngineService, IAbyssSignalListener<ServerReadyEvent>
{
    private readonly ILogger _logger;

    private readonly List<string> _initScripts = ["bootstrap.js", "index.js", "abyssirc.js"];

    private readonly DirectoriesConfig _directoriesConfig;
    private readonly Engine _jsEngine;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ScriptModuleData> _scriptModules;

    private readonly Dictionary<string, Action<object[]>> _callbacks = new();
    private readonly Dictionary<string, object> _constants = new();

    private readonly IAbyssSignalService _abyssSignalService;


    public ScriptEngineService(
        ILogger<ScriptEngineService> logger, DirectoriesConfig directoriesConfig, IServiceProvider serviceProvider,
        List<ScriptModuleData> scriptModules, IAbyssSignalService abyssSignalService
    )
    {
        _logger = logger;
        _directoriesConfig = directoriesConfig;
        _serviceProvider = serviceProvider;
        _scriptModules = scriptModules;

        _abyssSignalService = abyssSignalService;

        var typeResolver = TypeResolver.Default;

        typeResolver.MemberNameCreator = MemberNameCreator;
        _jsEngine = new Engine(
            options =>
            {
                options.EnableModules(directoriesConfig[DirectoryType.Scripts]);
                options.AllowClr(GetType().Assembly);
                options.SetTypeResolver(typeResolver);
            }
        );

        _abyssSignalService.Subscribe(this);
    }

    private IEnumerable<string> MemberNameCreator(MemberInfo memberInfo)
    {
        _logger.LogDebug("Member name creator for {MemberInfo}", memberInfo.Name.ToSnakeCase());
        yield return memberInfo.Name.ToSnakeCase();
    }


    public Task OnEventAsync(ServerReadyEvent signalEvent)
    {
        if (_callbacks.TryGetValue("onStarted", out var callback))
        {
            _logger.LogInformation("Executing onStarted");
            callback(null);
        }

        return Task.CompletedTask;
    }

    private void ExecuteBootstrap()
    {
        foreach (var file in _initScripts.Select(s => Path.Combine(_directoriesConfig[DirectoryType.Scripts], s)))
        {
            if (File.Exists(file))
            {
                var fileName = Path.GetFileName(file);
                _logger.LogInformation("Executing {FileName}  script", fileName);
                ExecuteScriptFile(file);
            }
        }
    }

    public Task StartAsync()
    {
        AddConstant("Version", "1.0.0");

        foreach (var module in _scriptModules)
        {
            var scriptModuleAttribute = module.ModuleType.GetCustomAttribute<ScriptModuleAttribute>();
            var instance = _serviceProvider.GetService(module.ModuleType);

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Unable to create instance of script module {module.ModuleType.Name}"
                );
            }

            _logger.LogDebug("Registering script module {Name}", scriptModuleAttribute.Name);

            _jsEngine.SetValue(scriptModuleAttribute.Name, instance);
        }


        _logger.LogDebug("Generating scripts documentation in scripts directory named 'index.d.ts'");
        var documentation = TypeScriptDocumentationGenerator.GenerateDocumentation(_scriptModules, _constants);

        File.WriteAllText(Path.Combine(_directoriesConfig[DirectoryType.Scripts], "index.d.ts"), documentation);


        ExecuteBootstrap();


        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }

    public void ExecuteScript(string script)
    {
        try
        {
            _jsEngine.Execute(script);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error executing script");
        }
    }

    public void ExecuteScriptFile(string scriptFile)
    {
        var content = File.ReadAllText(scriptFile);

        ExecuteScript(content);
    }

    public void AddCallback(string name, Action<object[]> callback)
    {
        _callbacks[name] = callback;
    }

    public void AddConstant(string name, object value)
    {
        _constants[name.ToSnakeCaseUpper()] = value;
        _jsEngine.SetValue(name.ToSnakeCaseUpper(), value);
    }
}
