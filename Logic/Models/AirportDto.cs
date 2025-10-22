#nullable disable
namespace ContinetExpress.TT.Logic.Models
{
    public class AirportDto
    {
        public string Iata { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string City_iata { get; set; }
        public string Icao { get; set; }
        public string Country { get; set; }
        public string Country_iata { get; set; }
        public Location Location { get; set; }

    }
}
