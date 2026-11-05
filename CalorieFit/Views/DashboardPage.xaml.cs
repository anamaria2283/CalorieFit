using System;
using System.Collections.Generic;
using System.Text;

using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public DashboardPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardData();
    }

    private async Task LoadDashboardData()
    {
        var profile = await _dbService.GetProfileAsync();
        var foods = await _dbService.GetFoodEntriesByDateAsync(DateTime.Today);
        var water = await _dbService.GetTodayWaterLogsAsync();

        double totalCalories = foods.Sum(f => f.Calories);
        int totalWater = water.Sum(w => w.Milliliters);

        WelcomeLabel.Text = $"Salut, {profile.Name}!";
        CalorieRatioLabel.Text = $"{totalCalories:F0} / {profile.DailyCalorieTarget} kcal";
        CalorieProgressBar.Progress = Math.Min(1.0, totalCalories / profile.DailyCalorieTarget);

        WaterRatioLabel.Text = $"{totalWater} / 2500 ml";
        WaterProgressBar.Progress = Math.Min(1.0, (double)totalWater / 2500);
    }

    private async void OnRefreshClicked(object sender, EventArgs e) => await LoadDashboardData();
}