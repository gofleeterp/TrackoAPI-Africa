using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;
using TinyJson;

namespace GoFleetCLR.Functions
{
    public class HttpResultEntity
    {
        public string RequestId { get; set; }
        public string ResultBody { get; set; }
        public string Headers { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }

    public static class BatchHttpCalls
    {
        [SqlFunction(
            DataAccess = DataAccessKind.Read,
            FillRowMethodName = "FillRow",
            TableDefinition =
                "requestId nvarchar(500),resultBody nvarchar(max),headers nvarchar(max),statusCode int,errorMessage nvarchar(max),stackTrace nvarchar(max)"
        )]
        public static IEnumerable ExecuteBatchRequest(
            [SqlFacet(MaxSize = -1)] SqlString selectQuery,SqlBoolean isDebug)
        {
            var result = new List<HttpResultEntity>();
            var dt = new DataTable();
            if (string.IsNullOrWhiteSpace(selectQuery.ToString())) return result;
            try
            {
                using (var connection = new SqlConnection("context connection=true"))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = selectQuery.ToString();
                        cmd.CommandType = CommandType.Text;
                        using (IDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.Add(new HttpResultEntity()
                {
                    ErrorMessage = e.GetBaseException().Message,
                    StackTrace = e.ToString()
                });
                return result;
            }
            if (dt.Rows.Count <= 0) return result;
            IQueryable<Task<HttpResultEntity>> requests = from req in dt.Select().AsQueryable()
                select ProcessHttpRequest(req,isDebug.Value);
            // Parallel.ForEach(requests, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }
            var task=Task.WhenAll(requests).ContinueWith(reqs =>
            {
                if (reqs.Status == TaskStatus.RanToCompletion)
                {
                    result.AddRange(reqs.Result);
                }
            });
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Task.WaitAll(task);
            return result;
        }

        private static async Task<HttpResultEntity> ProcessHttpRequest(DataRow requestRow, bool isDebug)
        {
            try
            {
                var requestMethod = requestRow["Method"]?.ToString();
                if (string.IsNullOrWhiteSpace(requestMethod))
                    throw new ArgumentNullException("Method", "Http Method Name is blank");
                var url = requestRow["Uri"]?.ToString();
                if (string.IsNullOrWhiteSpace(url))
                    throw new ArgumentNullException("Url", "Http Request Url Name is blank");
                var parameters = requestRow["RequestBody"]?.ToString();
                var headers = requestRow["Headers"]?.ToString();
                var timeout = (requestRow["Timeout"] as int?) ?? -1;
                var autoDecompress = (requestRow["Autodecompress"] as bool?) ?? false;
                var convertResponseToBas64 = (requestRow["ResponseTobase64"] as bool?) ?? false;
                var requestId = requestRow["RequestId"]?.ToString();
                if (string.IsNullOrWhiteSpace(requestId))
                    throw new ArgumentNullException("RequestId", "Http RequestId is blank");
                
                // If GET request, and there are parameters, build into url
                if (requestMethod?.ToUpper() == "GET" && !string.IsNullOrWhiteSpace(parameters))
                    url += (url?.IndexOf('?') > 0 ? "&" : "?") + parameters;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Create an HttpWebRequest with the url
                var request = (HttpWebRequest) WebRequest.Create(url);

                // Add in any headers provided
                var contentLengthSetFromHeaders = false;
                var contentTypeSetFromHeaders = false;
                if (!string.IsNullOrWhiteSpace(headers))
                {
                    var headersLines = headers.Split(
                        new[] { "^!^", "^!!^","^!!!^"},
                        StringSplitOptions.None);
                    foreach (var headerLine in headersLines)
                    {
                        var header = headerLine.Split(':');
                        // Retrieve header's name and value
                        var headerValue =header?[1]?.Trim();
                        var headerName = header?[0]?.Trim();
                        if (string.IsNullOrWhiteSpace(headerName))
                        {
                            throw new ArgumentNullException("HeaderName",headerLine);
                        }
                        /*
                         * You cannot use SqlContext.Pipe in CLR Function
                         */
                        // if (SqlContext.IsAvailable)
                        // {
                        //     SqlContext.Pipe.Send($"{headerName}:{headerValue}");
                        // }
                        // Some headers cannot be set by request.Headers.Add() and need to set the HttpWebRequest property directly
                        switch (headerName)
                        {
                            case "Accept":
                                request.Accept = headerValue;
                                break;
                            case "Connection":
                                request.Connection = headerValue;
                                break;
                            case "Content-Length":
                                request.ContentLength = long.Parse(headerValue);
                                contentLengthSetFromHeaders = true;
                                break;
                            case "Content-Type":
                                request.ContentType = headerValue;
                                contentTypeSetFromHeaders = true;
                                break;
                            case "Date":
                                request.Date = DateTime.Parse(headerValue);
                                break;
                            case "Expect":
                                request.Expect = headerValue;
                                break;
                            case "Host":
                                request.Host = headerValue;
                                break;
                            case "If-Modified-Since":
                                request.IfModifiedSince = DateTime.Parse(headerValue);
                                break;
                            case "Range":
                                var parts = headerValue?.Split('-');
                                request.AddRange(int.Parse(parts?[0]), int.Parse(parts?[1]));
                                break;
                            case "Referer":
                                request.Referer = headerValue;
                                break;
                            case "Transfer-Encoding":
                                request.TransferEncoding = headerValue;
                                break;
                            case "User-Agent":
                                request.UserAgent = headerValue;
                                break;
                            case "Authorization-Bearer-Credentials":
                                request.Headers.Add("Authorization", $"Bearer {headerValue}");
                                break;
                            case "Authorization-Basic-Credentials":
                                request.Headers.Add("Authorization",
                                    "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(headerValue)));
                                break;
                            case "Authorization-Network-Credentials":
                                request.Credentials =
                                    new NetworkCredential(headerValue?.Split(':')?[0], headerValue?.Split(':')?[1]);
                                break;
                            default: // other headers
                                if (!string.IsNullOrWhiteSpace(headerName))
                                    request.Headers.Add(headerName, headerValue);
                                break;
                        }
                    }
                }
                
                // Set the method, timeout, and decompression
                request.Method = requestMethod.ToUpper();
                request.Timeout = timeout;
                if (autoDecompress)
                    request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                //Main Logic Begins here
                // Retrieve results from response
                string requestHeaders = string.Empty;
                if (request.Headers != null&&isDebug)
                {
                    var items = Enumerable
                        .Range(0, request.Headers.Count)
                        .SelectMany(i =>
                            (request.Headers.GetValues(i) ?? new string[] { })
                            .Select(v => new KeyValuePair<string, object>(request.Headers.GetKey(i), v))
                        );
                    requestHeaders = items.ToDictionary(pair => pair.Key, pair => pair.Value).ToJson();
                }

                try
                {
                    // Add in non-GET parameters provided
                    if (requestMethod.ToUpper() != "GET" && !string.IsNullOrWhiteSpace(parameters))
                    {
                        // Convert to byte array
                        var parameterData = Encoding.ASCII.GetBytes(parameters);

                        // Set content info
                        if (!contentLengthSetFromHeaders) request.ContentLength = parameterData.Length;
                        if (!contentTypeSetFromHeaders) request.ContentType = "application/x-www-form-urlencoded";

                        // Add data to request stream
                        using (var stream = request.GetRequestStream())
                        {
                            stream.Write(parameterData, 0, parameterData.Length);
                        }
                    }

                    using (var response = await request.GetResponseAsync())
                    {
                        return ParseResponse(response, convertResponseToBas64, requestId);
                    }
                }
                catch (WebException ex)
                {
                    var res = new HttpResultEntity
                    {
                        RequestId = requestId,
                        ErrorMessage = ex.GetBaseException().Message,
                        StackTrace = ex.StackTrace
                    };
                    var entity= ParseResponse(ex.Response, convertResponseToBas64, requestId, res);
                    if (isDebug)
                    {
                        entity.Headers = requestHeaders;
                    }
                    return entity;

                }
                catch (Exception ex)
                {
                    request.Abort();
                    return new HttpResultEntity
                    {
                        RequestId = requestId,
                        ErrorMessage = ex.GetBaseException().Message,
                        StackTrace = ex.StackTrace
                    };
                }
            }
            catch (Exception ex)
            {
                string requestId = requestRow["RequestId"]?.ToString();
                var res = new HttpResultEntity
                {
                    RequestId = requestId,
                    ErrorMessage = ex.GetBaseException().Message,
                    StackTrace = ex.ToString(),
                    ResultBody = requestRow.ItemArray.ToJson()
                };
                return res;
            }
        }

        private static HttpResultEntity ParseResponse(WebResponse responseTask,
            SqlBoolean convertResponseToBas64, SqlString requestId, HttpResultEntity entity = null)
        {
            var response = (HttpWebResponse) responseTask;
            entity = entity ?? new HttpResultEntity();
            entity.RequestId = requestId.Value;
            // Get headers (loop through response's headers)
            var responseHeaders = response.Headers;
            var resHeaders = new Dictionary<string, object>();
            for (var i = 0; i < responseHeaders.Count; ++i)
            {
                // Get values for this header
                var values = "";
                var vls = responseHeaders?.GetValues(i);
                if (vls != null)
                    values = vls.Aggregate(values,
                        (current, value) => current + $"{(string.IsNullOrWhiteSpace(current) ? "" : ";")}{value}");

                resHeaders.AddIfNotExists(responseHeaders.GetKey(i), values);
            }

            resHeaders.AddIfNotExists("CharacterSet", response.CharacterSet);
            resHeaders.AddIfNotExists("ContentEncoding", response.ContentEncoding);
            resHeaders.AddIfNotExists("ContentLength", response.ContentLength);
            resHeaders.AddIfNotExists("ContentType", response.ContentType);
            resHeaders.AddIfNotExists("CookiesCount", response.Cookies.Count);
            resHeaders.AddIfNotExists("HeadersCount", response.Headers.Count);
            resHeaders.AddIfNotExists("IsFromCache", response.IsFromCache);
            resHeaders.AddIfNotExists("IsMutuallyAuthenticated", response.IsMutuallyAuthenticated);
            resHeaders.AddIfNotExists("LastModified", response.LastModified);
            resHeaders.AddIfNotExists("Method", response.Method);
            resHeaders.AddIfNotExists("ProtocolVersion", response.ProtocolVersion);
            resHeaders.AddIfNotExists("ResponseUri", response.ResponseUri);
            resHeaders.AddIfNotExists("Server", response.Server);
            resHeaders.AddIfNotExists("StatusCode", response.StatusCode);
            resHeaders.AddIfNotExists("StatusNumber", (int) response.StatusCode);
            resHeaders.AddIfNotExists("StatusDescription", response.StatusDescription);
            resHeaders.AddIfNotExists("SupportsHeaders", response.SupportsHeaders);
            entity.StatusCode = (int) response.StatusCode;
            entity.Headers = resHeaders.ToJson();
            //entity.Headers = JsonConvert.SerializeObject(resHeaders);
            // var ser = new DataContractJsonSerializer(typeof(Dictionary<string, object>));
            // using (var stream1 = new MemoryStream())
            // {
            //     ser.WriteObject(stream1, resHeaders);
            //     stream1.Position = 0;
            //     var sr = new StreamReader(stream1);
            //     entity.Headers = sr.ReadToEnd();
            // }

            // Get the response body
            string responseString=string.Empty;
            using (var stream = response.GetResponseStream())
            {
                // If requested to convert to base 64 string, use memory stream, otherwise stream reader
                if (convertResponseToBas64)
                    using (var memoryStream = new MemoryStream())
                    {
                        // Copy response stream to memory stream
                        stream?.CopyTo(memoryStream);

                        // Convert memory stream to a byte array
                        var bytes = memoryStream.ToArray();

                        // Convert to base 64 string
                        responseString = Convert.ToBase64String(bytes);
                    }
                else if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        // Retrieve response string
                        responseString = reader.ReadToEnd();
                    }
                }
            }

            entity.ResultBody = responseString;
            response.Close();
            return entity;
        }

        public static void FillRow(object obj, out SqlChars requestId, out SqlChars resultBody, out SqlChars headers,
            out SqlInt32 statusCode, out SqlChars errorMessage, out SqlChars stackTrace)
        {
            var result = (HttpResultEntity) obj;
            requestId = new SqlChars(result.RequestId);
            resultBody = new SqlChars(result.ResultBody);
            statusCode = new SqlInt32(result.StatusCode);
            errorMessage = new SqlChars(result.ErrorMessage);
            stackTrace = new SqlChars(result.StackTrace);
            headers = new SqlChars(result.Headers);
        }

        private static void AddIfNotExists<TKey, TValue>(this IDictionary<TKey, TValue> dics, TKey key, TValue value)
        {
            if (dics.ContainsKey(key)) return;
            dics.Add(key, value);
        }

        private static void ReplaceIfExists<TKey, TValue>(this IDictionary<TKey, TValue> dics, TKey key, TValue value)
        {
            if (dics.ContainsKey(key))
            {
                dics[key] = value;
                return;
            }

            dics.Add(key, value);
        }

        public static string JoinString<T>(this IEnumerable<T> sequence, string separator, Func<T, string> convertor)
        {
            var seed = new StringBuilder();
            sequence.Aggregate(seed, (builder, item) =>
            {
                if (builder.Length > 0 && !string.IsNullOrWhiteSpace(separator)) builder.Append(separator);
                builder.Append(convertor(item));
                return builder;
            });
            return seed.ToString();
        }

        public static string JoinStrings<T>(this IEnumerable<T> sequence, string separator)
        {
            return JoinString(sequence, separator, t => t.ToString());
        }
    }
}