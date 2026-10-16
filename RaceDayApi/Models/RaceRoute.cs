using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    [Table("Route")]
    [Index(nameof(CategoryID), IsUnique = true)]   // CategoryID must always be unique 
    public class RaceRoute
    {
        [Key]
        public int RouteID { get; set; }

        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; } = null!;

        [MaxLength(50)]
        public string RouteName { get; set; } = "";

        [Precision(5, 2)]
        public decimal DistanceKm { get; set; }
    }
}
