using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Models.Global
{
    public class vwPostalAddress
    {
        public long Id { get; set; }
        public string UnitNo { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string Landmark { get; set; }
        public string EmailAddress { get; set; }
        public string AltEmailAddress { get; set; }
        public string ContactNumber { get; set; }
        public string ContactPerson { get; set; }
        public string AltContactNumber { get; set; }
        public string AltContactPerson { get; set; }
        public long? CityId { get; set; }
        public string CityName { get; set; }
        public long? StateId { get; set; }
        public string StateName { get; set; }
        public long? CountryId { get; set; }
        public string CountryName{ get; set; }
    }
}
