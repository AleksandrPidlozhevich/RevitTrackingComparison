using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Outcome of a request to capture and store a snapshot. <paramref name="Info"/> is the created
/// snapshot on success; <paramref name="Message"/> carries a user-facing reason on failure.
/// </summary>
public sealed record SnapshotResult(bool Success, string Message, SnapshotInfo? Info)
{
    public static SnapshotResult Ok(SnapshotInfo info) => new(true, string.Empty, info);

    public static SnapshotResult Fail(string message) => new(false, message, null);
}
