namespace RevitTrackingComparison.Core.Abstractions;

public sealed record ParameterEditResult(bool Success, string Message)
{
    public static ParameterEditResult Ok() => new(true, string.Empty);

    public static ParameterEditResult Fail(string message) => new(false, message);
}
