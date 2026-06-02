using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Core.Services;

namespace RevitTrackingComparison.UI.ViewModels;

public partial class SnapshotExportViewModel : ObservableObject
{
    private readonly ISnapshotStore _store;
    private readonly IPluginLogger _logger;
    private readonly Func<string, string?> _promptSavePath;

    public ObservableCollection<SnapshotInfo> Snapshots { get; } = new();

    [ObservableProperty] private SnapshotInfo? _selectedSnapshot;

    [ObservableProperty] private string _status = string.Empty;

    [ObservableProperty] private bool _isExporting;

    public string Project { get; }

    // promptSavePath: given a suggested file name, returns the chosen full path or null if cancelled.
    public SnapshotExportViewModel(
        ISnapshotStore store,
        IPluginLogger<SnapshotExportViewModel> logger,
        string project,
        Func<string, string?> promptSavePath)
    {
        _store = store;
        _logger = logger;
        Project = project;
        _promptSavePath = promptSavePath;
    }

    // Triggered when the window loads; keeps file I/O off the constructor (and off the UI thread).
    public async Task InitializeAsync()
    {
        try
        {
            var infos = await _store.ListAsync(Project);
            Snapshots.Clear();
            foreach (var info in infos)
                Snapshots.Add(info);

            SelectedSnapshot = Snapshots.FirstOrDefault();
            Status = Snapshots.Count == 0 ? "No snapshots available for this project." : string.Empty;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to load snapshots for '{Project}'.");
            Status = "Could not load snapshots.";
        }
    }

    partial void OnSelectedSnapshotChanged(SnapshotInfo? value)
    {
        ExportCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsExportingChanged(bool value)
    {
        ExportCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync()
    {
        if (SelectedSnapshot is null)
            return;

        var path = _promptSavePath(SuggestFileName(SelectedSnapshot));
        if (path is null)
            return;

        IsExporting = true;
        Status = "Exporting…";
        try
        {
            var snapshot = await _store.LoadAsync(SelectedSnapshot);
            if (snapshot is null)
            {
                _logger.Warn($"CSV export failed for '{Project}': could not load '{SelectedSnapshot.Id}'.");
                Status = "Could not load the snapshot.";
                return;
            }

            await Task.Run(() => SnapshotCsvExporter.ExportToFile(snapshot, path));
            _logger.Info(
                $"Exported snapshot '{SelectedSnapshot.Id}' to '{path}' ({snapshot.Elements.Count} elements).");
            Status = $"Exported {snapshot.Elements.Count} elements to {Path.GetFileName(path)}.";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"CSV export failed for '{Project}' snapshot '{SelectedSnapshot.Id}'.");
            Status = "Export failed.";
        }
        finally
        {
            IsExporting = false;
        }
    }

    private bool CanExport => SelectedSnapshot is not null && !IsExporting && Snapshots.Count > 0;

    private static string SuggestFileName(SnapshotInfo info)
    {
        var stamp = info.CapturedAt.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var project = SanitizeFileName(info.Project);
        return $"{project}_{stamp}.csv";
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }
}