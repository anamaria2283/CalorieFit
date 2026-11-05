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

    public async Task<(int FoodEntries, int WaterLogs)> GetDataCountsAsync()
    {
        await InitAsync();
        return (await _database!.Table<FoodEntry>().CountAsync(), await _database.Table<WaterLog>().CountAsync());
    }

    public async Task<DatabaseSnapshot> GetDataSnapshotAsync()
    {
        await InitAsync();
        return new DatabaseSnapshot
        {
            ExportedAt = DateTime.Now,
            Profile = await GetProfileAsync(),
            FoodEntries = await _database!.Table<FoodEntry>().ToListAsync(),
            WaterLogs = await _database.Table<WaterLog>().ToListAsync()
        };
    }

    public async Task ClearTrackingDataAsync()
    {
        await InitAsync();
        await _database!.DeleteAllAsync<FoodEntry>();
        await _database.DeleteAllAsync<WaterLog>();
    }
}

public class DatabaseSnapshot
{
    public DateTime ExportedAt { get; set; }
    public UserProfile Profile { get; set; } = new();
    public List<FoodEntry> FoodEntries { get; set; } = new();
    public List<WaterLog> WaterLogs { get; set; } = new();
}
