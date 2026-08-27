using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tenant.Models
{
    public class URLShortener
    {
        public URLShortener()
        {

        }
        public URLShortener(string longurl)
        {
            LongUrl = longurl;
        }
        public long LinkId { get; set; }
        public string LongUrl { get; set; }
        public string ShortUrl { get; set; }
        public int Hits { get; set; } = 0;
        public long CreatedTicks { get; set; } = DateTime.Now.Ticks;
        public long UserId { get; set; }
        public long ExpiryTicks { get; set; } = DateTime.Now.AddDays(2).Ticks;
        public string SharedWith { get; set; }
        public string Password { get; set; }
        public bool IsProtected { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        /// <summary>
        /// Check if provided URL is valid HTTP url
        /// </summary>
        /// <param name="url">URL (Uniform resource locator)</param>
        /// <returns>true or false depends on the URL contains HTTP</returns>
        public bool HasHTTPProtocol(string url)
        {
            url = url.ToLower();
            if (url.Length > 5)
            {
                if (url.ToLower().StartsWith("http://") || url.ToLower().StartsWith("https://"))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        /// <summary>
        /// Check whether provided URL exists by doing request to it and waiting for response.
        /// </summary>
        /// <returns>true or false depends on the availability of the provided URL</returns>
        public bool CheckLongUrlExists()
        {
            int linkLength = LongUrl.Length;
            if (!HasHTTPProtocol(LongUrl))
                LongUrl = "http://" + LongUrl;

            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(LongUrl) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                if (Headers!=null&& Headers.Any())
                {
                    foreach (var head in Headers)
                    {
                        request.Headers.Add(head.Key, head.Value?.ToString());
                    }
                }
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }
        public bool IsEmailMatched(string email)
        {
            if (string.IsNullOrWhiteSpace(SharedWith)||!IsProtected)
            {
                return true;
            }
            var emails = SharedWith.Split(',');
            return emails.Contains(email);
        }
    }
}
