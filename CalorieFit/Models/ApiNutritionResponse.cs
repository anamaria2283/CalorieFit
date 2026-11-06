using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json.Serialization;

namespace CalorieFit.Models;

public class ApiFoodItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("calories")]
    public double Calories { get; set; }

    [JsonPropertyName("protein_g")]
    public double ProteinG { get; set; }

    [JsonPropertyName("carbohydrates_total_g")]
    public double CarbsG { get; set; }

    [JsonPropertyName("fat_total_g")]
    public double FatG { get; set; }
}

public class MockRecipe
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Calories { get; set; }
    public string CookingTime { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = "healthy_meal.png";
    public string Ingredients { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}