using System.ComponentModel.DataAnnotations.Schema;
namespace Rozklady.Models;

[Table("day_types")]
public class DayType
{
    [Column("date")]
    public DateOnly Date { get; set; }
    [Column("type")]
    public string Type { get; set; } = null!;
}