using System;
using System.Collections.Generic;
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

    private void OnProteinSliderChanged(object sender, ValueChangedEventArgs e)
    {
        SliderLabel.Text = $"Proteine aproximative: {e.NewValue:F0} g";
    }

    private async void OnSaveMealClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FoodNameEntry.Text) || !double.TryParse(CaloriesEntry.Text, out double calories))
        {
            await DisplayAlert("Eroare", "Vă rugăm introduceți numele alimentului și caloriile.", "OK");
            return;
        }

        var entry = new FoodEntry
        {
            FoodName = FoodNameEntry.Text.Trim(),
            MealType = MealTypePicker.SelectedItem?.ToString() ?? "Mic Dejun",
            Calories = calories,
            ProteinGrams = ProteinSlider.Value,
            DateLogged = DateTime.Today
        };

        await _dbService.AddFoodEntryAsync(entry);
        await DisplayAlert("Succes", "Aliment salvat cu succes!", "OK");

        FoodNameEntry.Text = string.Empty;
        CaloriesEntry.Text = string.Empty;
    }
}