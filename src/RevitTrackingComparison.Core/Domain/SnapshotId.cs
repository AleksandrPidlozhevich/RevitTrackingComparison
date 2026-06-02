namespace RevitTrackingComparison.Core.Domain;

public readonly record struct SnapshotId(string Value)
{
    public override string ToString() => Value;
}
