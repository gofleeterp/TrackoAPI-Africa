using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS.GPS
{
    [Table("mGpsEndPoint")]
    public class GpsEndPoint : AuditableEntity,IValidatableObject
    {
        [Column("Method"),MaxLength(10)]
        public string Method { get; set; }

        [Column("Url"),MaxLength(2000)]
        public string Url { get; set; }

        [Column("Authorization")]
        public string Authorization { get; set; }

        [Column("AcceptEncoding"),MaxLength(50)]
        public string AcceptEncoding { get; set; }

        [Column("SuccessCode"), MaxLength(50)]
        public string SuccessCode { get; set; }

        [Column("ContentType"), MaxLength(50)]
        public string ContentType { get; set; }

        [Column("ContentEncoding"), MaxLength(50)]
        public string ContentEncoding { get; set; }
        [JsonIgnore]
        public string _Headers { get; set; }
        public IDictionary<string,object> Headers
        {
            get
            {
                try
                {
                    return string.IsNullOrWhiteSpace(_Headers) ? null : JsonConvert.DeserializeObject<IDictionary<string, object>>(_Headers);
                }
                catch
                {
                    return null;
                }
                
            }
            set
            {
                if (value != null)
                {
                    _Headers = JsonConvert.SerializeObject(value);
                }
            }
        }
        public long? VendorId { get; set; }
        [ForeignKey("VendorId")]
        public virtual Ledger fk_Vendor { get; set; }

        
        public long ServiceTypeId { get; set; }
        [ForeignKey("ServiceTypeId")]
        public virtual ConstantValue fk_ServiceType { get; set; }

        [Column("ParameterMapping")]
        public string ParameterMapping { get; set; }

        [Column("ParameterTemplate")]
        public string ParameterTemplate { get; set; }

        public bool IsParameterInArray { get; set; }

        [Column("ResultMapping")]
        public string ResultMapping { get; set; }
        [Column("ResultJsonPath")]
         public string ResultJsonPath { get; set; }
        [Column("DateFormat")]
        public string DateFormat { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Headers != null&&Headers.Count>0)
            {
                _Headers = JsonConvert.SerializeObject(Headers);
            }
            if (string.IsNullOrWhiteSpace(Url))
            {
                yield return new ValidationResult("EndPoint Url is Required", new[] { Url });
            }
        }
    }    
}
