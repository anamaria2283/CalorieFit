using System;
using System.Collections.Generic;
using System.Text;

using CalorieFit.Models;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class RecipeIdeasPage : ContentPage
{
    private readonly NutritionApiService _apiService;
    private List<MockRecipe> _allRecipes = new();

    public RecipeIdeasPage(NutritionApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRecipes();
    }

    private async Task LoadRecipes()
    {
        _allRecipes = await _apiService.GetHealthyRecipesAsync();
        RecipesList.ItemsSource = _allRecipes;
    }

    private async void OnLoadRecipesClicked(object sender, EventArgs e) => await LoadRecipes();

    private void OnRecipeSearchChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
            RecipesList.ItemsSource = _allRecipes;
        else
            RecipesList.ItemsSource = _allRecipes.Where(r => r.Title.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}