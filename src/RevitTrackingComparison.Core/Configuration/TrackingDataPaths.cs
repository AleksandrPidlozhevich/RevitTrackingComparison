namespace RevitTrackingComparison.Core.Configuration;

// Single source of truth for the application's on-disk root under %AppData%\TrackingComparison.
// Lives in Core (no Revit/LiteDB/WPF dependency) so both Persistence (LiteDbOptions) and the Revit
// logging bootstrap (PluginLog) derive the same root instead of each hardcoding it.
public static class TrackingDataPaths
{
    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TrackingComparison");

    public static string LogDirectory => Path.Combine(Root, "log");
}
