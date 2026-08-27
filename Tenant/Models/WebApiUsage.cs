using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Tenant.Models
{
    [DataContract]
    public class WebApiUsage
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string TenantKey { get; set; }
        [DataMember]
        public string ApplicationKey { get; set; }
        [DataMember]
        public long UserKey { get; set; }
        [DataMember]
        public DateTime RequestTimestamp { get; set; }
        [DataMember]
        public DateTime ResponseTimestamp { get; set; }
        [DataMember]
        public string RequestContent { get; set; }
        [DataMember]
        public string ResponseContent { get; set; }
        [DataMember]
        public string RequestHeaders { get; set; }
        [DataMember]
        public string ResponseHeaders { get; set; }

        public string extractHeaders(HttpHeaders h)
        {
            List<KeyValuePair<string, string[]>> list = new List<KeyValuePair<string, string[]>>();
            foreach (var pair in h) list.Add(new KeyValuePair<string, string[]>(pair.Key, pair.Value.ToArray()));
            return extractHeaders(list);
            //Dictionary<string, string> dict = new Dictionary<string, string>();
            //foreach (var i in h.ToList())
            //{
            //    if(i.Key=="Authorization")continue;
            //    if (i.Value != null)
            //    {
            //        string header = string.Empty;
            //        foreach (var j in i.Value)
            //        {
            //            header += j + " ";
            //        }
            //        dict.Add(i.Key, header);
            //    }
            //}
            //return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }
        [DataMember]
        public string Uri { get; set; }
        [DataMember]
        public string RequestMethod { get; set; }
        [DataMember]
        public string IP { get; set; }
        [DataMember]
        public int ResponseStatusCode { get; set; }

        public string extractHeaders(List<KeyValuePair<string, string[]>> toList)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var i in toList)
            {
                if (i.Key == "Authorization") continue;
                if (i.Value != null)
                {
                    string header = string.Empty;
                    foreach (var j in i.Value)
                    {
                        header += j + " ";
                    }
                    dict.Add(i.Key, header);
                }
            }
            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }
    }

    //[DataContract]
    //public class WebApiUsageRequest : WebApiUsage
    //{
        

        
    //}

    //[DataContract]
    //public class WebApiUsageResponse : WebApiUsage
    //{
        
    //}
}
