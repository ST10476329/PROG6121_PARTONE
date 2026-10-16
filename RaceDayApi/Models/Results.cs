using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    [Table("results")]
    [Index(nameof(RegistrationID), IsUnique = true)]
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        public int RegistrationID { get; set; }
        [ForeignKey("RegistrationID")]
        public Registration Registration { get; set; } = null!;

        public TimeSpan? FinishTime { get; set; }   
        public int? Position { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "";
    }
}
