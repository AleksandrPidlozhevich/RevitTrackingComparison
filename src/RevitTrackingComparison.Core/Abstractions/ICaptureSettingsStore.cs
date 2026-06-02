using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Loads and saves the capture configuration (categories + parameters to record), backed by an
/// external config file.
/// </summary>
/// <remarks>
/// Error policy: <see cref="Load"/> degrades gracefully — it returns sensible defaults when the file is
/// missing or unreadable (and logs that, since the caller cannot tell defaults from a real config).
/// <see cref="Save"/> instead propagates failures as exceptions for the caller to surface.
/// </remarks>
public interface ICaptureSettingsStore
{
    CaptureSettings Load();

    void Save(CaptureSettings settings);
}