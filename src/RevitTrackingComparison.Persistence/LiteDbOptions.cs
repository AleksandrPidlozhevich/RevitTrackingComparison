using RevitTrackingComparison.Core.Configuration;

namespace RevitTrackingComparison.Persistence;

public sealed class LiteDbOptions
{
    /// <summary>Data root (default %AppData%\TrackingComparison); override for tests.</summary>
    public string RootFolder { get; set; } = TrackingDataPaths.Root;

    /// <summary>Folder holding one subfolder per project, each with one file per snapshot.</summary>
    public string SnapshotsFolder => Path.Combine(RootFolder, "Snapshots");

    /// <summary>Path of the capture configuration file (categories + parameters).</summary>
    public string CaptureConfigPath => Path.Combine(RootFolder, "capture-config.json");
}