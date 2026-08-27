using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base.Attributes;

namespace TrackoApi.Models.Global
{
    [Table("tHttpRequestPool")]
    public class HttpRequestPool
    {
        public HttpRequestPool()
        {
            RequestId = Guid.NewGuid().ToString("D");
        }
        [MaxLength(300),Key]
        public string RequestId { get; set; }

        [MaxLength(300)]
        public string BatchId { get; set; }

        [MaxLength(50)]
        public string Method { get; set; }

        public string Uri { get; set; }

        public string RequestBody { get; set; }

        public string Headers { get; set; }
        public string Result { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        public DateTime? ExecutedTime { get; set; }

        [MaxLength(200)]
        public string Sender { get; set; }
        [Index("IDX_HttpPurpose",IsUnique =false),MaxLength(200)]
        public string Purpose { get; set; }

        public int Timeout { get; set; } = -1;
        public bool Autodecompress { get; set; } = false;
        public bool ResponseTobase64 { get; set; } = false;
        public bool IsProceeded { get; set; }
        public DateTime? ProcessTime { get; set; }
        public string Ref3Text { get; set; }
        public long? Ref3Int { get; set; }
        public int? IsPostedInErp { get; set; }
        public string SuccessString { get; set; }
        public string ProcessedBy { get; set; }
        public IDictionary<string,object> _headers
        {
            get
            {
                try
                {
                    if(string.IsNullOrWhiteSpace(Headers))return new Dictionary<string,object>();
                    return JsonConvert.DeserializeObject<Dictionary<string, object>>(Headers);
                }catch
                {
                    return new Dictionary<string, object>();
                }
            }
        }
        [SqlDefaultValue(DefaultValue ="0")]
        public bool LogRequest { get; set; }
        public string LogData { get; set; }

        [SqlDefaultValue(DefaultValue = "0")]
        public int? NoofAttempts { get; set; } = 0;
    }
}
