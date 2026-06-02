using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.UI.ViewModels;

/// <summary>
/// Editor for the capture configuration. Categories and their available parameters are read from
/// the active model; the user ticks the parameters to record. Existing config selections are kept
/// (and shown checked) even if a category/parameter isn't currently present in the model.
/// </summary>
public partial class CaptureSettingsViewModel : ObservableObject
{
    private static readonly StringComparer Ci = StringComparer.OrdinalIgnoreCase;

    private readonly ICaptureSettingsStore _store;
    private readonly IModelMetadataProvider _metadata;
    private readonly IPluginLogger _logger;

    public ObservableCollection<CategoryNodeViewModel> Categories { get; } = new();

    [ObservableProperty] private CategoryNodeViewModel? _selectedCategory;

    [ObservableProperty] private string _status = string.Empty;

    [ObservableProperty] private bool _isLoading;

    public CaptureSettingsViewModel(
        ICaptureSettingsStore store,
        IModelMetadataProvider metadata,
        IPluginLogger logger)
    {
        _store = store;
        _metadata = metadata;
        _logger = logger;

        // Show the saved config immediately; the model catalog is merged in once loaded.
        Build(store.Load(), new Dictionary<string, IReadOnlyList<string>>());
    }

    /// <summary>Reads categories/parameters from the model and merges them with the saved config.</summary>
    public async Task LoadFromModelAsync()
    {
        IsLoading = true;
        Status = "Reading parameters from the model…";
        try
        {
            var catalog = await _metadata.GetCategoryParametersAsync();
            Build(_store.Load(), catalog);
            Status = $"Loaded {catalog.Count} categories from the model.";
            _logger.Info($"Capture settings loaded {catalog.Count} categories from the model.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load capture settings from the model.");
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private Task Reload()
    {
        return LoadFromModelAsync();
    }

    [RelayCommand]
    private void SelectAllCategories()
    {
        SetAllCategories(true);
    }

    [RelayCommand]
    private void ClearAllCategories()
    {
        SetAllCategories(false);
    }

    [RelayCommand]
    private void SelectAllParameters()
    {
        SetAllParameters(true);
    }

    [RelayCommand]
    private void ClearAllParameters()
    {
        SetAllParameters(false);
    }

    [RelayCommand]
    private void Save()
    {
        var settings = new CaptureSettings
        {
            Rules = Categories
                .Where(category => category.IsIncluded)
                .Select(category => new CaptureRule
                {
                    Category = category.Name,
                    Parameters = category.Parameters.Where(p => p.IsSelected).Select(p => p.Name).ToList()
                })
                .ToList()
        };

        try
        {
            _store.Save(settings);
            Status = "Saved";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save capture settings from the UI.");
            Status = "Error: Could not save settings.";
        }
    }

    private void SetAllCategories(bool included)
    {
        foreach (var category in Categories)
            category.IsIncluded = included;
    }

    private void SetAllParameters(bool selected)
    {
        if (SelectedCategory is null)
            return;

        foreach (var parameter in SelectedCategory.Parameters)
            parameter.IsSelected = selected;
    }

    private void Build(CaptureSettings config, IReadOnlyDictionary<string, IReadOnlyList<string>> catalog)
    {
        Categories.Clear();

        var categoryNames = new SortedSet<string>(Ci);
        foreach (var rule in config.Rules) categoryNames.Add(rule.Category);
        foreach (var category in catalog.Keys) categoryNames.Add(category);

        foreach (var categoryName in categoryNames)
        {
            var rule = config.Rules.FirstOrDefault(r => Ci.Equals(r.Category, categoryName));
            var selected = rule?.Parameters ?? Array.Empty<string>();

            var available = catalog.TryGetValue(categoryName, out var fromModel) ? fromModel : Array.Empty<string>();

            var parameterNames = new SortedSet<string>(Ci);
            foreach (var name in available) parameterNames.Add(name);
            foreach (var name in selected) parameterNames.Add(name); // keep config params not in the model sample

            // Included = the category is part of the saved config.
            var node = new CategoryNodeViewModel(categoryName, rule is not null);
            foreach (var name in parameterNames)
                node.Parameters.Add(new ParameterChoiceViewModel(name, selected.Contains(name, Ci)));

            Categories.Add(node);
        }

        SelectedCategory = Categories.FirstOrDefault();
    }
}