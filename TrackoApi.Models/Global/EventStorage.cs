using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace TrackoApi.Models.Global
{
    public class EventStorage
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None),MaxLength(100)]
        public string EventLogId { get; set; }
        [MaxLength(100)]
        public string JobLogId { get; set; }
        public int EventCode { get; set; }
        [MaxLength(200)]
        public string EventName { get; set; }
        [MaxLength(100)]
        public string SenderId { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public DateTimeOffset EventReceivedTime { get; set; }
        public IDictionary<string,object> EventData
        {
            get
            {
                return string.IsNullOrWhiteSpace(_Properties)|| EventDataIsListObject ? null : JsonConvert.DeserializeObject<IDictionary<string,object>>(_Properties);
            }
            set
            {
                if (value != null&& !EventDataIsListObject)
                {
                    _Properties = JsonConvert.SerializeObject(value);
                }
            }
        }
        public List<IDictionary<string, object>> EventDataArray
        {
            get
            {
                return string.IsNullOrWhiteSpace(_Properties) || !EventDataIsListObject ? null : JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(_Properties);
            }
            set
            {
                if (value != null && EventDataIsListObject)
                {
                    _Properties = JsonConvert.SerializeObject(value);
                }
            }
        }
        [JsonIgnore]
        public string _Properties { get; set; }

        public bool IsProcessed { get; set; } = false;
        public string Error { get; set; }
        public DateTimeOffset? ProcessedTime { get; set; }
        [MaxLength(200)]
        public string SenderName { get; set; }
        public bool EventDataIsListObject { get; set; } = false;
    }
}