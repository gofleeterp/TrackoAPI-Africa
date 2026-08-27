using MimeKit;
using MimeKit.Utils;

using Newtonsoft.Json;

using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

using RestSharp;

using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Core.Helpers;
using TrackoApi.MessageService;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

using TrackoAPI.Infrastructure.Services;

namespace TrackoApi.Service.Global
{
    public static class Extentions
    {
        public static async Task AddRequest(this IRestClient client, HttpRequestPool req)
        {
            bool isError = false;
            try
            {
                req.NoofAttempts = req.NoofAttempts ?? 0;

                var contentType = "application/json";
                var watch = new Stopwatch();

                var method = (RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), req.Method.ToUpper());

                var request = new RestRequest(req.Uri, method);
                var _headervalue = string.Empty;
                foreach (var item in req._headers)
                {
                    _headervalue = item.Value.ToString();

                    if (item.Key.ToLower() == "content-type" && !string.IsNullOrWhiteSpace(_headervalue))
                    { contentType = _headervalue; }

                    if (item.Key.ToLower() == "authorization" && _headervalue.ToLower().Contains("basicauth"))
                    {
                        try
                        {
                            var _authdata = item.Value.ToString();
                            var cred = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authdata.Substring(10)));
                            _headervalue = $"Basic {cred}";
                        }
                        catch (Exception ex)
                        {
                            req.Result = $"Authorization Conversion Failed:{ex.Message}.\n Sample should be looks like:'{{\"Authorization\": \"BasicAuth:admin:admin123\"}}';";
                        }
                    }
                    request.AddHeader(item.Key, _headervalue);
                }
                if (req.Timeout <= 0)
                {
                    req.Timeout = 18000;
                }
                if (contentType.Contains("json"))
                {
                    request.RequestFormat = DataFormat.Json;
                }
                else if (contentType.Contains("xml"))
                {
                    request.RequestFormat = DataFormat.Xml;
                }
                if (!string.IsNullOrWhiteSpace(req.RequestBody))
                {
                    request.AddParameter(contentType, req.RequestBody, ParameterType.RequestBody);
                }
                IRestResponse res;
                do
                {
                    watch.Start();
                    res = await client.ExecuteTaskAsync(request);
                    req.ExecutedTime = req.ProcessTime = DateTime.Now;
                    watch.Stop();
                    req.Result = res.Content;

                    if (string.IsNullOrWhiteSpace(req.Result) && res.ErrorException != null)
                    {
                        isError = true;
                        req.Result = res.ErrorException.GetBaseException().Message;
                    }
                    else if (string.IsNullOrWhiteSpace(req.Result))
                    {
                        isError = true;
                        req.Result = res.ErrorMessage;
                    }
                    else if (!string.IsNullOrWhiteSpace(req.Result) && !string.IsNullOrWhiteSpace(req.SuccessString) && !req.Result.StartsWith(req.SuccessString))
                    {
                        isError = true;
                    }
                    req.NoofAttempts--;
                }
                while (isError && req.NoofAttempts > 0);

                req.IsProceeded = !isError;
                if (req.LogRequest || isError)
                {
                    var log = LogRequest(client, request, res, watch.ElapsedMilliseconds);
                    req.LogData = JsonConvert.SerializeObject(log);
                }
            }
            catch (Exception ex)
            {
                req.Result = ex.GetBaseException()?.Message;
            }
        }

        public static async Task AddRequestWithDelay(this IRestClient client, HttpRequestPool req, int delayinsecond = 1)
        {
            bool isError = false;
            try
            {
                req.NoofAttempts = req.NoofAttempts ?? 0;

                var contentType = "application/json";
                var watch = new Stopwatch();

                var method = (RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), req.Method.ToUpper());

                var request = new RestRequest(req.Uri, method);
                var _headervalue = string.Empty;
                foreach (var item in req._headers)
                {
                    _headervalue = item.Value.ToString();

                    if (item.Key.ToLower() == "content-type" && !string.IsNullOrWhiteSpace(_headervalue))
                    { contentType = _headervalue; }

                    if (item.Key.ToLower() == "authorization" && _headervalue.ToLower().Contains("basicauth"))
                    {
                        try
                        {
                            // Remove "BasicAuth:" safely
                            var rawCreds = _headervalue.Substring("BasicAuth:".Length);
                            // Encode username:password
                            var cred = Convert.ToBase64String(
                                Encoding.UTF8.GetBytes(rawCreds));
                
                                            _headervalue = $"Basic {cred}";
                        }
                        catch (Exception ex)
                        {
                            req.Result = $"Authorization Conversion Failed:{ex.Message}.\n Sample should be looks like:'{{\"Authorization\": \"BasicAuth:admin:admin123\"}}';";
                        }
                    }
                    request.AddHeader(item.Key, _headervalue);
                }
                if (req.Timeout <= 0)
                {
                    req.Timeout = 18000;
                }
                if (contentType.Contains("json"))
                {
                    request.RequestFormat = DataFormat.Json;
                }
                else if (contentType.Contains("xml"))
                {
                    request.RequestFormat = DataFormat.Xml;
                }
                if (!string.IsNullOrWhiteSpace(req.RequestBody))
                {
                    request.AddParameter(contentType, req.RequestBody, ParameterType.RequestBody);
                }
                IRestResponse res;
                do
                {
                    watch.Start();
                    #region adding delay if error occur for next call
                    try { await Task.Delay(delayinsecond * 2000); } catch { }
                    #endregion
                    res = await client.ExecuteTaskAsync(request);
                    req.ExecutedTime = req.ProcessTime = DateTime.Now;
                    watch.Stop();
                    req.Result = res.Content;

                    if (string.IsNullOrWhiteSpace(req.Result) && res.ErrorException != null)
                    {
                        isError = true;
                        req.Result = res.ErrorException.GetBaseException().Message;
                    }
                    else if (string.IsNullOrWhiteSpace(req.Result))
                    {
                        isError = true;
                        req.Result = res.ErrorMessage;
                    }
                    else if (!string.IsNullOrWhiteSpace(req.Result) && !string.IsNullOrWhiteSpace(req.SuccessString) && !req.Result.StartsWith(req.SuccessString))
                    {
                        isError = true;
                    }
                    req.NoofAttempts--;
                }
                while (isError && req.NoofAttempts > 0);

                req.IsProceeded = !isError;
                if (req.LogRequest || isError)
                {
                    var log = LogRequest(client, request, res, watch.ElapsedMilliseconds);
                    req.LogData = JsonConvert.SerializeObject(log);
                }
            }
            catch (Exception ex)
            {
                req.Result = ex.GetBaseException()?.Message;
            }
        }
        private static dynamic LogRequest(IRestClient _restClient, IRestRequest request, IRestResponse response, long durationMs)
        {
            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = _restClient.BuildUri(request)
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };

            return new
            {
                requestToLog,
                responseToLog
            };
        }
    }
}
