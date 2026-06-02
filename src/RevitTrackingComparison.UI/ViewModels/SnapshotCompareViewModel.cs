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
    private readonly IPluginLogger _logger;

    public ObservableCollection<SnapshotInfo> Snapshots { get; } = new();

    [ObservableProperty] private SnapshotInfo? _from;

    [ObservableProperty] private SnapshotInfo? _to;

    [ObservableProperty] private string _status = string.Empty;

    public string Project { get; }

    public SnapshotCompareViewModel(
        ISnapshotStore store,
        ISnapshotComparer comparer,
        IModelEditor editor,
        IPluginLogger<SnapshotCompareViewModel> logger,
        string project)
    {
        _store = store;
        _comparer = comparer;
        _editor = editor;
        _logger = logger;
        Project = project;
    }

    // Triggered when the window loads; keeps file I/O off the constructor (and off the UI thread).
    public async Task InitializeAsync()
    {
        try
        {
            var infos = await _store.ListAsync(Project);
            Snapshots.Clear();
            foreach (var info in infos) // newest first
                Snapshots.Add(info);

            To = Snapshots.FirstOrDefault(); // newest
            From = Snapshots.Count > 1 ? Snapshots[^1] : Snapshots.FirstOrDefault(); // oldest
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to load snapshots for '{Project}'.");
            Status = "Could not load snapshots.";
        }
    }

    [RelayCommand]
    private async Task CompareAsync()
    {
        if (From is null || To is null)
        {
            Status = "Select two snapshots.";
            return;
        }

        try
        {
            var from = await _store.LoadAsync(From);
            var to = await _store.LoadAsync(To);
            if (from is null || to is null)
            {
                _logger.Warn(
                    $"Compare failed for '{Project}': could not load '{From.FileName}' or '{To.FileName}'.");
                Status = "Could not load the snapshots.";
                return;
            }

            var diff = _comparer.Compare(from, to);
            _logger.Info(
                $"Compared snapshots for '{Project}': '{From.FileName}' -> '{To.FileName}' " +
                $"(+{diff.Added.Count} -{diff.Removed.Count} ~{diff.Modified.Count}).");
            var viewModel = new ComparisonViewModel(_editor);
            viewModel.Load(diff);
            new ComparisonWindow(viewModel).Show();
            Status = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                $"Compare failed for '{Project}' ('{From.FileName}' -> '{To.FileName}').");
            Status = "Compare failed.";
        }
    }
}