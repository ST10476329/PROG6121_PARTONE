using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RaceDayApi.Models
{
    [Table("Events")]
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        public int OrganiserID { get; set; }
        [ForeignKey("OrganiserID")]
        public User Organiser { get; set; } = null!;

        [MaxLength(200)]
        public string EventName { get; set; } = "";

        [MaxLength(250)]
        public string Description { get; set; } = "";

        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }

        [Column("location")]
        [MaxLength(50)]
        public string Location { get; set; } = "";

        [Precision(5, 2)]
        public decimal DistanceKm { get; set; }

        [MaxLength(25)]
        public string EventType { get; set; } = "";
    }
}
