using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IWLT.TrackoAPI.Subscription.ViewModels
{
    public class IPToGeoLocation
    {
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string IpAddress { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public string RegionName { get; set; }
        public string StatusCode { get; set; }
        public int ZipCode { get; set; }
    }
}
