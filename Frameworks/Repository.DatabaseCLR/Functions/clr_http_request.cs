using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;

/// <summary>
///     clr_http_request was originally written by Eilert Hjelmeseth
///     and was published on 2018/10/11 here: http://www.sqlservercentral.com/articles/SQLCLR/177834/
///     This version has minor improvements that allow it to support TLS1.2 security protocol
///     and a couple of additional authorization methods.
/// </summary>
public class UserDefinedFunctions
{
    [SqlFunction]
    public static SqlXml clr_http_request(
        [SqlFacet(MaxSize = 10)] SqlString requestMethod,
        [SqlFacet(MaxSize = -1)] SqlString url,
        [SqlFacet(MaxSize = -1)] SqlString parameters,
        [SqlFacet(MaxSize = -1)] SqlString headers,
        SqlInt32 timeout,
        SqlBoolean autoDecompress,
        SqlBoolean convertResponseToBas64
        //, bool debug
    )
    {
        // If GET request, and there are parameters, build into url
        if (requestMethod.Value.ToUpper() == "GET" && !string.IsNullOrWhiteSpace(parameters.Value))
            url += (url.Value.IndexOf('?') > 0 ? "&" : "?") + parameters;

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        // Create an HttpWebRequest with the url
        var request = (HttpWebRequest) WebRequest.Create(url.Value);

        // Add in any headers provided
        var contentLengthSetFromHeaders = false;
        var contentTypeSetFromHeaders = false;
        if (!string.IsNullOrWhiteSpace(headers.Value))
        {
            // Parse provided headers as XML and loop through header elements
            var xmlData = XElement.Parse(headers.Value);
            foreach (var headerElement in xmlData.Descendants())
            {
                // Retrieve header's name and value
                var headerName = headerElement.Attribute("Name").Value;
                var headerValue = headerElement.Value;

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
                        var parts = headerValue.Split('-');
                        request.AddRange(int.Parse(parts[0]), int.Parse(parts[1]));
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
                            new NetworkCredential(headerValue.Split(':')[0], headerValue.Split(':')[1]);
                        break;
                    default: // other headers
                        request.Headers.Add(headerName, headerValue);
                        break;
                }
            }
        }

        // Set the method, timeout, and decompression
        request.Method = requestMethod.Value.ToUpper();
        request.Timeout = timeout.Value;
        if (autoDecompress) request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        //Main Logic Begins here
        // Retrieve results from response
        XElement returnXml;
        try
        {
            // Add in non-GET parameters provided
            if (requestMethod.Value.ToUpper() != "GET" && !string.IsNullOrWhiteSpace(parameters.Value))
            {
                // Convert to byte array
                var parameterData = Encoding.ASCII.GetBytes(parameters.Value);

                // Set content info
                if (!contentLengthSetFromHeaders) request.ContentLength = parameterData.Length;
                if (!contentTypeSetFromHeaders) request.ContentType = "application/x-www-form-urlencoded";

                // Add data to request stream
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(parameterData, 0, parameterData.Length);
                }
            }

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                returnXml = ParseResponse(response, convertResponseToBas64);
                response.Close();
            }
        }
        catch (WebException ex)
        {
            returnXml = ParseResponse((HttpWebResponse) ex.Response, convertResponseToBas64);
            ex.Response.Close();
        }
        catch (Exception ex)
        {
            request.Abort();
            throw;
        }

        // Return data
        return new SqlXml(returnXml.CreateReader());
    }

    private static XElement ParseResponse(HttpWebResponse response, SqlBoolean convertResponseToBas64)
    {
        XElement returnXml = null;
        // Get headers (loop through response's headers)
        var headersXml = new XElement("Headers");
        var responseHeaders = response.Headers;
        for (var i = 0; i < responseHeaders.Count; ++i)
        {
            // Get values for this header
            var values = responseHeaders.GetValues(i).Aggregate("", (current, value) => current + $"{(string.IsNullOrWhiteSpace(current) ? "" : ";")}{value}");
            var header = new XElement(responseHeaders.GetKey(i),
                //"Header",
                //    new XAttribute("Name", responseHeaders.GetKey(i)),
                values
            );
            // Add this header with its values to the headers xml
            headersXml.Add(header);
        }

        // Get the response body
        var responseString = string.Empty;
        using (var stream = response.GetResponseStream())
        {
            // If requested to convert to base 64 string, use memory stream, otherwise stream reader
            if (convertResponseToBas64)
                using (var memoryStream = new MemoryStream())
                {
                    // Copy response stream to memory stream
                    stream.CopyTo(memoryStream);

                    // Convert memory stream to a byte array
                    var bytes = memoryStream.ToArray();

                    // Convert to base 64 string
                    responseString = Convert.ToBase64String(bytes);
                }
            else
                using (var reader = new StreamReader(stream))
                {
                    // Retrieve response string
                    responseString = reader.ReadToEnd();
                }
        }

        // Assemble reponse XML from details of HttpWebResponse
        returnXml =
            new XElement("Response",
                new XElement("CharacterSet", response.CharacterSet),
                new XElement("ContentEncoding", response.ContentEncoding),
                new XElement("ContentLength", response.ContentLength),
                new XElement("ContentType", response.ContentType),
                new XElement("CookiesCount", response.Cookies.Count),
                new XElement("HeadersCount", response.Headers.Count),
                headersXml,
                new XElement("IsFromCache", response.IsFromCache),
                new XElement("IsMutuallyAuthenticated", response.IsMutuallyAuthenticated),
                new XElement("LastModified", response.LastModified),
                new XElement("Method", response.Method),
                new XElement("ProtocolVersion", response.ProtocolVersion),
                new XElement("ResponseUri", response.ResponseUri),
                new XElement("Server", response.Server),
                new XElement("StatusCode", response.StatusCode),
                new XElement("StatusNumber", (int) response.StatusCode),
                new XElement("StatusDescription", response.StatusDescription),
                new XElement("SupportsHeaders", response.SupportsHeaders),
                new XElement("Body", responseString)
            );
        return returnXml;
    }

    [SqlFunction]
    public static SqlXml clr_http_request_old(
        [SqlFacet(MaxSize = 10)] SqlString requestMethod,
        [SqlFacet(MaxSize = -1)] SqlString url,
        [SqlFacet(MaxSize = -1)] SqlString parameters,
        [SqlFacet(MaxSize = -1)] SqlString headers,
        SqlInt32 timeout,
        SqlBoolean autoDecompress,
        SqlBoolean convertResponseToBas64
        //, bool debug
    )
    {
        // If GET request, and there are parameters, build into url
        if (requestMethod.Value.ToUpper() == "GET" && !string.IsNullOrWhiteSpace(parameters.Value))
            url += (url.Value.IndexOf('?') > 0 ? "&" : "?") + parameters;

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        // Create an HttpWebRequest with the url
        var request = (HttpWebRequest) WebRequest.Create(url.Value);

        // Add in any headers provided
        var contentLengthSetFromHeaders = false;
        var contentTypeSetFromHeaders = false;
        if (!string.IsNullOrWhiteSpace(headers.Value))
        {
            // Parse provided headers as XML and loop through header elements
            var xmlData = XElement.Parse(headers.Value);
            foreach (var headerElement in xmlData.Descendants())
            {
                // Retrieve header's name and value
                var headerName = headerElement.Attribute("Name").Value;
                var headerValue = headerElement.Value;

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
                        var parts = headerValue.Split('-');
                        request.AddRange(int.Parse(parts[0]), int.Parse(parts[1]));
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
                            new NetworkCredential(headerValue.Split(':')[0], headerValue.Split(':')[1]);
                        break;
                    default: // other headers
                        request.Headers.Add(headerName, headerValue);
                        break;
                }
            }
        }

        // Set the method, timeout, and decompression
        request.Method = requestMethod.Value.ToUpper();
        request.Timeout = timeout.Value;
        if (autoDecompress) request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

        // Add in non-GET parameters provided
        if (requestMethod.Value.ToUpper() != "GET" && !string.IsNullOrWhiteSpace(parameters.Value))
        {
            // Convert to byte array
            var parameterData = Encoding.ASCII.GetBytes(parameters.Value);

            // Set content info
            if (!contentLengthSetFromHeaders) request.ContentLength = parameterData.Length;
            if (!contentTypeSetFromHeaders) request.ContentType = "application/x-www-form-urlencoded";

            // Add data to request stream
            using (var stream = request.GetRequestStream())
            {
                stream.Write(parameterData, 0, parameterData.Length);
            }
        }

        // Retrieve results from response
        XElement returnXml;
        using (var response = (HttpWebResponse) request.GetResponse())
        {
            // Get headers (loop through response's headers)
            var headersXml = new XElement("Headers");
            var responseHeaders = response.Headers;
            for (var i = 0; i < responseHeaders.Count; ++i)
            {
                // Get values for this header
                var values = responseHeaders.GetValues(i).Aggregate("", (current, value) => current + $"{(string.IsNullOrWhiteSpace(current) ? "" : ";")}{value}");
                var header = new XElement(responseHeaders.GetKey(i),
                    //"Header",
                    //    new XAttribute("Name", responseHeaders.GetKey(i)),
                    values
                );
                // Add this header with its values to the headers xml
                headersXml.Add(header);
            }

            // Get the response body
            var responseString = string.Empty;
            using (var stream = response.GetResponseStream())
            {
                // If requested to convert to base 64 string, use memory stream, otherwise stream reader
                if (convertResponseToBas64)
                    using (var memoryStream = new MemoryStream())
                    {
                        // Copy response stream to memory stream
                        stream.CopyTo(memoryStream);

                        // Convert memory stream to a byte array
                        var bytes = memoryStream.ToArray();

                        // Convert to base 64 string
                        responseString = Convert.ToBase64String(bytes);
                    }
                else
                    using (var reader = new StreamReader(stream))
                    {
                        // Retrieve response string
                        responseString = reader.ReadToEnd();
                    }
            }

            // Assemble reponse XML from details of HttpWebResponse
            returnXml =
                new XElement("Response",
                    new XElement("CharacterSet", response.CharacterSet),
                    new XElement("ContentEncoding", response.ContentEncoding),
                    new XElement("ContentLength", response.ContentLength),
                    new XElement("ContentType", response.ContentType),
                    new XElement("CookiesCount", response.Cookies.Count),
                    new XElement("HeadersCount", response.Headers.Count),
                    headersXml,
                    new XElement("IsFromCache", response.IsFromCache),
                    new XElement("IsMutuallyAuthenticated", response.IsMutuallyAuthenticated),
                    new XElement("LastModified", response.LastModified),
                    new XElement("Method", response.Method),
                    new XElement("ProtocolVersion", response.ProtocolVersion),
                    new XElement("ResponseUri", response.ResponseUri),
                    new XElement("Server", response.Server),
                    new XElement("StatusCode", response.StatusCode),
                    new XElement("StatusNumber", (int) response.StatusCode),
                    new XElement("StatusDescription", response.StatusDescription),
                    new XElement("SupportsHeaders", response.SupportsHeaders),
                    new XElement("Body", responseString)
                );
        }

        // Return data
        return new SqlXml(returnXml.CreateReader());
    }
}