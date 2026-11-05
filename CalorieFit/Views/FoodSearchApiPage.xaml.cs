using System;
using System.Net.Http;
using System.Text.Json;
using CalorieFit.Models;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class FoodSearchApiPage : ContentPage
{
    private readonly NutritionApiService _apiService;
    private readonly DatabaseService _dbService;

    public FoodSearchApiPage(NutritionApiService apiService, DatabaseService dbService)
    {
        InitializeComponent();
        _apiService = apiService;
        _dbService = dbService;
    }

    private async void OnSearchApiClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SearchTermEntry.Text)) return;

        LoadingSpinner.IsVisible = true;
        LoadingSpinner.IsRunning = true;
        SearchStatusLabel.IsVisible = false;
        ApiResultsList.ItemsSource = null;

        try
        {
            var results = await _apiService.SearchNutritionAsync(SearchTermEntry.Text.Trim());
            ApiResultsList.ItemsSource = results;
            if (results.Count == 0)
            {
                SearchStatusLabel.Text = "Nu au fost găsite alimente cu valori nutriționale complete.";
                SearchStatusLabel.IsVisible = true;
            }
        }
        catch (HttpRequestException ex)
        {
            SearchStatusLabel.Text = $"Eroare la conectarea cu USDA FoodData Central: {ex.Message}";
            SearchStatusLabel.IsVisible = true;
        }
        catch (JsonException ex)
        {
            SearchStatusLabel.Text = $"Răspunsul API-ului nu a putut fi interpretat: {ex.Message}";
            SearchStatusLabel.IsVisible = true;
        }
        catch (TaskCanceledException)
        {
            SearchStatusLabel.Text = "Căutarea a expirat. Verificați conexiunea și încercați din nou.";
            SearchStatusLabel.IsVisible = true;
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }

    private async void OnAddApiItemToDb(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ApiFoodItem item)
        {
            var entry = new FoodEntry
            {
                FoodName = item.Name,
                Calories = item.Calories,
                ProteinGrams = item.ProteinG,
                CarbsGrams = item.CarbsG,
                FatGrams = item.FatG,
                MealType = "Gustare",
                DateLogged = DateTime.Today
            };

            await _dbService.AddFoodEntryAsync(entry);
            await DisplayAlertAsync("Adăugat", $"{item.Name} (100 g) a fost salvat în SQLite!", "OK");
        }
    }
}