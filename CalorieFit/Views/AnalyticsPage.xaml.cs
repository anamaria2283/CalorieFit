using System;
using System.Collections.Generic;
using System.Text;

using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class AnalyticsPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public AnalyticsPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await UpdateChart();
    }

    private async Task UpdateChart()
    {
        var entries = await _dbService.GetFoodEntriesByDateAsync(DateTime.Today);
        double totalProtein = entries.Sum(e => e.ProteinGrams);
        double totalCarbs = entries.Sum(e => e.CarbsGrams);
        double totalFat = entries.Sum(e => e.FatGrams);

        // Scalare latime bare pe ecran (maxim 280 unitati)
        ProteinBar.WidthRequest = Math.Clamp(totalProtein * 2, 20, 280);
        CarbsBar.WidthRequest = Math.Clamp(totalCarbs * 2, 20, 280);
        FatBar.WidthRequest = Math.Clamp(totalFat * 2, 20, 280);
    }

    private void OnSimTargetChanged(object sender, ValueChangedEventArgs e)
    {
        TargetSimLabel.Text = $"Obiectiv simulat: {e.NewValue:F0} kcal";
    }

    private async void OnRefreshAnalyticsClicked(object sender, EventArgs e) => await UpdateChart();
}