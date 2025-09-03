using System.ComponentModel.DataAnnotations.Schema;
public class RouteStopsDto
{
    public string FeedId { get; set; }
    public string RouteId { get; set; }
    public int DirectionId { get; set; }
    public string VariantId { get; set; }

    // tu trzymasz cały JSON jako string
    public string Stops { get; set; }
}
