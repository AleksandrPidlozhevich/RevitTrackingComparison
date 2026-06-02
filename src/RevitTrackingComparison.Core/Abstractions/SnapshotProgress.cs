namespace RevitTrackingComparison.Core.Abstractions;

/// <summary>
/// Progress reported while <see cref="ISnapshotCommands.TakeSnapshotAsync"/> runs.
/// Capture happens on the Revit API thread; saving runs in the background.
/// </summary>
public enum SnapshotProgressPhase
{
    /// <summary>Reading elements and parameters from the active document.</summary>
    Capturing,

    /// <summary>Model data is in memory; writing the LiteDB file off the API thread.</summary>
    Saving
}

/// <summary>
/// A progress update. During <see cref="SnapshotProgressPhase.Capturing"/>, <paramref name="Current"/>
/// is the number of elements read so far out of <paramref name="Total"/>. During
/// <see cref="SnapshotProgressPhase.Saving"/>, <paramref name="Current"/> is the captured element count.
/// </summary>
public readonly record struct SnapshotProgress(SnapshotProgressPhase Phase, int Current = 0, int Total = 0);
