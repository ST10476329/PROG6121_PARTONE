using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RaceDayApi.Models
{
    [Table("users")]
    [Index(nameof(Email), IsUnique = true)]   // this will make sure emails are unique
    public class User
    {
        [Key]
        [Column("userID")]
        public int UserID { get; set; }

        [MaxLength(40)]
        public string FullName { get; set; } = "";

        [MaxLength(90)]
        public string Email { get; set; } = "";

        [Column("Password")]   // the column is called Password but we only store the hash in it
        [MaxLength(90)]
        public string PasswordHash { get; set; } = "";

        [MaxLength(25)]
        public string Role { get; set; } = "";
    }
}
