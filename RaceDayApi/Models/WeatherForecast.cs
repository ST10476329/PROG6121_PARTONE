using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayApi.Models
{
    [Table("WeatherForecast")]
    public class WeatherForecast
    {
        [Key]
        public int WeatherID { get; set; }

        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public Event Event { get; set; } = null!;

        [Column(TypeName = "date")]
        public DateTime ForecastDate { get; set; }

        [Precision(4, 1)]
        public decimal? Temperature { get; set; }

        [Precision(4, 1)]
        public decimal? WindSpeedKph { get; set; }
    }
}
