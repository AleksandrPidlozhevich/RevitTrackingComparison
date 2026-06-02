namespace RevitTrackingComparison.Core.Domain;

/// <summary>
/// Lightweight handle to a stored snapshot (one file). Used to list and pick snapshots without
/// loading their full element data.
/// </summary>
public sealed class SnapshotInfo
{
    public string Project { get; init; } = string.Empty;

    /// <summary>File name of the snapshot, e.g. <c>Project_20260602_142530.db</c>.</summary>
    public string FileName { get; init; } = string.Empty;

    public DateTime CapturedAt { get; init; }
}
