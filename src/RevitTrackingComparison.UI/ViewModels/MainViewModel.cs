using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.UI.ViewModels;

/// <summary>
/// Hub window: take a snapshot, open capture settings, or open the snapshot comparison.
/// Settings/compare windows are opened through delegates supplied by the view service.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ISnapshotCommands _commands;
    private readonly IPluginLogger _logger;
    private readonly Action _openSettings;
    private readonly Action _openCompare;

    public MainViewModel(
        ISnapshotCommands commands,
        IPluginLogger logger,
        string project,
        Action openSettings,
        Action openCompare)
    {
        _commands = commands;
        _logger = logger;
        _openSettings = openSettings;
        _openCompare = openCompare;
        Project = project;
    }

    public string Project { get; }

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    partial void OnIsBusyChanged(bool value) => TakeSnapshotCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanTakeSnapshot))]
    private async Task TakeSnapshotAsync()
    {
        IsBusy = true;
        Status = "Reading model…";
        var progress = new Progress<SnapshotProgress>(OnSnapshotProgress);
        try
        {
            var result = await _commands.TakeSnapshotAsync(progress);
            if (result.Success)
            {
                _logger.Info($"Manual snapshot completed for '{Project}': {result.Info?.DisplayName}.");
                Status = $"Saved: {result.Info?.DisplayName}";
            }
            else
            {
                _logger.Warn($"Manual snapshot failed for '{Project}': {result.Message}");
                Status = $"Error: {result.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Unexpected error during manual snapshot for '{Project}'.");
            Status = "Error: Snapshot failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnSnapshotProgress(SnapshotProgress update)
    {
        if (update.Phase == SnapshotProgressPhase.Saving)
        {
            IsBusy = false;
            Status = update.Current > 0
                ? $"Captured {update.Current} elements. Saving…"
                : "Saving snapshot…";
            return;
        }

        Status = update.Total > 0
            ? $"Reading model… {update.Current}/{update.Total}"
            : "Reading model…";
    }

    private bool CanTakeSnapshot => !IsBusy;

    [RelayCommand]
    private void OpenSettings() => _openSettings();

    [RelayCommand]
    private void OpenCompare() => _openCompare();
}
