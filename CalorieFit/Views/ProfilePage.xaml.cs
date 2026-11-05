using System;
using System.Collections.Generic;
using System.Text;

using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class ProfilePage : ContentPage
{
    private readonly DatabaseService _dbService;

    public ProfilePage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
        ActivityPicker.SelectedIndex = 1;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var p = await _dbService.GetProfileAsync();
        NameEntry.Text = p.Name;
        WeightEntry.Text = p.WeightKg.ToString();
        HeightEntry.Text = p.HeightCm.ToString();
        AgeEntry.Text = p.Age.ToString();
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        if (!AcceptTermsCheck.IsChecked)
        {
            await DisplayAlert("Atenție", "Trebuie să bifați consimțământul.", "OK");
            return;
        }

        double.TryParse(WeightEntry.Text, out double weight);
        double.TryParse(HeightEntry.Text, out double height);
        int.TryParse(AgeEntry.Text, out int age);

        // Formula Harris-Benedict simplificata pentru BMR
        int calculatedCalories = (int)(10 * weight + 6.25 * height - 5 * age + 500);

        var p = await _dbService.GetProfileAsync();
        p.Name = NameEntry.Text;
        p.WeightKg = weight;
        p.HeightCm = height;
        p.Age = age;
        p.ActivityLevel = ActivityPicker.SelectedItem?.ToString() ?? "Moderat";
        p.DailyCalorieTarget = calculatedCalories > 1200 ? calculatedCalories : 2000;

        await _dbService.SaveProfileAsync(p);
        await DisplayAlert("Salvat", $"Profil actualizat! Ținta calculată: {p.DailyCalorieTarget} kcal/zi", "OK");
    }
}