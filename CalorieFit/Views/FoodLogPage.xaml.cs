using System;
using System.Collections.Generic;
using System.Text;

using CalorieFit.Models;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class FoodLogPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private bool? _sortCaloriesDescending;

    public FoodLogPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
        LogDatePicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshList();
    }

    private async Task RefreshList()
    {
        var selectedDate = LogDatePicker.Date ?? DateTime.Today;
        var items = await _dbService.GetFoodEntriesByDateAsync(selectedDate);
        if (_sortCaloriesDescending is true)
        {
            items = items.OrderByDescending(item => item.Calories).ToList();
        }
        else if (_sortCaloriesDescending is false)
        {
            items = items.OrderBy(item => item.Calories).ToList();
        }

        FoodCollectionView.ItemsSource = items;
        SummaryLabel.Text = $"Total: {items.Sum(x => x.Calories):F0} kcal | " +
            $"P: {items.Sum(x => x.ProteinGrams):F1}g | " +
            $"C: {items.Sum(x => x.CarbsGrams):F1}g | " +
            $"G: {items.Sum(x => x.FatGrams):F1}g";
    }

    private async void OnDateSelected(object sender, DateChangedEventArgs e) => await RefreshList();

    private async void OnSortCaloriesClicked(object sender, EventArgs e)
    {
        _sortCaloriesDescending = _sortCaloriesDescending != true;
        SortCaloriesButton.Text = _sortCaloriesDescending.Value ? "Calorii ↓" : "Calorii ↑";
        await RefreshList();
    }

    private async void OnDeleteEntryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is FoodEntry entry)
        {
            await _dbService.DeleteFoodEntryAsync(entry);
            await RefreshList();
        }
    }
}