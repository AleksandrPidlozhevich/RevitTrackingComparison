using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.UI.ViewModels;
using RevitTrackingComparison.UI.Views;

namespace RevitTrackingComparison.UI.Services;

/// <summary>
/// Opens the snapshot hub and its child windows (settings, compare). All windows are modeless
/// (<c>Show()</c>) — required because taking a snapshot goes through an ExternalEvent.
/// </summary>
public sealed class SnapshotHubView : ISnapshotHubView
{
    private readonly ISnapshotCommands _commands;
    private readonly ISnapshotStore _store;
    private readonly ISnapshotComparer _comparer;
    private readonly ICaptureSettingsStore _settingsStore;
    private readonly IModelMetadataProvider _metadata;

    public SnapshotHubView(
        ISnapshotCommands commands,
        ISnapshotStore store,
        ISnapshotComparer comparer,
        ICaptureSettingsStore settingsStore,
        IModelMetadataProvider metadata)
    {
        _commands = commands;
        _store = store;
        _comparer = comparer;
        _settingsStore = settingsStore;
        _metadata = metadata;
    }

    public void Show(string project)
    {
        var viewModel = new MainViewModel(_commands, project, OpenSettings, () => OpenCompare(project));
        new MainWindow(viewModel).Show();
    }

    private void OpenSettings()
    {
        var viewModel = new CaptureSettingsViewModel(_settingsStore, _metadata);
        new CaptureSettingsWindow(viewModel).Show();
    }

    private void OpenCompare(string project)
    {
        var viewModel = new SnapshotCompareViewModel(_store, _comparer, project);
        new SnapshotCompareWindow(viewModel).Show();
    }
}
