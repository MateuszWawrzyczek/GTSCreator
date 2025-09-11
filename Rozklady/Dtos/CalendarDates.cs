using System.ComponentModel.DataAnnotations.Schema;

namespace Rozklady.Models
{
    [Table("calendar_dates")]
    public class CalendarDates
    {
        [Column("feed_id")]
        public string FeedId { get; set; } = null!;

        [Column("service_id")]
        public string ServiceId { get; set; } = null!;

        [Column("date")]
        public DateOnly Date { get; set; }

        [Column("exception_type")]
        public int ExceptionType { get; set; }
    }
}
