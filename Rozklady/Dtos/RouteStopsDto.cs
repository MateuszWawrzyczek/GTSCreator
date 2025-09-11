using System.ComponentModel.DataAnnotations.Schema;
public class RouteStopsDto
{
    public string FeedId { get; set; }
    public string RouteId { get; set; }
    public int DirectionId { get; set; }
    public string VariantId { get; set; }

    public string Stops { get; set; }
}
