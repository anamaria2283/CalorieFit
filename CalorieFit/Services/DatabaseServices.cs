using System;
using System.Collections.Generic;
using System.Text;

using SQLite;
using CalorieFit.Models;

namespace CalorieFit.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database != null) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "CalorieFit_v1.db3");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<UserProfile>();
        await _database.CreateTableAsync<FoodEntry>();
        await _database.CreateTableAsync<WaterLog>();
        await _database.CreateTableAsync<DayReflection>();
        await EnsureUserProfileGoalColumnsAsync();

        // Initializare profil implicit daca baza e goala
        var profile = await _database.Table<UserProfile>().FirstOrDefaultAsync();
        if (profile == null)
        {
            await _database.InsertAsync(new UserProfile
            {
                Name = "Utilizator CalorieFit",
                WeightKg = 65,
                HeightCm = 170,
                Age = 22,
                DailyCalorieTarget = 2100
            });
        }
    }

    private async Task EnsureUserProfileGoalColumnsAsync()
    {
        var columns = await _database!.GetTableInfoAsync("UserProfile");
        var existingColumns = columns.Select(column => column.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!existingColumns.Contains(nameof(UserProfile.ProteinGoalGrams)))
        {
            await _database.ExecuteAsync("ALTER TABLE UserProfile ADD COLUMN ProteinGoalGrams REAL NOT NULL DEFAULT 120");
        }

        if (!existingColumns.Contains(nameof(UserProfile.CarbsGoalGrams)))
        {
            await _database.ExecuteAsync("ALTER TABLE UserProfile ADD COLUMN CarbsGoalGrams REAL NOT NULL DEFAULT 250");
        }

        if (!existingColumns.Contains(nameof(UserProfile.FatGoalGrams)))
        {
            await _database.ExecuteAsync("ALTER TABLE UserProfile ADD COLUMN FatGoalGrams REAL NOT NULL DEFAULT 70");
        }
    }

    public async Task<UserProfile> GetProfileAsync()
    {
        await InitAsync();
        return await _database!.Table<UserProfile>().FirstOrDefaultAsync() ?? new UserProfile();
    }

    public async Task SaveProfileAsync(UserProfile profile)
    {
        await InitAsync();
        if (profile.Id == 0)
            await _database!.InsertAsync(profile);
        else
            await _database!.UpdateAsync(profile);
    }

    public async Task<List<FoodEntry>> GetFoodEntriesByDateAsync(DateTime date)
    {
        await InitAsync();
        var start = date.Date;
        var end = date.Date.AddDays(1);
        return await _database!.Table<FoodEntry>()
                               .Where(f => f.DateLogged >= start && f.DateLogged < end)
                               .ToListAsync();
    }

    public async Task<List<FoodEntry>> GetFoodEntriesBetweenDatesAsync(DateTime startDate, DateTime endDate)
    {
        await InitAsync();
        var start = startDate.Date;
        var end = endDate.Date.AddDays(1);
        return await _database!.Table<FoodEntry>()
                               .Where(f => f.DateLogged >= start && f.DateLogged < end)
                               .ToListAsync();
    }

    public async Task AddFoodEntryAsync(FoodEntry entry)
    {
        await InitAsync();
        await _database!.InsertAsync(entry);
    }

    public async Task DeleteFoodEntryAsync(FoodEntry entry)
    {
        await InitAsync();
        await _database!.DeleteAsync(entry);
    }

    public async Task<List<WaterLog>> GetTodayWaterLogsAsync()
    {
        await InitAsync();
        var today = DateTime.Today;
        return await _database!.Table<WaterLog>()
                               .Where(w => w.Timestamp >= today)
                               .ToListAsync();
    }

    public async Task AddWaterLogAsync(int ml)
    {
        await InitAsync();
        await _database!.InsertAsync(new WaterLog { Milliliters = ml, Timestamp = DateTime.Now });
    }

    public async Task<DayReflection?> GetDayReflectionAsync(DateTime date)
    {
        await InitAsync();
        var start = date.Date;
        var end = start.AddDays(1);
        return await _database!.Table<DayReflection>()
            .Where(reflection => reflection.DateLogged >= start && reflection.DateLogged < end)
            .FirstOrDefaultAsync();
    }

    public async Task SaveDayReflectionAsync(DayReflection reflection)
    {
        await InitAsync();
        var existing = await GetDayReflectionAsync(reflection.DateLogged);
        if (existing is null)
        {
            await _database!.InsertAsync(reflection);
            return;
        }

        existing.Rating = reflection.Rating;
        existing.Notes = reflection.Notes;
        await _database!.UpdateAsync(existing);
    }

    public async Task ResetTodayWaterAsync()
    {
        await InitAsync();
        var today = DateTime.Today;
        var items = await _database!.Table<WaterLog>().Where(w => w.Timestamp >= today).ToListAsync();
        foreach (var item in items)
        {
            await _database.DeleteAsync(item);
        }
    }

    public async Task<(int FoodEntries, int WaterLogs, int DayReflections)> GetDataCountsAsync()
    {
        await InitAsync();
        return (
            await _database!.Table<FoodEntry>().CountAsync(),
            await _database.Table<WaterLog>().CountAsync(),
            await _database.Table<DayReflection>().CountAsync());
    }

    public async Task<DatabaseSnapshot> GetDataSnapshotAsync()
    {
        await InitAsync();
        return new DatabaseSnapshot
        {
            ExportedAt = DateTime.Now,
            Profile = await GetProfileAsync(),
            FoodEntries = await _database!.Table<FoodEntry>().ToListAsync(),
            WaterLogs = await _database.Table<WaterLog>().ToListAsync(),
            DayReflections = await _database.Table<DayReflection>().ToListAsync()
        };
    }

    public async Task ClearTrackingDataAsync()
    {
        await InitAsync();
        await _database!.DeleteAllAsync<FoodEntry>();
        await _database.DeleteAllAsync<WaterLog>();
        await _database.DeleteAllAsync<DayReflection>();
    }
}

public class DatabaseSnapshot
{
    public DateTime ExportedAt { get; set; }
    public UserProfile Profile { get; set; } = new();
    public List<FoodEntry> FoodEntries { get; set; } = new();
    public List<WaterLog> WaterLogs { get; set; } = new();
    public List<DayReflection> DayReflections { get; set; } = new();
}
