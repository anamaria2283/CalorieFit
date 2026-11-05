using System;
using System.Collections.Generic;
using System.Text;

namespace CalorieFit.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void OnDarkModeToggled(object sender, ToggledEventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        }
    }

    private async void OnClearDataClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Resetare", "Doriți să resetați preferințele cache?", "Da", "Nu");
        if (confirm)
        {
            await DisplayAlert("Succes", "Preferințele au fost resetate.", "OK");
        }
    }

    private async void OnAboutClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Despre", "Aplicația CalorieFit\nProiect realizat de Echipa 15 (.NET 10.0 MAUI).", "OK");
    }
}
