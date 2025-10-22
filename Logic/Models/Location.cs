namespace ContinetExpress.TT.Logic.Models
{
    public class  Location
    {
        public Location() 
        { 
        }

        public Location(double lon, double lat) 
        {
            Lon = lon;
            Lat = lat;
        }

        public double Lon { get; set; }
        public double Lat { get; set; }
    }
}
