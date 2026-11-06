using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using CalorieFit.Models;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class AddMealPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public AddMealPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
        MealTypePicker.SelectedIndex = 0;
    }

    private async void OnSaveMealClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FoodNameEntry.Text)
            || !TryParseRequiredNumber(CaloriesEntry.Text, out var calories)
            || !TryParseOptionalNumber(ProteinEntry.Text, out var protein)
            || !TryParseOptionalNumber(CarbsEntry.Text, out var carbs)
            || !TryParseOptionalNumber(FatEntry.Text, out var fat))
        {
            await DisplayAlertAsync("Eroare", "Introduceți numele, caloriile și valori numerice valide pentru macronutrienții opționali.", "OK");
            return;
        }

        var entry = new FoodEntry
        {
            FoodName = FoodNameEntry.Text.Trim(),
            MealType = MealTypePicker.SelectedItem?.ToString() ?? "Mic Dejun",
            Calories = calories,
            ProteinGrams = protein,
            CarbsGrams = carbs,
            FatGrams = fat,
            DateLogged = DateTime.Today
        };

        try
        {
            await _dbService.AddFoodEntryAsync(entry);
            await DisplayAlertAsync(
                "Succes",
                $"Aliment salvat cu succes!\n\n" +
                $"Proteine: {entry.ProteinGrams:F1} g\n" +
                $"Carbohidrați: {entry.CarbsGrams:F1} g\n" +
                $"Grăsimi: {entry.FatGrams:F1} g",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Eroare la salvare", $"Alimentul nu a putut fi salvat:\n{ex.Message}", "OK");
            return;
        }

        FoodNameEntry.Text = string.Empty;
        CaloriesEntry.Text = string.Empty;
        ProteinEntry.Text = string.Empty;
        CarbsEntry.Text = string.Empty;
        FatEntry.Text = string.Empty;
    }

    private static bool TryParseRequiredNumber(string? text, out double value)
    {
        return TryParseNumber(text, out value) && value >= 0;
    }

    private static bool TryParseOptionalNumber(string? text, out double value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = 0;
            return true;
        }

        return TryParseNumber(text, out value) && value >= 0;
    }

    private static bool TryParseNumber(string? text, out double value)
    {
        var normalizedText = text?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            value = 0;
            return false;
        }

        var commaIndex = normalizedText.LastIndexOf(',');
        var periodIndex = normalizedText.LastIndexOf('.');
        if (commaIndex >= 0 && periodIndex >= 0)
        {
            var decimalSeparator = commaIndex > periodIndex ? ',' : '.';
            var thousandsSeparator = decimalSeparator == ',' ? '.' : ',';
            normalizedText = normalizedText.Replace(thousandsSeparator.ToString(), string.Empty)
                .Replace(decimalSeparator, '.');
        }
        else
        {
            normalizedText = normalizedText.Replace(',', '.');
        }

        return double.TryParse(normalizedText, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            && double.IsFinite(value);
    }
}