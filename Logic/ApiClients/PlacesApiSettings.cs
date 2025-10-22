#nullable disable
namespace ContinetExpress.TT.Logic.ApiClients
{
    public class PlacesApiSettings
    {
        public const string SectionName = "PlacesApiSettings";
        public Uri BaseUri { get; set; }
        public TimeSpan Timeout { get; set; }
    }
}
