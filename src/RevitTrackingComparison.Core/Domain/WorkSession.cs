namespace RevitTrackingComparison.Core.Domain;

public sealed class WorkSession
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime StartTime { get; init; }

    public DateTime? EndTime { get; set; }

    public TimeSpan? Duration => EndTime is { } end ? end - StartTime : null;
}