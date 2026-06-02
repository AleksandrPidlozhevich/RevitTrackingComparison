using System.Globalization;

namespace RevitTrackingComparison.Core.Domain;

/// <summary>
/// Lightweight handle to a stored snapshot (one file). Used to list and pick snapshots without
/// loading their full element data.
/// </summary>
public sealed class SnapshotInfo
{
    public string Project { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;

    public DateTime CapturedAt { get; init; }

    public string DisplayName =>
        CapturedAt.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
}