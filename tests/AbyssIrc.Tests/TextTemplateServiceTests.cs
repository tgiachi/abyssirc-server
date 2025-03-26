using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Services;
using AbyssIrc.Signals.Data.Configs;
using AbyssIrc.Signals.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace AbyssIrc.Tests;

public class TextTemplateServiceTests
{
    private ITextTemplateService _textTemplateService;

    [SetUp]
    public void Setup()
    {
        _textTemplateService = new TextTemplateService(
            new NullLogger<TextTemplateService>(),
            new AbyssSignalService(new AbyssIrcSignalConfig())
        );
    }

    [Test]
    public void ContextReplaceTest()
    {
        // var template = Template.Parse("Hello {{name}}!");
        // var result = template.Render(new { Name = "World" }); // => "Hello World!"

        var result = _textTemplateService.TranslateText("Hello {{context.name}}!", new { Name = "World" });

        Assert.That(result, Is.EqualTo("Hello World!"));
    }

    [Test]
    public void GlobalReplaceTest()
    {
        _textTemplateService.AddVariable("name", "World");

        var result = _textTemplateService.TranslateText("Hello {{name}}!");

        Assert.That(result, Is.EqualTo("Hello World!"));
    }

    [Test]
    public void DynamicGlobalVariableTest()
    {
        _textTemplateService.AddVariableBuilder("name", () => "World");

        var result = _textTemplateService.TranslateText("Hello {{name}}!");

        Assert.That(result, Is.EqualTo("Hello World!"));
    }
}
