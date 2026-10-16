using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    [Table("Registration")]
    [Index(nameof(RaceNumber), IsUnique = true)]   // RaceNumber must always be unique
    public class Registration
    {
        [Key]
        public int RegistrationID { get; set; }

        public int ParticipantID { get; set; }
        [ForeignKey("ParticipantID")]
        public User Participant { get; set; } = null!;

        [Column("categoryID")]
        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; } = null!;

        [Column(TypeName = "datetime")]
        public DateTime RegistrationDate { get; set; }

        [MaxLength(10)]
        public string RaceNumber { get; set; } = "";
    }
}
