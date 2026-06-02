using System.Globalization;

namespace RevitTrackingComparison.Core.Domain;

/// <summary>
/// Lightweight handle to a stored snapshot. Used to list and pick snapshots without loading their
/// full element data. <see cref="Id"/> is an opaque token owned by the store.
/// </summary>
public sealed class SnapshotInfo
{
    public string Project { get; init; } = string.Empty;
    public SnapshotId Id { get; init; }

    public DateTime CapturedAt { get; init; }

    public string DisplayName =>
        CapturedAt.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
}