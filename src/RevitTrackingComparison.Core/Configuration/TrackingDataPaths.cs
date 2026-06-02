namespace RevitTrackingComparison.Core.Configuration;

public static class TrackingDataPaths
{
    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TrackingComparison");

    public static string LogDirectory => Path.Combine(Root, "log");
}
