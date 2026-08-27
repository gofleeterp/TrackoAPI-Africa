using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using RestSharp;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.FMS.GPS;
using TrackoAPI.GSTN.Models.EWB;

namespace TrackoApi.Service.Global
{
    
    public interface IEWayBillBackgroundService
    {

    }
    public class EWayBillBackgroundService
    {
        private readonly IGlobalStore _gs;
        private readonly ICacheClient _cache;
        const string EWBCachePrefix = "ewb_";

        public EWayBillBackgroundService(IGlobalStore globalStore, ICacheClient cache)
        {
            _gs = globalStore;
            _cache = cache;
        }
        public void RefreshEWBAuthToken(PerformContext context =null)
        {

        }
        private GSPAuthToken GenerateAuthToken(GpsEndPoint endpoint, PerformContext context)
        {
            var response = SendWebRequest("", endpoint, context);
            if (response.IsSuccessful)
            {
                var result = response.Content;
                if(!string.IsNullOrWhiteSpace(result))
                {
                    return JsonConvert.DeserializeObject<GSPAuthToken>(result);

                    //_cache.Add<GSPAuthToken>($"{EWBCachePrefix}_AuthToken", accessToken);
                }
            }
            return null;
        }
        private IRestResponse SendWebRequest(string requestbody, GpsEndPoint endpoint, PerformContext context = null)
        {

            try
            {
                context?.WriteLine(ConsoleTextColor.Cyan, $"Processing Web Request under SendGpsRequest method URL:{endpoint.Url} of type {endpoint.Method.ToUpper()} with Authorization Header as { endpoint.Authorization}");
                var client = new RestSharp.RestClient(endpoint.Url);
                var request = new RestSharp.RestRequest((RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), endpoint.Method.ToUpper()));
                if (!string.IsNullOrWhiteSpace(endpoint.Authorization))
                {
                    request.AddHeader("Authorization", endpoint.Authorization);
                }
                if (endpoint.Method == "GET")
                {
                    if (!string.IsNullOrWhiteSpace(requestbody))
                    {
                        request.Resource = requestbody.Trim().Replace('\n', ' ');
                    }
                    var getresponse = client.ExecuteAsGet(request, endpoint.Method.ToUpper());
                    context?.WriteLine(getresponse.IsSuccessful ? ConsoleTextColor.Cyan : ConsoleTextColor.Red, $"Web Request Processed with StatusCode {getresponse.StatusCode} and was {(getresponse.IsSuccessful ? "sucessfull" : $"unsuccessful with response {(!string.IsNullOrWhiteSpace(getresponse.Content) ? getresponse.Content : getresponse.ErrorMessage ?? getresponse.ErrorException?.GetBaseException().Message ?? "NA")}")}");
                    return getresponse;
                }

                if (!string.IsNullOrWhiteSpace(requestbody))
                {
                    request.AddParameter("application/json; charset=utf-8", requestbody, ParameterType.RequestBody);
                }
                var postresponse = client.Execute(request);
                context?.WriteLine(postresponse.IsSuccessful ? ConsoleTextColor.Cyan : ConsoleTextColor.Red, $"Web Request Processed with StatusCode {postresponse.StatusCode} and was {(postresponse.IsSuccessful ? "sucessfull" : $"unsucessfull with response {(!string.IsNullOrWhiteSpace(postresponse.Content) ? postresponse.Content : postresponse.ErrorMessage ?? postresponse.ErrorException?.GetBaseException().Message ?? "NA")}")}");
                return postresponse;
            }
            catch (Exception ex)
            {
                context?.WriteLine(ConsoleTextColor.Red, ex);
                try
                {
                    if (!Helper.HostedOnPremise)
                        using (var db = new TenantDbContext())
                        {
                            db.ApiLog.Add(new WebApiUsage()
                            {
                                IP = endpoint.Url,
                                ResponseContent = ex.GetBaseException().Message + "\n" + ex.StackTrace,
                                RequestMethod = endpoint.Method,
                                ResponseTimestamp = DateTime.Now,
                                RequestTimestamp = DateTime.Now,
                                RequestContent = JsonConvert.SerializeObject(new
                                {
                                    RequestBody = requestbody,
                                    EndPoint = endpoint
                                })
                            });
                            db.SaveChanges();
                        }
                }
                catch (Exception)
                {
                    //Ignore
                }
                throw;
            }
        }
    }
    
}
