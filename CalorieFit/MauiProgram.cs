using Microsoft.Extensions.Logging;
using CalorieFit.Services;
using CalorieFit.Views;

namespace CalorieFit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Inregistrare servicii
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<NutritionApiService>();

        // Inregistrare pagini
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<FoodLogPage>();
        builder.Services.AddTransient<AddMealPage>();
        builder.Services.AddTransient<WaterTrackerPage>();
        builder.Services.AddTransient<FoodSearchApiPage>();
        builder.Services.AddTransient<AnalyticsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<MacroGoalsPage>();
        builder.Services.AddTransient<RecipeIdeasPage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}