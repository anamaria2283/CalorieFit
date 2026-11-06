using System;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class MacroGoalsPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public MacroGoalsPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var profile = await _dbService.GetProfileAsync();
        ProteinGoalSlider.Value = Math.Clamp(profile.ProteinGoalGrams, 0, 500);
        CarbsGoalSlider.Value = Math.Clamp(profile.CarbsGoalGrams, 0, 500);
        FatGoalSlider.Value = Math.Clamp(profile.FatGoalGrams, 0, 500);
    }

    private async void OnApplyGoalsClicked(object sender, EventArgs e)
    {
        var profile = await _dbService.GetProfileAsync();
        profile.ProteinGoalGrams = ProteinGoalSlider.Value;
        profile.CarbsGoalGrams = CarbsGoalSlider.Value;
        profile.FatGoalGrams = FatGoalSlider.Value;
        await _dbService.SaveProfileAsync(profile);

        await DisplayAlertAsync("Succes", "Obiectivele zilnice au fost salvate.", "OK");
    }

}