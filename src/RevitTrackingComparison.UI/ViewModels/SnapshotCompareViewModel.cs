using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.UI.Views;

namespace RevitTrackingComparison.UI.ViewModels;

/// <summary>
/// Lets the user pick two stored snapshots of the project and compare them; the diff is shown by
/// reusing <see cref="ComparisonWindow"/>.
/// </summary>
public partial class SnapshotCompareViewModel : ObservableObject
{
    private readonly ISnapshotStore _store;
    private readonly ISnapshotComparer _comparer;
    private readonly IModelEditor _editor;

    public ObservableCollection<SnapshotInfo> Snapshots { get; } = new();

    [ObservableProperty]
    private SnapshotInfo? _from;

    [ObservableProperty]
    private SnapshotInfo? _to;

    [ObservableProperty]
    private string _status = string.Empty;

    public SnapshotCompareViewModel(ISnapshotStore store, ISnapshotComparer comparer, IModelEditor editor, string project)
    {
        _store = store;
        _comparer = comparer;
        _editor = editor;

        foreach (var info in store.List(project)) // newest first
            Snapshots.Add(info);

        To = Snapshots.FirstOrDefault();                            // newest
        From = Snapshots.Count > 1 ? Snapshots[^1] : Snapshots.FirstOrDefault(); // oldest
    }

    [RelayCommand]
    private void Compare()
    {
        if (From is null || To is null)
        {
            Status = "Select two snapshots.";
            return;
        }

        var from = _store.Load(From);
        var to = _store.Load(To);
        if (from is null || to is null)
        {
            Status = "Could not load the snapshots.";
            return;
        }

        var diff = _comparer.Compare(from, to);
        var viewModel = new ComparisonViewModel(_editor);
        viewModel.Load(diff);
        new ComparisonWindow(viewModel).Show();
        Status = string.Empty;
    }
}
