namespace RevitTrackingComparison.Persistence;

public sealed class LiteDbOptions
{
    public string DatabaseFolder { get; set; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TrackingComparison", "Revit");
}