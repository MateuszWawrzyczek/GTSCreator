using System.ComponentModel.DataAnnotations.Schema;
namespace Rozklady.Models;

[Table("service_types")]
public class ServiceType
{
    [Column("service_id")]
    public string ServiceId { get; set; } = null!;

    [Column("day_type")]
    public string DayType { get; set; } = null!;

}