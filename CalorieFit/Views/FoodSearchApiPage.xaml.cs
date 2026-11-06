using System;
using System.Globalization;
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
        if (sender is not Button btn || btn.CommandParameter is not ApiFoodItem item)
        {
            return;
        }

        var amountText = await DisplayPromptAsync(
            "Cantitate",
            $"Introduceți cantitatea de {item.Name} în grame:",
            "Adaugă",
            "Anulează",
            keyboard: Keyboard.Numeric,
            initialValue: "100");

        if (amountText is null)
        {
            return;
        }

        if ((!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.CurrentCulture, out var grams)
             && !decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out grams))
            || grams <= 0)
        {
            await DisplayAlertAsync("Cantitate invalidă", "Introduceți o cantitate mai mare decât zero.", "OK");
            return;
        }

        var multiplier = (double)(grams / 100m);
        var entry = new FoodEntry
        {
            FoodName = item.Name,
            Calories = item.Calories * multiplier,
            ProteinGrams = item.ProteinG * multiplier,
            CarbsGrams = item.CarbsG * multiplier,
            FatGrams = item.FatG * multiplier,
            MealType = "Gustare",
            DateLogged = DateTime.Today
        };

        await _dbService.AddFoodEntryAsync(entry);
        await DisplayAlertAsync(
            "Adăugat",
            $"{item.Name} ({grams:F0} g) a fost salvat în jurnal!\n\n" +
            $"Calorii: {entry.Calories:F0} kcal\n" +
            $"Proteine: {entry.ProteinGrams:F1} g\n" +
            $"Carbohidrați: {entry.CarbsGrams:F1} g\n" +
            $"Grăsimi: {entry.FatGrams:F1} g",
            "OK");
    }
}