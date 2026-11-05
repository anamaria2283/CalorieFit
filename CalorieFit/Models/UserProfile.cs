using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace CalorieFit.Models;

[Table("UserProfile")]
public class UserProfile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public double HeightCm { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; } = "Feminin";
    public string ActivityLevel { get; set; } = "Moderat";
    public int DailyCalorieTarget { get; set; } = 2000;
}
