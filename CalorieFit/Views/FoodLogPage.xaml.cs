using System;
using System.Collections.Generic;
using System.Text;

using CalorieFit.Models;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class FoodLogPage : ContentPage
{
    private readonly DatabaseService _dbService;

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
        // var items = await _dbService.GetFoodEntriesByDateAsync(LogDatePicker.Date);
        FoodCollectionView.ItemsSource = items;
        SummaryLabel.Text = $"Total: {items.Sum(x => x.Calories):F0} kcal | Proteine: {items.Sum(x => x.ProteinGrams):F0}g";
    }

    private async void OnDateSelected(object sender, DateChangedEventArgs e) => await RefreshList();
    private async void OnFilterClicked(object sender, EventArgs e) => await RefreshList();

    private async void OnDeleteEntryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is FoodEntry entry)
        {
            await _dbService.DeleteFoodEntryAsync(entry);
            await RefreshList();
        }
    }
}