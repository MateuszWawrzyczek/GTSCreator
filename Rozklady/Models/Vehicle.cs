using System.ComponentModel.DataAnnotations.Schema;
namespace Rozklady.Models;

[Table("vehicles")]
public class Vehicle
{
    [Column("fleet_number")]
    public string FleetNumber { get; set; } = null!;

    [Column("feed_id")]
    public string FeedId { get; set; } = null!;

    [Column("operator")]
    public string? Operator { get; set; }

    [Column("production_year")]
    public int ProductionYear { get; set; }

    [Column("model")]
    public string? Model { get; set; }

    [Column("air_conditioning")]
    public bool AirConditioning { get; set; }
}
