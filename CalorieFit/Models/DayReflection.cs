using SQLite;

namespace CalorieFit.Models;

[Table("DayReflections")]
public class DayReflection
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime DateLogged { get; set; } = DateTime.Today;
    public int Rating { get; set; }
    public string Notes { get; set; } = string.Empty;
}
