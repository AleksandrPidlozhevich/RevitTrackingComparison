using Autodesk.Revit.DB;
using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Revit.Snapshots;

public static class WarningMapper
{
    public static WarningRecord Map(FailureMessage message, Document doc)
    {
        var failing = message.GetFailingElements()
            .Select(id => doc.GetElement(id)?.UniqueId)
            .Where(uid => !string.IsNullOrEmpty(uid))
            .Select(uid => uid!)
            .ToList();

        return new WarningRecord
        {
            DefinitionGuid = message.GetFailureDefinitionId().Guid.ToString(),
            Description = message.GetDescriptionText(),
            FailingElements = failing,
            CreatedBy = Environment.UserName.ToLowerInvariant(),
            CreatedAt = DateTime.Now
        };
    }
}