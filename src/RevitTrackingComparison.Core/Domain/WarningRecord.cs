namespace RevitTrackingComparison.Core.Domain;

public sealed class WarningRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string DefinitionGuid { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<string> FailingElements { get; init; } = Array.Empty<string>();

    public string CreatedBy { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; } = DateTime.Now;
}