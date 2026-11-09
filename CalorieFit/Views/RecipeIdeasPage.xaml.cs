using CalorieFit.Models;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class RecipeIdeasPage : ContentPage
{
    private readonly NutritionApiService _apiService;
    private List<MockRecipe> _allRecipes = new();
    private string _selectedCategory = "Toate";

    public RecipeIdeasPage(NutritionApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        CategoryPicker.SelectedIndex = 0;
        await LoadRecipes();
    }

    private async Task LoadRecipes()
    {
        _allRecipes = await _apiService.GetHealthyRecipesAsync();
        ApplyFilters();
    }

    private void OnRecipeSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

    private void OnCategoryChanged(object sender, EventArgs e)
    {
        _selectedCategory = CategoryPicker.SelectedItem?.ToString() ?? "Toate";
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var searchText = SearchBar.Text?.Trim() ?? string.Empty;
        var filtered = _allRecipes.Where(recipe =>
            (_selectedCategory == "Toate" || recipe.Category.Equals(_selectedCategory, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(searchText) ||
             recipe.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
             recipe.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
             recipe.Ingredients.Contains(searchText, StringComparison.OrdinalIgnoreCase))).ToList();

        RecipesList.ItemsSource = filtered;
    }

    private async void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not MockRecipe recipe)
            return;

        RecipesList.SelectedItem = null;
        var details = $"{recipe.Category} • {recipe.Calories} kcal • {recipe.CookingTime}\n\n" +
                      $"Ingrediente:\n{recipe.Ingredients}\n\n" +
                      $"Mod de preparare:\n{recipe.Instructions}";
        await DisplayAlert(recipe.Title, details, "Închide");
    }
}