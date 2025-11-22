namespace Nop.Plugin.Baramjk.LocationDetector.Models
{
    public class Ip2LocationResponseModel
    {
        public string ip { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string region_name { get; set; }
        public string city_name { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string zip_code { get; set; }
        public string time_zone { get; set; }
        public string asn { get; set; }
        public string @as { get; set; }
        public bool is_proxy { get; set; }
    }

}