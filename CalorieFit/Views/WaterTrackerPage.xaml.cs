using System;
using System.Collections.Generic;
using System.Text;

using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class WaterTrackerPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public WaterTrackerPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshWaterUI();
    }

    private async Task RefreshWaterUI()
    {
        var logs = await _dbService.GetTodayWaterLogsAsync();
        int total = logs.Sum(w => w.Milliliters);
        WaterStatusLabel.Text = $"Apă băută azi: {total} ml / 2500 ml";
        WaterBar.Progress = Math.Min(1.0, (double)total / 2500);
    }

    private void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
    {
        StepperLabel.Text = $"Porție selectată: {e.NewValue:F0} ml";
    }

    private async void OnAddWaterClicked(object sender, EventArgs e)
    {
        await _dbService.AddWaterLogAsync((int)GlassStepper.Value);
        await RefreshWaterUI();
    }

    private async void OnResetWaterClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirmare", "Sigur dorești să resetezi apa de azi?", "Da", "Nu");
        if (confirm)
        {
            await _dbService.ResetTodayWaterAsync();
            await RefreshWaterUI();
        }
    }
}