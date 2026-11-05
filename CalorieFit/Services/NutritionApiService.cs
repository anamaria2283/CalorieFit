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
            new() { Title = "Salată mediteraneană cu quinoa", Category = "Prânz", Calories = 420, CookingTime = "15 min", Ingredients = "Quinoa, roșii, castravete, măsline, feta, ulei de măsline", Instructions = "Fierbeți quinoa, lăsați-o să se răcească, apoi amestecați toate ingredientele și asezonați." },
            new() { Title = "Omletă cu spanac și brânză feta", Category = "Mic Dejun", Calories = 310, CookingTime = "10 min", Ingredients = "2 ouă, spanac proaspăt, brânză feta, piper", Instructions = "Căliți spanacul, adăugați ouăle bătute și feta, apoi gătiți până când omleta este închegată." },
            new() { Title = "Terci de ovăz cu banană și nuci", Category = "Mic Dejun", Calories = 350, CookingTime = "8 min", Ingredients = "Fulgi de ovăz, lapte, banană, nuci, scorțișoară", Instructions = "Fierbeți ovăzul în lapte, apoi adăugați banana feliată, nucile și scorțișoara." },
            new() { Title = "Toast cu avocado și ou", Category = "Mic Dejun", Calories = 380, CookingTime = "12 min", Ingredients = "Pâine integrală, avocado, ou, lămâie, roșii cherry", Instructions = "Prăjiți pâinea, zdrobiți avocado cu lămâie și serviți cu oul fiert sau poșat." },
            new() { Title = "Somon la cuptor cu sparanghel", Category = "Cină", Calories = 520, CookingTime = "25 min", Ingredients = "File de somon, sparanghel, lămâie, usturoi, ulei de măsline", Instructions = "Asezonați somonul și sparanghelul, apoi coaceți la 200°C timp de aproximativ 15-20 de minute." },
            new() { Title = "Bol cu iaurt grecesc și fructe de pădure", Category = "Gustare", Calories = 230, CookingTime = "5 min", Ingredients = "Iaurt grecesc, fructe de pădure, fulgi de ovăz, miere", Instructions = "Puneți ingredientele într-un bol, în straturi, și serviți imediat." },
            new() { Title = "Piept de pui cu legume la tigaie", Category = "Cină", Calories = 460, CookingTime = "30 min", Ingredients = "Piept de pui, broccoli, ardei, dovlecel, sos de soia", Instructions = "Tăiați puiul și legumele, gătiți-le pe rând într-o tigaie încinsă și combinați-le la final." },
            new() { Title = "Paste integrale cu sos de roșii", Category = "Prânz", Calories = 440, CookingTime = "25 min", Ingredients = "Paste integrale, roșii, ceapă, usturoi, busuioc, parmezan", Instructions = "Fierbeți pastele, pregătiți sosul de roșii cu usturoi și amestecați. Presărați busuioc și parmezan." },
            new() { Title = "Supă cremă de linte", Category = "Prânz", Calories = 290, CookingTime = "35 min", Ingredients = "Linte roșie, morcov, ceapă, țelină, roșii, condimente", Instructions = "Fierbeți toate ingredientele până se înmoaie, apoi pasați supa și ajustați condimentele." },
            new() { Title = "Hummus cu legume proaspete", Category = "Gustare", Calories = 210, CookingTime = "10 min", Ingredients = "Năut, tahini, lămâie, usturoi, morcov, castravete", Instructions = "Pasați năutul cu tahini, lămâie și usturoi. Serviți cu legumele tăiate bastonașe." },
            new() { Title = "Salată cu ton și fasole", Category = "Prânz", Calories = 390, CookingTime = "10 min", Ingredients = "Ton, fasole roșie, porumb, ceapă roșie, salată verde", Instructions = "Scurgeți ingredientele, amestecați-le într-un bol și asezonați cu lămâie și ulei de măsline." },
            new() { Title = "Budincă de chia cu mango", Category = "Gustare", Calories = 250, CookingTime = "10 min + răcire", Ingredients = "Semințe de chia, lapte de cocos, mango, miere", Instructions = "Amestecați chia cu laptele și mierea, lăsați la frigider câteva ore și serviți cu mango." }
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
