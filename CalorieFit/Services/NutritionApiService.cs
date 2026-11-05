using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using CalorieFit.Models;

namespace CalorieFit.Services;

public class NutritionApiService
{
    private const string ApiKey = "DEMO_KEY";
    private readonly HttpClient _httpClient = new();

    public async Task<List<ApiFoodItem>> SearchNutritionAsync(string query)
    {
        var url = $"https://api.nal.usda.gov/fdc/v1/foods/search?query={Uri.EscapeDataString(query.Trim())}&pageSize=20&api_key={ApiKey}";
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        var searchResult = await JsonSerializer.DeserializeAsync<FdcSearchResponse>(contentStream);
        if (searchResult?.Foods is null)
        {
            throw new JsonException("USDA FoodData Central returned an unexpected response.");
        }

        var results = new List<ApiFoodItem>();
        foreach (var food in searchResult.Foods)
        {
            var item = ParseFood(food);
            if (item is not null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    public async Task<List<MockRecipe>> GetHealthyRecipesAsync()
    {
        await Task.Delay(300); // Simulare latenta retea
        return new List<MockRecipe>
        {
            new() { Title = "Salată mediteraneană cu quinoa", Category = "Prânz", Calories = 420, CookingTime = "15 min" },
            new() { Title = "Omletă cu spanac și brânză feta", Category = "Mic Dejun", Calories = 310, CookingTime = "10 min" },
            new() { Title = "Somon la cuptor cu sparanghel", Category = "Cină", Calories = 520, CookingTime = "25 min" },
            new() { Title = "Bol cu iaurt grecesc și fructe de pădure", Category = "Gustare", Calories = 230, CookingTime = "5 min" }
        };
    }

    private static ApiFoodItem? ParseFood(FdcFood food)
    {
        if (string.IsNullOrWhiteSpace(food.Description) || food.FoodNutrients is null)
        {
            return null;
        }

        double? calories = null;
        double? protein = null;
        double? carbohydrates = null;
        double? fat = null;

        foreach (var nutrient in food.FoodNutrients)
        {
            if (nutrient.Value is not double value || !double.IsFinite(value))
            {
                continue;
            }

            switch (nutrient.NutrientId)
            {
                case 1008: calories = value; break; // Energy (kcal)
                case 1003: protein = value; break; // Protein (g)
                case 1005: carbohydrates = value; break; // Carbohydrate (g)
                case 1004: fat = value; break; // Total lipid (fat) (g)
            }
        }

        if (calories is null || protein is null || carbohydrates is null || fat is null)
        {
            return null;
        }

        return new ApiFoodItem
        {
            Name = food.Description,
            Calories = calories.Value,
            ProteinG = protein.Value,
            CarbsG = carbohydrates.Value,
            FatG = fat.Value
        };
    }

    private sealed class FdcSearchResponse
    {
        [JsonPropertyName("foods")]
        public List<FdcFood>? Foods { get; set; }
    }

    private sealed class FdcFood
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("foodNutrients")]
        public List<FdcNutrient>? FoodNutrients { get; set; }
    }

    private sealed class FdcNutrient
    {
        [JsonPropertyName("nutrientId")]
        public int NutrientId { get; set; }

        [JsonPropertyName("value")]
        public double? Value { get; set; }
    }
}
