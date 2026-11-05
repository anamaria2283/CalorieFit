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
        int totalWater = water.Sum(w => w.Milliliters);

        WelcomeLabel.Text = $"Salut, {profile.Name}!";
        CalorieRatioLabel.Text = $"{totalCalories:F0} / {profile.DailyCalorieTarget} kcal";
        CalorieProgressBar.Progress = Math.Min(1.0, totalCalories / profile.DailyCalorieTarget);

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
}