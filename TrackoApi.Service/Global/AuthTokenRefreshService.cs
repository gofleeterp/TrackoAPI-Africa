using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.GPS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.CronJobs;
using TrackoAPI.Infrastructure.Services;
using TrackoAPI.Reporting.Models;
using TrackoAPI.Reports.ViewModels;
using TrackoAPI.Reports.ViewModels.FMS;
using TrackoAPI.Reports.ViewModels.Global.Integration;
using TrackoAPI.ViewModels.Integration;
using Unity;
using SqlParameter = System.Data.SqlClient.SqlParameter;

namespace TrackoApi.Service.Global
{
    public class AuthTokenRefreshService: IAuthTokenRefreshService
    {
        public void RefreshTokenAllClients()
        {

        }
        public void GenerateNewToken(GpsEndPoint endpoint)
        {

        }
        public void RefreshExistingToken(GpsEndPoint endpoint)
        {
            var client = new RestClient(endpoint.Url);
            client.Timeout = -1;
            var request = new RestRequest((RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), endpoint.Method.ToUpper()));
            request.AddHeader("Content-Type", endpoint.ContentType);
            request.AddParameter("application/x-www-form-urlencoded", "client_id=Q3BgkRkXFF01gtZQrTJ5UbfCn2eWmvpyNEx5oHvDKg&client_secret=ExSN4CBEVco7d0dzsGIlWN8U4Afr7NyoExYP0Hc6U&applicationId=859a5ade-896b-46a7-b355-5ea0a4f711da&refresh_token=c3f9d69d9b094d71bf8f71588211ea69&grant_type=refresh_token&deviceId=BFEBFBFF000306C3&version=5.0.0.3&osname=Microsoft+Windows+10+Pro", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
        public string TestCall(GpsEndPoint endpoint,string requestbody)
        {
            var client = new RestClient(endpoint.Url);/*"https://fastaglogin.icicibank.com/ISRCUSTAPI/Customer/GetTransactionDetails"*/
            var request = new RestRequest((RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), endpoint.Method.ToUpper()));/*RestSharp.Method.POST*/

            if (!string.IsNullOrWhiteSpace(endpoint.AcceptEncoding))
            {
                request.AddHeader("Accept-Encoding", endpoint.AcceptEncoding);
            }
            if (!string.IsNullOrWhiteSpace(endpoint.Authorization))
            {
                request.AddHeader("Authorization", endpoint.Authorization);
            }
            if (!string.IsNullOrWhiteSpace(endpoint.ContentType))
            {
                request.AddHeader("Content-Type", endpoint.ContentType);
            }
            if (!string.IsNullOrWhiteSpace(endpoint.ContentEncoding))
            {
                request.AddHeader("Content-Transfer-Encoding", endpoint.ContentEncoding);
            }

            if (endpoint.Headers != null && endpoint.Headers.Count > 0)
            {
                endpoint.Headers.Keys.ToList().ForEach(x => {
                    if (x.ToLower() != "customerid")
                    {
                        request.AddHeader(x, endpoint.Headers[x].ToString());
                    }
                });
            }

            if (endpoint.Method.ToUpper() == "GET")
            {
                request.Resource = requestbody.Trim().Replace('\n', ' ');
            }
            else
            {
                request.AddParameter(endpoint.ContentType, requestbody, ParameterType.RequestBody);
            }

            IRestResponse response = client.Execute(request);
            if (200 <= ((int)response.StatusCode) && ((int)response.StatusCode) < 400)
            {
                //context.WriteLine(ConsoleTextColor.DarkGreen, $"Request Successded By Status Code{response.StatusCode}");
                return response.Content;
            }
            else
            {
                //context?.WriteLine(ConsoleTextColor.Red, $"Request Was not Sucess\n Content:{response?.Content},\nStatusCode:{response?.StatusCode}\nErrorMessage:{response?.ErrorMessage}\nResponseStatus:{response?.ResponseStatus}\nStatusDescription:{response?.StatusDescription}\nErrorException:{response?.ErrorException}");
                throw new BusinessException(ErrorCode.EventFailed, !string.IsNullOrWhiteSpace(response.Content) ? response.Content : response.ErrorException?.ToString() ?? response.Content);
            }
        }
    }

    public interface IAuthTokenRefreshService
    {
        void RefreshTokenAllClients();
        void GenerateNewToken(GpsEndPoint endpoint);
        void RefreshExistingToken(GpsEndPoint endpoint);
    }
}
