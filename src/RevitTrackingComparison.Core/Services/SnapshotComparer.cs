using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Core.Domain.Diff;

namespace RevitTrackingComparison.Core.Services;

public sealed class SnapshotComparer : ISnapshotComparer
{
    public SnapshotDiff Compare(DocumentSnapshot from, DocumentSnapshot to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        var before = from.Elements.ToDictionary(e => e.UniqueId);
        var after = to.Elements.ToDictionary(e => e.UniqueId);

        var added = new List<ElementChange>();
        var removed = new List<ElementChange>();
        var modified = new List<ElementChange>();

        foreach (var (uniqueId, newElement) in after)
        {
            if (!before.TryGetValue(uniqueId, out var oldElement))
            {
                added.Add(new ElementChange
                {
                    ChangeType = ChangeType.Added,
                    UniqueId = uniqueId,
                    After = newElement
                });
                continue;
            }

            var parameterChanges = DiffParameters(oldElement, newElement);
            if (parameterChanges.Count > 0)
                modified.Add(new ElementChange
                {
                    ChangeType = ChangeType.Modified,
                    UniqueId = uniqueId,
                    Before = oldElement,
                    After = newElement,
                    ChangedParameters = parameterChanges
                });
        }

        foreach (var (uniqueId, oldElement) in before)
            if (!after.ContainsKey(uniqueId))
                removed.Add(new ElementChange
                {
                    ChangeType = ChangeType.Removed,
                    UniqueId = uniqueId,
                    Before = oldElement
                });

        return new SnapshotDiff
        {
            FromSnapshotId = from.Id,
            ToSnapshotId = to.Id,
            Added = added,
            Removed = removed,
            Modified = modified
        };
    }

    private static List<ParameterChange> DiffParameters(ElementSnapshot oldElement, ElementSnapshot newElement)
    {
        var changes = new List<ParameterChange>();
        var names = new HashSet<string>(oldElement.Parameters.Keys);
        names.UnionWith(newElement.Parameters.Keys);

        foreach (var name in names)
        {
            oldElement.Parameters.TryGetValue(name, out var oldValue);
            newElement.Parameters.TryGetValue(name, out var newValue);

            if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
                changes.Add(new ParameterChange
                {
                    Name = name,
                    OldValue = oldValue,
                    NewValue = newValue
                });
        }

        return changes;
    }
}