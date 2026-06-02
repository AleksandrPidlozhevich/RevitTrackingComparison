namespace RevitTrackingComparison.Core.Abstractions;

public enum SnapshotProgressPhase
{
    Capturing,
    Saving
}

// Capturing: Current = elements read so far out of Total. Saving: Current = captured element count.
public readonly record struct SnapshotProgress(SnapshotProgressPhase Phase, int Current = 0, int Total = 0);