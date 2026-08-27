using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.Global
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
