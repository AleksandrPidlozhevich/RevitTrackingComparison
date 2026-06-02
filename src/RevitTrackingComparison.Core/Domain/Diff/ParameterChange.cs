namespace RevitTrackingComparison.Core.Domain.Diff;

public sealed class ParameterChange
{
    public string Name { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }

    public bool ValuesDiffer => !string.Equals(OldValue, NewValue, StringComparison.Ordinal);
}