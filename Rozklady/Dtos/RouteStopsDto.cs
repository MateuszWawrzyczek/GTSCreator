using System.ComponentModel.DataAnnotations.Schema;
public class RouteStopsDto
{
    public required string FeedId { get; set; }
    public required string RouteId { get; set; }
    public required string RouteShortName { get; set; }
    public int DirectionId { get; set; }
    public required string VariantId { get; set; }
    public required string Stops { get; set; }
}
