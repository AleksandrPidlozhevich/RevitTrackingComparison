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
    private readonly IPluginLogger _logger;

    public JsonCaptureSettingsStore(LiteDbOptions options, IPluginLogger logger)
    {
        _options = options;
        _logger = logger;
    }

    public CaptureSettings Load()
    {
        var path = _options.CaptureConfigPath;
        if (!File.Exists(path))
        {
            _logger.Info($"Capture config not found at '{path}'; creating defaults.");
            var defaults = Default();
            Save(defaults);
            return defaults;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<SettingsDto>(File.ReadAllText(path));
            if (dto is null)
            {
                _logger.Warn($"Capture config at '{path}' deserialized to null; using defaults.");
                return Default();
            }

            return dto.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to read capture config from '{path}'; using defaults.");
            return Default();
        }
    }

    public void Save(CaptureSettings settings)
    {
        var path = _options.CaptureConfigPath;
        try
        {
            Directory.CreateDirectory(_options.RootFolder);
            File.WriteAllText(path, JsonSerializer.Serialize(SettingsDto.FromDomain(settings), JsonOptions));
            _logger.Info($"Capture config saved to '{path}' ({settings.Rules.Count} rules).");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to save capture config to '{path}'.");
            throw;
        }
    }

    private static CaptureSettings Default()
    {
        return new CaptureSettings
        {
            Rules = new[]
            {
                new CaptureRule { Category = "Walls", Parameters = new[] { "Base Offset", "Unconnected Height" } },
                new CaptureRule { Category = "Doors", Parameters = new[] { "Height", "Width" } }
            }
        };
    }

    private sealed class SettingsDto
    {
        public List<RuleDto> Rules { get; set; } = new();

        public CaptureSettings ToDomain()
        {
            return new CaptureSettings { Rules = Rules.Select(r => r.ToDomain()).ToList() };
        }

        public static SettingsDto FromDomain(CaptureSettings settings)
        {
            return new SettingsDto { Rules = settings.Rules.Select(RuleDto.FromDomain).ToList() };
        }
    }

    private sealed class RuleDto
    {
        public string Category { get; set; } = string.Empty;

        public List<string> Parameters { get; set; } = new();

        public CaptureRule ToDomain()
        {
            return new CaptureRule { Category = Category, Parameters = Parameters.ToList() };
        }

        public static RuleDto FromDomain(CaptureRule rule)
        {
            return new RuleDto { Category = rule.Category, Parameters = rule.Parameters.ToList() };
        }
    }
}