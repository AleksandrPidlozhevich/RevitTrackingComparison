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

public readonly record struct SnapshotProgress(SnapshotProgressPhase Phase, int ElementCount = 0);
