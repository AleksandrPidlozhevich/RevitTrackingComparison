using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
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

        AddedView = CollectionViewSource.GetDefaultView(Added);
        RemovedView = CollectionViewSource.GetDefaultView(Removed);
        ModifiedView = CollectionViewSource.GetDefaultView(Modified);
        AddedView.Filter = Matches;
        RemovedView.Filter = Matches;
        ModifiedView.Filter = Matches;
    }

    [ObservableProperty] private string _title = "Snapshot comparison";

    [ObservableProperty] private bool _hasChanges;

    [ObservableProperty] private string _searchText = string.Empty;

    public ObservableCollection<ElementRowViewModel> Added { get; } = new();
    public ObservableCollection<ElementRowViewModel> Removed { get; } = new();
    public ObservableCollection<ElementRowViewModel> Modified { get; } = new();

    // The DataGrids bind to these views, so sorting (header click) and search filtering apply live.
    public ICollectionView AddedView { get; }
    public ICollectionView RemovedView { get; }
    public ICollectionView ModifiedView { get; }

    [ObservableProperty] private ElementRowViewModel? _selectedModified;
    [ObservableProperty] private ElementRowViewModel? _selectedAdded;

    public ObservableCollection<ParameterRowViewModel> SelectedParameters { get; } = new();
    public ObservableCollection<ParameterRowViewModel> SelectedAddedParameters { get; } = new();

    public int AddedCount => Added.Count;
    public int RemovedCount => Removed.Count;
    public int ModifiedCount => Modified.Count;

    public void Load(SnapshotDiff? diff)
    {
        ReplaceAll(Added, diff?.Added, useAfter: true);
        ReplaceAll(Removed, diff?.Removed, useAfter: false);
        ReplaceAll(Modified, diff?.Modified, useAfter: true);
        SelectedModified = null;
        SelectedAdded = null;

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

    partial void OnSearchTextChanged(string value)
    {
        AddedView.Refresh();
        RemovedView.Refresh();
        ModifiedView.Refresh();
    }

    partial void OnSelectedModifiedChanged(ElementRowViewModel? value) => Populate(SelectedParameters, value?.Source);

    partial void OnSelectedAddedChanged(ElementRowViewModel? value) => Populate(SelectedAddedParameters, value?.Source);

    private bool Matches(object item)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        var row = (ElementRowViewModel)item;
        var term = SearchText.Trim();
        return Contains(row.Category, term)
            || Contains(row.Name, term)
            || Contains(row.UniqueId, term)
            || Contains(row.ElementId.ToString(CultureInfo.InvariantCulture), term);

        static bool Contains(string source, string term) =>
            source.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

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

    private static void ReplaceAll(
        ObservableCollection<ElementRowViewModel> target,
        IReadOnlyList<ElementChange>? source,
        bool useAfter)
    {
        target.Clear();
        if (source is null)
            return;

        foreach (var item in source)
            target.Add(new ElementRowViewModel(item, useAfter));
    }
}
