using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace CalorieFit.Models;

[Table("WaterLogs")]
public class WaterLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int Milliliters { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
