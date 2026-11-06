using System;
using System.Globalization;
using CalorieFit.Models;
using CalorieFit.Services;

namespace CalorieFit.Views;

public partial class AnalyticsPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private readonly MacroPieChartDrawable _macroPieChartDrawable = new();
    private readonly ProteinTrendDrawable _proteinTrendDrawable = new();

    public AnalyticsPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
        AnalysisDatePicker.Date = DateTime.Today;
        MacroPieChart.Drawable = _macroPieChartDrawable;
        ProteinTrendChart.Drawable = _proteinTrendDrawable;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await UpdateChart();
    }

    private async Task UpdateChart()
    {
        var selectedDate = (AnalysisDatePicker.Date ?? DateTime.Today).Date;
        var entries = await _dbService.GetFoodEntriesByDateAsync(selectedDate);
        double totalProtein = entries.Sum(e => e.ProteinGrams);
        double totalCarbs = entries.Sum(e => e.CarbsGrams);
        double totalFat = entries.Sum(e => e.FatGrams);
        double totalCalories = entries.Sum(e => e.Calories);

        SelectedDateLabel.Text = $"Totaluri pentru {selectedDate.ToString("dddd, dd MMMM yyyy", new CultureInfo("ro-RO"))}";
        CaloriesTotalLabel.Text = $"{totalCalories:F0} kcal";
        ProteinTotalLabel.Text = $"Proteine: {totalProtein:F1} g";
        CarbsTotalLabel.Text = $"Carbohidrați: {totalCarbs:F1} g";
        FatTotalLabel.Text = $"Grăsimi: {totalFat:F1} g";
        NoDataLabel.Text = entries.Count == 0
            ? "Nu există alimente înregistrate pentru această zi."
            : $"{entries.Count} înregistrări analizate.";

        _macroPieChartDrawable.ProteinGrams = totalProtein;
        _macroPieChartDrawable.CarbsGrams = totalCarbs;
        _macroPieChartDrawable.FatGrams = totalFat;
        MacroPieChart.Invalidate();

        var startDate = selectedDate.AddDays(-6);
        var history = await _dbService.GetFoodEntriesBetweenDatesAsync(startDate, selectedDate);
        _proteinTrendDrawable.Values = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = startDate.AddDays(offset);
                return new ProteinDay(date, history
                    .Where(entry => entry.DateLogged.Date == date.Date)
                    .Sum(entry => entry.ProteinGrams));
            })
            .ToList();
        ProteinTrendChart.Invalidate();
    }

    private async void OnAnalysisDateSelected(object sender, DateChangedEventArgs e) => await UpdateChart();

}