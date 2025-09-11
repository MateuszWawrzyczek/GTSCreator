using System.ComponentModel.DataAnnotations.Schema;

namespace Rozklady.Models
{
    [Table("calendar")]
    public class Calendar
    {
        [Column("feed_id")]
        public string FeedId { get; set; } = null!;

        [Column("service_id")]
        public string ServiceId { get; set; } = null!;

        [Column("monday")]
        public int Monday { get; set; }

        [Column("tuesday")]
        public int Tuesday { get; set; }

        [Column("wednesday")]
        public int Wednesday { get; set; }

        [Column("thursday")]
        public int Thursday { get; set; }

        [Column("friday")]
        public int Friday { get; set; }

        [Column("saturday")]
        public int Saturday { get; set; }

        [Column("sunday")]
        public int Sunday { get; set; }

        [Column("start_date")]
        public DateOnly StartDate { get; set; }

        [Column("end_date")]
        public DateOnly EndDate { get; set; }

    }
}
