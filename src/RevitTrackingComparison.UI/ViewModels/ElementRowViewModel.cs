using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTrackingComparison.Core.Domain.Diff;

namespace RevitTrackingComparison.UI.ViewModels;

// Flat row for the element tables: exposes Category/Id/UniqueId/Name as simple properties so the
// DataGrid can sort and filter on them directly. Source keeps the diff entry for the detail view.
public partial class ElementRowViewModel : ObservableObject
{
    public ElementRowViewModel(ElementChange source, bool useAfter)
    {
        Source = source;
        var snapshot = useAfter ? source.After : source.Before;
        Category = snapshot?.Category ?? string.Empty;
        ElementId = snapshot?.ElementId ?? 0;
        Name = snapshot?.Name ?? string.Empty;
        UniqueId = source.UniqueId;
        Changes = source.ChangedParameters.Count;
    }

    public ElementChange Source { get; }
    public string Category { get; }
    public long ElementId { get; }
    public string UniqueId { get; }
    public string Name { get; }
    public int Changes { get; }

    [RelayCommand] private void CopyId() => Copy(ElementId.ToString(CultureInfo.InvariantCulture));
    [RelayCommand] private void CopyUniqueId() => Copy(UniqueId);
    [RelayCommand] private void CopyName() => Copy(Name);

    private static void Copy(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        try
        {
            Clipboard.SetText(text);
        }
        catch
        {
            // The clipboard can be momentarily locked by another process; ignore.
        }
    }
}
