using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Core.Types;
using AbyssIrc.Server.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Services;

public class StringMessageService : IStringMessageService
{
    private static readonly string[] _extensions = { "*.txt", "*.messages", "*.strings" };
    private readonly ILogger _logger;
    private readonly DirectoriesConfig _directoriesConfig;

    private readonly ITextTemplateService _textTemplateService;

    private readonly Dictionary<string, string> _messages = new();

    public StringMessageService(
        ILogger<StringMessageService> logger, DirectoriesConfig directoriesConfig, ITextTemplateService textTemplateService
    )
    {
        _logger = logger;
        _directoriesConfig = directoriesConfig;
        _textTemplateService = textTemplateService;

        LoadTextMessages(_directoriesConfig[DirectoryType.Messages]);
    }

    private void LoadTextMessages(string messagesPath)
    {
        foreach (var extension in _extensions)
        {
            foreach (var file in Directory.GetFiles(messagesPath, extension, SearchOption.AllDirectories))
            {
                try
                {
                    _logger.LogDebug("Loading text messages from file: {File}", file);
                    string[] lines = File.ReadAllLines(file);
                    int loadedCount = 0;

                    foreach (var line in lines)
                    {
                        string trimmedLine = line.Trim();


                        if (string.IsNullOrWhiteSpace(trimmedLine) ||
                            trimmedLine.StartsWith(';') ||
                            trimmedLine.StartsWith('#') ||
                            trimmedLine.StartsWith("//"))
                        {
                            continue;
                        }


                        int equalIndex = trimmedLine.IndexOf('=');
                        if (equalIndex > 0)
                        {
                            string key = trimmedLine[..equalIndex].Trim();
                            string value = trimmedLine[(equalIndex + 1)..];

                            _messages[key] = value;
                            loadedCount++;
                        }
                    }

                    _logger.LogInformation("Loaded {Count} text messages from {File}", loadedCount, file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading text messages from file: {File}", file);
                }
            }
        }
    }

    public string GetMessage(string key, object? context = null)
    {
        if (!_messages.TryGetValue(key, out var message))
        {
            _logger.LogWarning("Message not found: {Key}", key);
            return key;
        }

        return _textTemplateService.TranslateText(message, context);
    }
}
