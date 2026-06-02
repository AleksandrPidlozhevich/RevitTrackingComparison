namespace RevitTrackingComparison.Core.Abstractions;

public sealed record ParameterEditResult(bool Success, string Message)
{
    public static ParameterEditResult Ok()
    {
        return new ParameterEditResult(true, string.Empty);
    }

    public static ParameterEditResult Fail(string message)
    {
        return new ParameterEditResult(false, message);
    }
}