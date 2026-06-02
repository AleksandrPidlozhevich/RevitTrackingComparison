using System.Text.Json;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Persistence.Config;

/// <summary>
/// Stores the capture configuration as a human-editable JSON file. Creates a sensible default
/// on first load if the file is missing.
/// </summary>
public sealed class JsonCaptureSettingsStore : ICaptureSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly LiteDbOptions _options;

    public JsonCaptureSettingsStore(LiteDbOptions options)
    {
        _options = options;
    }

    public CaptureSettings Load()
    {
        var path = _options.CaptureConfigPath;
        if (!File.Exists(path))
        {
            var defaults = Default();
            Save(defaults);
            return defaults;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<SettingsDto>(File.ReadAllText(path));
            return dto?.ToDomain() ?? Default();
        }
        catch
        {
            return Default();
        }
    }

    public void Save(CaptureSettings settings)
    {
        Directory.CreateDirectory(_options.RootFolder);
        File.WriteAllText(_options.CaptureConfigPath, JsonSerializer.Serialize(SettingsDto.FromDomain(settings), JsonOptions));
    }

    private static CaptureSettings Default() => new()
    {
        Rules = new[]
        {
            new CaptureRule { Category = "Walls", Parameters = new[] { "Base Offset", "Unconnected Height" } },
            new CaptureRule { Category = "Doors", Parameters = new[] { "Height", "Width" } },
        },
    };

    private sealed class SettingsDto
    {
        public List<RuleDto> Rules { get; set; } = new();

        public CaptureSettings ToDomain() => new() { Rules = Rules.Select(r => r.ToDomain()).ToList() };

        public static SettingsDto FromDomain(CaptureSettings settings) =>
            new() { Rules = settings.Rules.Select(RuleDto.FromDomain).ToList() };
    }

    private sealed class RuleDto
    {
        public string Category { get; set; } = string.Empty;

        public List<string> Parameters { get; set; } = new();

        public CaptureRule ToDomain() => new() { Category = Category, Parameters = Parameters.ToList() };

        public static RuleDto FromDomain(CaptureRule rule) =>
            new() { Category = rule.Category, Parameters = rule.Parameters.ToList() };
    }
}
