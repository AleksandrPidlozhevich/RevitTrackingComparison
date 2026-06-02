using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTrackingComparison.UI.ViewModels;

/// <summary>
/// A category in the capture settings: an include checkbox plus its available parameters (each
/// selectable). The category is recorded into snapshots when <see cref="IsIncluded"/> is true.
/// </summary>
public partial class CategoryNodeViewModel : ObservableObject
{
    public CategoryNodeViewModel(string name, bool isIncluded)
    {
        Name = name;
        _isIncluded = isIncluded;
    }

    public string Name { get; }

    [ObservableProperty]
    private bool _isIncluded;

    public ObservableCollection<ParameterChoiceViewModel> Parameters { get; } = new();
}
