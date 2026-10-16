using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RaceDayApi.Models
{
    [Table("Category")]
    public class Category
    {
        [Key]
        [Column("categoryID")]
        public int CategoryID { get; set; }

        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public Event Event { get; set; } = null!;

        [Column("categoryName")]
        [MaxLength(50)]
        public string CategoryName { get; set; } = "";

        [Precision(5, 2)]
        public decimal DistanceKm { get; set; }

        [Precision(8, 2)]
        public decimal EntryFee { get; set; }

        public int? MinAge { get; set; }
    }
}
