using System;
using System.Collections.Generic;
using System.Text;

namespace CalorieFit.Views;

public partial class MacroGoalsPage : ContentPage
{
    public MacroGoalsPage()
    {
        InitializeComponent();
    }

    private void OnSliderChanged(object sender, ValueChangedEventArgs e)
    {
        ProteinGoalLabel.Text = $"Proteine: {ProteinGoalSlider.Value:F0}%";
        CarbGoalLabel.Text = $"Carbohidrați: {CarbGoalSlider.Value:F0}%";
        FatGoalLabel.Text = $"Grăsimi: {FatGoalSlider.Value:F0}%";
    }

    private async void OnApplyGoalsClicked(object sender, EventArgs e)
    {
        double total = ProteinGoalSlider.Value + CarbGoalSlider.Value + FatGoalSlider.Value;
        if (Math.Abs(total - 100) > 5)
        {
            await DisplayAlert("Atenție", $"Suma procentelor este {total:F0}%. Recomandat este să fie ~100%.", "OK");
            return;
        }
        await DisplayAlert("Succes", "Obiectivele macronutrienților au fost actualizate!", "OK");
    }
}