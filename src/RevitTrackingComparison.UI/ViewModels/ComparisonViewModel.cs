using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RevitTrackingComparison.Core.Domain.Diff;

namespace RevitTrackingComparison.UI.ViewModels;

public partial class ComparisonViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "Snapshot comparison";

    [ObservableProperty] private bool _hasChanges;

    public ObservableCollection<ElementChange> Added { get; } = new();
    public ObservableCollection<ElementChange> Removed { get; } = new();
    public ObservableCollection<ElementChange> Modified { get; } = new();

    [ObservableProperty] private ElementChange? _selectedModified;

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