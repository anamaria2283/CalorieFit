using System.Text.Json;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class SettingsPage : ContentPage
{
    private const string DarkModeKey = "settings.dark_mode";
    private readonly DatabaseService _dbService;

    public SettingsPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var darkMode = Preferences.Get(DarkModeKey, false);
        DarkModeSwitch.IsToggled = darkMode;
        ApplyTheme(darkMode);
        await RefreshDatabaseStatusAsync();
    }

    private void OnDarkModeToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set(DarkModeKey, e.Value);
        ApplyTheme(e.Value);
    }

    private static void ApplyTheme(bool darkMode)
    {
        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme = darkMode ? AppTheme.Dark : AppTheme.Light;
        }
    }

    private async Task RefreshDatabaseStatusAsync()
    {
        var counts = await _dbService.GetDataCountsAsync();
        DatabaseStatusLabel.Text = $"Profil: 1 | Mese: {counts.FoodEntries} | Apă: {counts.WaterLogs} | Note: {counts.DayReflections}";
    }

    private async void OnResetPreferencesClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Resetare preferințe", "Reveniți la tema deschisă? Datele din baza de date nu vor fi șterse.", "Da", "Nu");
        if (!confirm)
        {
            return;
        }

        Preferences.Remove(DarkModeKey);
        DarkModeSwitch.IsToggled = false;
        ApplyTheme(false);
        await DisplayAlert("Gata", "Preferințele au fost resetate.", "OK");
    }

    private async void OnExportDataClicked(object sender, EventArgs e)
    {
        try
        {
            var snapshot = await _dbService.GetDataSnapshotAsync();
            var fileName = $"caloriefit-export-{DateTime.Now:yyyyMMdd-HHmmss}.json";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            await Share.Default.RequestAsync(new ShareFileRequest("Export CalorieFit", new ShareFile(filePath)));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export nereușit", $"Datele nu au putut fi exportate: {ex.Message}", "OK");
        }
    }

    private async void OnClearDataClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Ștergere jurnal", "Doriți să ștergeți mesele, înregistrările de apă și notele zilnice? Profilul va fi păstrat.", "Da", "Nu");
        if (!confirm)
        {
            return;
        }

        await _dbService.ClearTrackingDataAsync();
        await RefreshDatabaseStatusAsync();
        await DisplayAlert("Gata", "Jurnalul alimentar, apa și notele zilnice au fost șterse.", "OK");
    }

    private async void OnAboutClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Despre CalorieFit",
            "CalorieFit te ajută să-ți urmărești alimentația și obiceiurile zilnice pentru un stil de viață mai echilibrat.\n\n" +
            "Poți înregistra mesele și valorile lor nutriționale, inclusiv prin căutarea alimentelor în baza USDA, și poți urmări progresul zilnic pentru calorii și macronutrienți. Aplicația include și monitorizarea consumului de apă, statistici și grafice nutriționale, idei de rețete sănătoase, un profil cu calculator BMR și un jurnal în care îți poți nota cum ți-a fost ziua.\n\n" +
            "Datele și preferințele pot fi gestionate din Setări.\nProiect realizat de Echipa 15: Ailioaei Sorina-Elena, Babeș Ana-Maria-Cristina, Baicu Alexandra-Paula, Banaurs Maria-Magdalena.",
            "OK");
    }
}
