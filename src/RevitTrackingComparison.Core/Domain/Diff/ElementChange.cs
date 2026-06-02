namespace RevitTrackingComparison.Core.Domain.Diff;

public sealed class ElementChange
{
    public required ChangeType ChangeType { get; init; }

    public required string UniqueId { get; init; }

    public ElementSnapshot? Before { get; init; }

    public ElementSnapshot? After { get; init; }

    public IReadOnlyList<ParameterChange> ChangedParameters { get; init; }
        = Array.Empty<ParameterChange>();
}