using CalorieFit.Models;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private int? _selectedRating;
    private bool _hasDailyReflection;

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
        var reflection = await _dbService.GetDayReflectionAsync(DateTime.Today);

        double totalCalories = foods.Sum(f => f.Calories);
        double totalProtein = foods.Sum(f => f.ProteinGrams);
        double totalCarbs = foods.Sum(f => f.CarbsGrams);
        double totalFat = foods.Sum(f => f.FatGrams);
        int totalWater = water.Sum(w => w.Milliliters);

        var displayName = string.IsNullOrWhiteSpace(profile.Name) ? "Utilizator" : profile.Name.Trim();
        WelcomeLabel.Text = $"Salut, {displayName}!";
        CalorieRatioLabel.Text = $"{totalCalories:F0} / {profile.DailyCalorieTarget} kcal";
        CalorieProgressBar.Progress = Math.Min(1.0, totalCalories / profile.DailyCalorieTarget);

        UpdateMacroProgress(
            totalProtein,
            profile.ProteinGoalGrams,
            ProteinProgressLabel,
            ProteinProgressBar,
            ProteinProgressMessage,
            "Proteine");
        UpdateMacroProgress(
            totalCarbs,
            profile.CarbsGoalGrams,
            CarbsProgressLabel,
            CarbsProgressBar,
            CarbsProgressMessage,
            "Carbohidrați");
        UpdateMacroProgress(
            totalFat,
            profile.FatGoalGrams,
            FatProgressLabel,
            FatProgressBar,
            FatProgressMessage,
            "Grăsimi");

        MacroProgressSummaryLabel.Text = totalProtein >= profile.ProteinGoalGrams
            && totalCarbs >= profile.CarbsGoalGrams
            && totalFat >= profile.FatGoalGrams
            ? "Felicitări! Ai atins toate obiectivele propuse pentru azi."
            : "Ești pe drumul cel bun. Barele se colorează pe măsură ce te apropii de obiective.";

        WaterRatioLabel.Text = $"{totalWater} / 2500 ml";
        WaterProgressBar.Progress = Math.Min(1.0, (double)totalWater / 2500);

        _hasDailyReflection = reflection is not null;
        DailyReflectionSummaryLabel.Text = reflection is null
            ? "Încă nu ai notat ziua de azi."
            : $"{GetRatingEmoji(reflection.Rating)} {reflection.Rating}/5\n{reflection.Notes}";
        DailyReflectionToggleButton.Text = reflection is null
            ? "Adaugă o notă despre ziua ta"
            : "Editează nota zilei";
    }

    private async void OnToggleDailyReflectionClicked(object sender, EventArgs e)
    {
        DailyReflectionEditor.IsVisible = !DailyReflectionEditor.IsVisible;
        if (DailyReflectionEditor.IsVisible)
        {
            DailyReflectionToggleButton.Text = "Închide";
            await LoadExistingReflectionForEditingAsync();
        }
        else
        {
            DailyReflectionToggleButton.Text = _hasDailyReflection
                ? "Editează nota zilei"
                : "Adaugă o notă despre ziua ta";
        }
    }

    private async Task LoadExistingReflectionForEditingAsync()
    {
        var reflection = await _dbService.GetDayReflectionAsync(DateTime.Today);
        DailyReflectionNotesEditor.Text = reflection?.Notes ?? string.Empty;
        _selectedRating = reflection?.Rating;
        UpdateRatingButtons();
    }

    private void OnRatingSelected(object sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.CommandParameter?.ToString(), out var rating))
        {
            _selectedRating = rating;
            UpdateRatingButtons();
        }
    }

    private void UpdateRatingButtons()
    {
        var buttons = new[] { Rating1Button, Rating2Button, Rating3Button, Rating4Button, Rating5Button };
        foreach (var button in buttons)
        {
            var isSelected = int.TryParse(button.CommandParameter?.ToString(), out var rating) && rating == _selectedRating;
            button.BackgroundColor = isSelected ? Color.FromArgb("#2E7D32") : Color.FromArgb("#E0E0E0");
            button.TextColor = isSelected ? Colors.White : Colors.Black;
        }
    }

    private async void OnSaveDailyReflectionClicked(object sender, EventArgs e)
    {
        if (_selectedRating is not int rating)
        {
            await DisplayAlert("Alege un scor", "Selectează un scor de la 1 la 5 pentru ziua ta.", "OK");
            return;
        }

        await _dbService.SaveDayReflectionAsync(new DayReflection
        {
            DateLogged = DateTime.Today,
            Rating = rating,
            Notes = DailyReflectionNotesEditor.Text?.Trim() ?? string.Empty
        });

        _hasDailyReflection = true;
        DailyReflectionEditor.IsVisible = false;
        DailyReflectionToggleButton.Text = "Editează nota zilei";
        await LoadDashboardData();
    }

    private static string GetRatingEmoji(int rating) => rating switch
    {
        1 => "😞",
        2 => "🙁",
        3 => "😐",
        4 => "🙂",
        5 => "😄",
        _ => "⭐"
    };

    private static void UpdateMacroProgress(
        double consumed,
        double goal,
        Label valueLabel,
        ProgressBar progressBar,
        Label messageLabel,
        string nutrientName)
    {
        if (goal <= 0)
        {
            valueLabel.Text = $"{nutrientName}: {consumed:F0} g / obiectiv nesetat";
            progressBar.Progress = 0;
            progressBar.ProgressColor = Color.FromArgb("#9E9E9E");
            messageLabel.Text = "Setează un obiectiv pentru a vedea progresul.";
            return;
        }

        var ratio = consumed / goal;
        var visibleProgress = Math.Clamp(ratio, 0, 1);
        valueLabel.Text = $"{nutrientName}: {consumed:F1} / {goal:F1} g ({ratio:P0})";
        progressBar.Progress = visibleProgress;
        progressBar.ProgressColor = GetProgressColor(visibleProgress);
        messageLabel.Text = GetProgressMessage(ratio, nutrientName);
    }

    private static string GetProgressMessage(double ratio, string nutrientName)
    {
        if (ratio >= 1)
        {
            return $"Felicitări! Ai atins obiectivul pentru {nutrientName.ToLowerInvariant()}.";
        }

        if (ratio >= 0.75)
        {
            return $"Foarte aproape! Mai ai {100 * (1 - ratio):F0}% până la obiectiv.";
        }

        if (ratio >= 0.4)
        {
            return "Mai ai puțin, continuă în ritmul acesta!";
        }

        return "Ești la începutul zilei. Fiecare alegere contează!";
    }

    private static Color GetProgressColor(double progress)
    {
        var red = (int)Math.Round(229 + (67 - 229) * progress);
        var green = (int)Math.Round(57 + (160 - 57) * progress);
        var blue = (int)Math.Round(53 + (71 - 53) * progress);
        return Color.FromArgb($"#{red:X2}{green:X2}{blue:X2}");
    }
}