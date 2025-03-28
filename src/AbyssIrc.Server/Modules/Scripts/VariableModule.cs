using AbyssIrc.Core.Attributes.Scripts;
using AbyssIrc.Server.Interfaces.Services.System;
using HamLink.Core.Attributes.Scripts;

namespace AbyssIrc.Server.Modules.Scripts;

[ScriptModule("template")]
public class VariableModule
{
    private readonly ITextTemplateService _textTemplateService;

    public VariableModule(ITextTemplateService textTemplateService)
    {
        _textTemplateService = textTemplateService;
    }

    [ScriptFunction("Add Variable to the text template service and you can find by use {{name}}")]
    public void AddVariable(string name, object value)
    {
        _textTemplateService.AddVariable(name, value);
    }

    [ScriptFunction("Add Variable Builder to the text template service and you can find by use {{name}}")]
    public void AddVariableBuilder(string name, Func<object> builder)
    {
        _textTemplateService.AddVariableBuilder(name, builder);
    }


    [ScriptFunction("Replaces the text with the variables")]
    public string TranslateText(string text, object context = null)
    {
        return _textTemplateService.TranslateText(text, context);
    }

    [ScriptFunction("Get all variables")]
    public List<string> GetVariables()
    {
        return _textTemplateService.GetVariables();
    }
}
