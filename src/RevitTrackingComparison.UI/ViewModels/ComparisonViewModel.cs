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

    public int AddedCount => Added.Count;
    public int RemovedCount => Removed.Count;
    public int ModifiedCount => Modified.Count;

    public void Load(SnapshotDiff? diff)
    {
        Added.Clear();
        Removed.Clear();
        Modified.Clear();

        if (diff is null)
        {
            HasChanges = false;
            Title = "No changes detected (no previous snapshot)";
        }
        else
        {
            foreach (var change in diff.Added) Added.Add(change);
            foreach (var change in diff.Removed) Removed.Add(change);
            foreach (var change in diff.Modified) Modified.Add(change);
            HasChanges = diff.HasChanges;
            Title = HasChanges ? "Changes detected" : "No changes detected";
        }

        OnPropertyChanged(nameof(AddedCount));
        OnPropertyChanged(nameof(RemovedCount));
        OnPropertyChanged(nameof(ModifiedCount));
    }
}