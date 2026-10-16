using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    [Table("payments")]
    [Index(nameof(RegistrationID), IsUnique = true)]
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        public int RegistrationID { get; set; }
        [ForeignKey("RegistrationID")]
        public Registration Registration { get; set; } = null!;

        [Precision(8, 2)]
        public decimal Amount { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime PaymentDate { get; set; }

        [MaxLength(30)]
        public string PaymentMethod { get; set; } = "";

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "";
    }
}
