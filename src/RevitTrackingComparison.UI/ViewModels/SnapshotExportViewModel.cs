using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Core.Services;

namespace RevitTrackingComparison.UI.ViewModels;

public partial class SnapshotExportViewModel : ObservableObject
{
    private readonly ISnapshotStore _store;
    private readonly IPluginLogger _logger;

    public ObservableCollection<SnapshotInfo> Snapshots { get; } = new();

    [ObservableProperty]
    private SnapshotInfo? _selectedSnapshot;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private bool _isExporting;

    public string Project { get; }

    public SnapshotExportViewModel(ISnapshotStore store, IPluginLogger logger, string project)
    {
        _store = store;
        _logger = logger;
        Project = project;

        foreach (var info in store.List(project))
            Snapshots.Add(info);

        SelectedSnapshot = Snapshots.FirstOrDefault();
        Status = Snapshots.Count == 0 ? "No snapshots available for this project." : string.Empty;
    }

    partial void OnSelectedSnapshotChanged(SnapshotInfo? value) => ExportCommand.NotifyCanExecuteChanged();

    partial void OnIsExportingChanged(bool value) => ExportCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Export()
    {
        if (SelectedSnapshot is null)
            return;

        var dialog = new SaveFileDialog
        {
            Title = "Export snapshot to CSV",
            Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = "csv",
            FileName = SuggestFileName(SelectedSnapshot),
            AddExtension = true
        };

        if (dialog.ShowDialog() != true)
            return;

        IsExporting = true;
        Status = "Exporting…";
        try
        {
            var snapshot = _store.Load(SelectedSnapshot);
            if (snapshot is null)
            {
                _logger.Warn($"CSV export failed for '{Project}': could not load '{SelectedSnapshot.FileName}'.");
                Status = "Could not load the snapshot.";
                return;
            }

            SnapshotCsvExporter.ExportToFile(snapshot, dialog.FileName);
            _logger.Info(
                $"Exported snapshot '{SelectedSnapshot.FileName}' to '{dialog.FileName}' ({snapshot.Elements.Count} elements).");
            Status = $"Exported {snapshot.Elements.Count} elements to {Path.GetFileName(dialog.FileName)}.";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"CSV export failed for '{Project}' snapshot '{SelectedSnapshot.FileName}'.");
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
