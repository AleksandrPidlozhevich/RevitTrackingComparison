using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain.Diff;

namespace RevitTrackingComparison.UI.ViewModels;

// One row of the modified-element parameter grid: the diff values plus inline editing that writes the
// new value back into Revit via IModelEditor. CanEdit is false when no editor is available.
public partial class ParameterRowViewModel : ObservableObject
{
    private readonly IModelEditor? _editor;
    private readonly string _uniqueId;
    private readonly string? _originalNewValue;

    public ParameterRowViewModel(ParameterChange change, string uniqueId, IModelEditor? editor)
    {
        Name = change.Name;
        OldValue = change.OldValue;
        ValuesDiffer = change.ValuesDiffer;
        _originalNewValue = change.NewValue;
        _newValue = change.NewValue;
        _uniqueId = uniqueId;
        _editor = editor;
    }

    public string Name { get; }
    public string? OldValue { get; }
    public bool ValuesDiffer { get; }
    public bool CanEdit => _editor is not null;

    [ObservableProperty] private string? _newValue;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _status = string.Empty;

    partial void OnIsBusyChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void BeginEdit()
    {
        Status = string.Empty;
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        NewValue = _originalNewValue;
        Status = string.Empty;
        IsEditing = false;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (_editor is null)
            return;

        IsBusy = true;
        Status = "Saving…";
        try
        {
            var result = await _editor.SetParameterValueAsync(_uniqueId, Name, NewValue ?? string.Empty);
            Status = result.Success ? "Saved" : $"Error: {result.Message}";
            if (result.Success)
                IsEditing = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave => !IsBusy && _editor is not null;
}