using Autodesk.Revit.DB;
using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Infrastructure;

public sealed class FailureSuppressor : IFailuresPreprocessor
{
    private static readonly IPluginLogger Logger = PluginLog.For(nameof(FailureSuppressor));

    public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
    {
        var document = failuresAccessor.GetDocument();
        var messages = failuresAccessor.GetFailureMessages();

        var suppressedWarning = false;
        var hasError = false;

        foreach (var message in messages)
            if (message.GetSeverity() == FailureSeverity.Warning)
            {
                Log("WARN", message, document);
                failuresAccessor.DeleteWarning(message);
                suppressedWarning = true;
            }
            else
            {
                Log("ERROR", message, document);
                hasError = true;
            }

        if (hasError)
            return FailureProcessingResult.ProceedWithRollBack;

        return suppressedWarning
            ? FailureProcessingResult.ProceedWithCommit
            : FailureProcessingResult.Continue;
    }

    private static void Log(string kind, FailureMessageAccessor message, Document document)
    {
        var elements = message.GetFailingElementIds()
            .Select(id => Describe(document, id))
            .ToList();

        var where = elements.Count == 0 ? string.Empty : $" [{string.Join(", ", elements)}]";
        var prefix = kind == "ERROR" ? "Revit transaction error" : "Suppressed Revit warning";
        var text = $"{prefix}: {message.GetDescriptionText()}{where}";
        if (kind == "ERROR")
            Logger.Error(null, text);
        else
            Logger.Warn(text);
    }

    private static string Describe(Document document, ElementId id)
    {
        var element = document.GetElement(id);
        return element is null ? id.Value.ToString() : $"{element.GetType().Name}#{id.Value}";
    }
}