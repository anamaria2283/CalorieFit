using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace CalorieFit.Models;

[Table("FoodEntries")]
public class FoodEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public string MealType { get; set; } = "Mic Dejun"; // Mic Dejun, Pranz, Cina, Gustare
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }
    public DateTime DateLogged { get; set; } = DateTime.Today;
}
