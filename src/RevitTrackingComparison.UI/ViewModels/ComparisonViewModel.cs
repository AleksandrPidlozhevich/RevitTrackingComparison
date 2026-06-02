using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain.Diff;

namespace RevitTrackingComparison.UI.ViewModels;

public partial class ComparisonViewModel : ObservableObject
{
    private readonly IModelEditor? _editor;

    public ComparisonViewModel(IModelEditor? editor = null)
    {
        _editor = editor;
    }

    [ObservableProperty] private string _title = "Snapshot comparison";

    [ObservableProperty] private bool _hasChanges;

    public ObservableCollection<ElementChange> Added { get; } = new();
    public ObservableCollection<ElementChange> Removed { get; } = new();
    public ObservableCollection<ElementChange> Modified { get; } = new();

    [ObservableProperty] private ElementChange? _selectedModified;
    [ObservableProperty] private ElementChange? _selectedAdded;

    // Rebuilt for the selected element; each row supports inline editing back into Revit.
    public ObservableCollection<ParameterRowViewModel> SelectedParameters { get; } = new();
    public ObservableCollection<ParameterRowViewModel> SelectedAddedParameters { get; } = new();

    partial void OnSelectedModifiedChanged(ElementChange? value) => Populate(SelectedParameters, value);

    partial void OnSelectedAddedChanged(ElementChange? value) => Populate(SelectedAddedParameters, value);

    private void Populate(ObservableCollection<ParameterRowViewModel> target, ElementChange? element)
    {
        target.Clear();
        if (element is null)
            return;

        var before = element.Before?.Parameters;
        var after = element.After?.Parameters;

        // Show every captured parameter (After order first, then any that exist only in Before);
        // ParameterChange.ValuesDiffer drives the highlight of the ones that actually changed.
        var names = (after?.Keys ?? Enumerable.Empty<string>())
            .Concat(before?.Keys ?? Enumerable.Empty<string>())
            .Distinct(StringComparer.Ordinal);

        foreach (var name in names)
        {
            var change = new ParameterChange
            {
                Name = name,
                OldValue = before is not null && before.TryGetValue(name, out var old) ? old : null,
                NewValue = after is not null && after.TryGetValue(name, out var @new) ? @new : null
            };
            target.Add(new ParameterRowViewModel(change, element.UniqueId, _editor));
        }
    }

    public int AddedCount => Added.Count;
    public int RemovedCount => Removed.Count;
    public int ModifiedCount => Modified.Count;

    public void Load(SnapshotDiff? diff)
    {
        ReplaceAll(Added, diff?.Added);
        ReplaceAll(Removed, diff?.Removed);
        ReplaceAll(Modified, diff?.Modified);
        SelectedModified = null;

        if (diff is null)
        {
            HasChanges = false;
            Title = "No changes detected (no previous snapshot)";
        }
        else
        {
            HasChanges = diff.HasChanges;
            Title = HasChanges ? "Changes detected" : "No changes detected";
        }

        OnPropertyChanged(nameof(AddedCount));
        OnPropertyChanged(nameof(RemovedCount));
        OnPropertyChanged(nameof(ModifiedCount));
    }

    private static void ReplaceAll(
        ObservableCollection<ElementChange> target,
        IReadOnlyList<ElementChange>? source)
    {
        target.Clear();
        if (source is null || source.Count == 0)
            return;

        foreach (var item in source)
            target.Add(item);
    }
}