using System.Collections.Concurrent;
using AbyssIrc.Server.Core.Events.Variables;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Servers.Utils;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace AbyssIrc.Server.Services;

public class TextTemplateService
    : ITextTemplateService, IAbyssSignalListener<AddVariableEvent>, IAbyssSignalListener<AddVariableBuilderEvent>
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, Func<object>> _variableBuilder = new();
    private readonly ConcurrentDictionary<string, object> _variables = new();
    private readonly IAbyssSignalService _signalService;

    public TextTemplateService(ILogger<TextTemplateService> logger, IAbyssSignalService signalService)
    {
        _logger = logger;
        _signalService = signalService;

        AddDefaultVariables();

        signalService.Subscribe<AddVariableEvent>(this);
        signalService.Subscribe<AddVariableBuilderEvent>(this);
    }

    private void AddDefaultVariables()
    {
        AddVariable("cpu_count", Environment.ProcessorCount);
        AddVariable("os_name", Environment.OSVersion.VersionString);
        AddVariable("os_version", Environment.OSVersion.Version);
        AddVariable("os_platform", Environment.OSVersion.Platform);
    }

    public void AddVariableBuilder(string variableName, Func<object> builder)
    {
        _logger.LogDebug("Adding variable builder for {variableName}", variableName);
        _variableBuilder[variableName] = builder;
    }

    public void AddVariable(string variableName, object value)
    {
        _logger.LogDebug("Adding variable {variableName} with value {value}", variableName, value);
        _variables[variableName] = value;
    }

    public string TranslateText(string text, object? context = null)
    {
        try
        {
            // Parse il template
            Template template = Template.Parse(text);


            var scriptContext = new TemplateContext();
            var scriptObject = new ScriptObject();


            foreach (var variable in _variables)
            {
                scriptObject.Add(variable.Key, variable.Value);
            }


            foreach (var builder in _variableBuilder)
            {
                scriptObject.Add(builder.Key, new DynamicVariable(builder.Value));
            }

            scriptContext.PushGlobal(scriptObject);

            if (context != null)
            {
                scriptObject.Add("context", context);
            }

            return MircNotationConverter.Convert(template.Render(scriptContext));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template: {Template}", text);

            throw new Exception("Error rendering template", ex);
        }
    }

    public List<string> GetVariables()
    {
        var list = new List<string>();
        list.AddRange(_variables.Keys);
        list.AddRange(_variableBuilder.Keys);

        list = list.OrderByDescending(x => x).ToList();

        return list;
    }

    public void RebuildVariables()
    {
        foreach (var builder in _variableBuilder.AsParallel())
        {
            try
            {
                _variables[builder.Key] = builder.Value();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error building variable {VariableName}", builder.Key);
            }
        }
    }

    public async Task OnEventAsync(AddVariableEvent message)
    {
        AddVariable(message.VariableName, message.Value);
    }

    public async Task OnEventAsync(AddVariableBuilderEvent message)
    {
        AddVariableBuilder(message.VariableName, message.Builder);
    }

    private class DynamicVariable : IScriptCustomFunction
    {
        private readonly Func<object> _valueFactory;

        public DynamicVariable(Func<object> valueFactory)
        {
            _valueFactory = valueFactory;
        }

        public object Invoke(
            TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement
        )
        {
            try
            {
                return _valueFactory();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ValueTask<object> InvokeAsync(
            TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement
        )
        {
            try
            {
                var result = _valueFactory();
                return new ValueTask<object>(result);
            }
            catch (Exception)
            {
                return new ValueTask<object>((object)null);
            }
        }

        public ScriptParameterInfo GetParameterInfo(int index)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public int RequiredParameterCount => 0;
        public int ParameterCount => 0;
        public ScriptVarParamKind VarParamKind => ScriptVarParamKind.Direct;


        public Type ReturnType => typeof(object);
    }
}
