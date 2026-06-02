using CommunityToolkit.Mvvm.ComponentModel;

namespace RevitTrackingComparison.UI.ViewModels;

/// <summary>A selectable parameter in the capture settings (checkbox = recorded into snapshots).</summary>
public partial class ParameterChoiceViewModel : ObservableObject
{
    public ParameterChoiceViewModel(string name, bool isSelected)
    {
        Name = name;
        _isSelected = isSelected;
    }

    public string Name { get; }

    [ObservableProperty] private bool _isSelected;
}