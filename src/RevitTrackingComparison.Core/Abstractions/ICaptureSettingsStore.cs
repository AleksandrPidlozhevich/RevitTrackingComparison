using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Loads and saves the capture configuration (categories + parameters to record). Backed by an
/// external config file; a sensible default is created on first load if the file is missing.
/// </summary>
public interface ICaptureSettingsStore
{
    CaptureSettings Load();

    void Save(CaptureSettings settings);
}
