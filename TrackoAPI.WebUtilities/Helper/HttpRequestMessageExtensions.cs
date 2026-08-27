using Microsoft.AspNet.SignalR;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

using Tenant.Models;

using TrackoApi.Core.Helpers;
using TrackoAPI.Hubs;
using TrackoAPI.Infrastructure;
using TrackoAPI.SignalR.Core;
using Unity;
using Unity.AspNet.WebApi;

namespace TrackoAPI.WebUtilities.Helper
{
    public static class HttpRequestMessageExtensions
    {
        private const string DbContext = "Batch_DbContext";
        private const string AuthContext = "AuthRepoContext";
        private const string Hub = "Hub";
        private const string TenantDb = "Batch_TenantDb";

        /// <summary>
        /// Sets the context.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        public static void SetContext(this HttpRequestMessage request, IUnitOfWorkAsync context)
        {
            try
            {
                request.Properties[DbContext] = context;
            }
            catch (Exception)
            {
                throw;
            }
        
        }
        /// <summary>
        /// Sets the context.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        public static void SetTenantDb(this HttpRequestMessage request, ITenantDbContext context)
        {
            try
            {
                request.Properties[TenantDb] = context;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public static ITenantDbContext GetTenantDb(this HttpRequestMessage request)
        {
            try
            {
                object trackoApiContext;
                if (request.Properties.TryGetValue(TenantDb, out trackoApiContext))
                {
                    return (ITenantDbContext)trackoApiContext;
                }
                var uhdr = (UnityHierarchicalDependencyResolver)request.GetConfiguration().DependencyResolver;
                var _uow = ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer))).Resolve<ITenantDbContext>();
                SetTenantDb(request, _uow);
                //request.RegisterForDispose(unity);
                return _uow;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static IUnitOfWorkAsync GetContext(this HttpRequestMessage request)
        {
            try
            {
                object trackoApiContext;
                if (request.Properties.TryGetValue(DbContext, out trackoApiContext))
                {
                    return (IUnitOfWorkAsync)trackoApiContext;
                }
                var uhdr = (UnityHierarchicalDependencyResolver)request.GetConfiguration().DependencyResolver;
                var _uow = ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer))).Resolve<IUnitOfWorkAsync>();
                SetContext(request, _uow);
                //request.RegisterForDispose(unity);
                return _uow;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static IClientHub GetHubContext(this HttpRequestMessage request)
        {
            try
            {
                object context;
                if (request.Properties.TryGetValue(Hub, out context))
                {
                    return (IClientHub)context;
                }
                var uhdr = (UnityHierarchicalDependencyResolver)request.GetConfiguration().DependencyResolver;
                var _uow = ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer))).Resolve<IClientHub>();
                SetHubContext(request, _uow);
                //request.RegisterForDispose(unity);
                return _uow;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void SetHubContext(this HttpRequestMessage request, IClientHub context)
        {
            try
            {
                request.Properties[Hub] = context;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public static int GetFinanceStatus(this HttpRequestMessage request)
        {
            //TODO:Retrive this from Claims of User
            return 1;
        }
        public static void PushBackMessage(this HttpRequestMessage request, string message, string title, PushSelfMessageType type)
        {
            var context = request.GetHubContext();
            var connectionId = request.GetHeader("SignalRConnectionId");
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                context?.PushEventSelf(connectionId, message, title, type);
            }
        }
        public static void BroadCastMessageExceptMe(this HttpRequestMessage request, string message, string title)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ClientHub>();
            var tenantId = TrackoApi.Core.Helpers.Helper.LoggedInTenantId;
            var connectionId = request.GetHeader("SignalRConnectionId");
            if (!string.IsNullOrWhiteSpace(tenantId)&& !string.IsNullOrWhiteSpace(connectionId))
            {
                context.Clients.AllExcept(connectionId).Group(tenantId).BroadCastMessage(message, title);
            }
        }
        public static void SetSecurityContext(this HttpRequestMessage request, IAuthRepository context)
        {
            try
            {
                request.Properties[AuthContext] = context;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static IAuthRepository GetSecurityContext(this HttpRequestMessage request)
        {
            try
            {
                object authRepository;
                if (request.Properties.TryGetValue(AuthContext, out authRepository))
                {
                    return (IAuthRepository)authRepository;
                }
                var uhdr = (UnityHierarchicalDependencyResolver)request.GetConfiguration().DependencyResolver;
                var _auth = ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer))).Resolve<IAuthRepository>();
                SetSecurityContext(request, _auth);
                //request.RegisterForDispose(unity);
                return _auth;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static IHttpActionResult Ok(this ApiController controller,object content, Type type)
        {
            Type resultType = typeof(OkNegotiatedContentResult<>).MakeGenericType(type);
            return Activator.CreateInstance(resultType, content, controller) as IHttpActionResult;
        }
        public static string RelativePath(this HttpServerUtility srv, string path, HttpRequest context)
        {

            return path.Replace(context.ServerVariables["APPL_PHYSICAL_PATH"], "~/").Replace(@"\", "/");
        }

        public static string GetHeader(this HttpRequestMessage request,string headername)
        {
            
            if (request.Headers!=null&&request.Headers.Any(x => x.Key == headername)) return request.Headers.FirstOrDefault(x => x.Key == headername).Value.JoinStrings(",");
            if (request.Content!=null&&request.Content.Headers.Any(x => x.Key == headername)) return request.Content.Headers.FirstOrDefault(x => x.Key == headername).Value.JoinStrings(",");
            return string.Empty;
        }
        #region OData Action Batch Handler Code 
        private const string HttpMediaType = @"application/http";
        private const string MessageTypeHeaderParameter = "msgtype";
        private const string HttpRequestHeaderParameter = "request";
        private const string HostHeader = "Host";
        private const string HttpRequestPattern = @"(\S+\s)(.+)(\sHTTP.*)";

        private static bool FixUpHttpRequest(string requestLine, out string fixedUpRequestLine)
        {
            Contract.Requires(!string.IsNullOrEmpty(requestLine));

            fixedUpRequestLine = null;
            var match = Regex.Match(requestLine, HttpRequestPattern, RegexOptions.Singleline);

            if (!match.Success)
                return false;

            var url = match.Groups[2].Value;
            Uri uri;

            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
                return false;

            // the request url must be either absolute or start with a '/'
            if (uri.IsAbsoluteUri || url[0] == '/')
                return false;

            fixedUpRequestLine = string.Format(CultureInfo.InvariantCulture, "{0}/{1}{2}", match.Groups[1], url, match.Groups[3]);

            return true;
        }

        ///// <summary>
        ///// Returns a value indicating whether the specified request is for an OData action.
        ///// </summary>
        ///// <param name="request">The <see cref="HttpRequestMessage">request</see> to evaluate.</param>
        ///// <returns>True if the <paramref name="request"/> is for an OData action; otherwise, false.</returns>
        //[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract.")]
        //public static bool IsODataAction(this HttpRequestMessage request)
        //{
        //    Contract.Requires(request != null);

        //    if (request.Method != HttpMethod.Post)
        //        return false;

        //    var config = request.GetConfiguration();

        //    if (config == null)
        //        return false;

        //    // HACK: because of the HttpRouteCollection implementation OfType yields no results and ToArray and ToList
        //    // throw exceptions. Skip(0) does the trick although there is no logical difference.
        //    var url = (request.RequestUri.IsAbsoluteUri ? request.RequestUri.LocalPath : request.RequestUri.OriginalString).TrimStart('/');
        //    var routes = config.Routes.Skip(0).OfType<ODataRoute>();
        //    var actions = from route in routes
        //                  from constraint in route.Constraints.Values.OfType<ODataPathRouteConstraint>()
        //                  let segment = url.Substring((route.RoutePrefix ?? string.Empty).Length)
        //                  let path = constraint.PathHandler.Parse(constraint.EdmModel, segment,url)
        //                  where path != null && path.Segments.Last() is ActionPathSegment
        //                  select true;
        //    var matched = actions.Any();

        //    return matched;
        //}

        /// <summary>
        /// Normalizes the content of a nested HTTP message.
        /// </summary>
        /// <param name="content">The nested <see cref="HttpContent">HTTP message</see> to normalize.</param>
        /// <param name="host">The value applied to the HTTP Host header, if necessary. This value should be derived
        /// from a parent MIME multipart <see cref="HttpRequestMessage">request</see>.</param>
        /// <returns>The normalized <see cref="HttpContent">content</see>.</returns>
        public static async Task<HttpContent> NormalizeNestedHttpMessageContentAsync(this HttpContent content, string host)
        {
            Contract.Requires(content != null);
            Contract.Requires(!string.IsNullOrEmpty(host));
            Contract.Ensures(Contract.Result<Task<HttpContent>>() != null);

            // HACK: ReadAsHttpRequestMessageAsync normalization:
            // --------------------------------------------------------------------------------------------------------------
            // 1. the Host header must be present in the content and the ContentHttpHeaders will not allow the Host header
            //    to be added. we could have tried using some sleazy Reflection, but it is just as easy to read the content,
            //    inject the host header, and reassemble the content.
            //
            // 2. the url for a nested request is often relative; however, the method fails if the url either isn't
            //    absolute or the url doesn't start with a '/'

            var text = await content.ReadAsStringAsync();
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            // defense; this should never happen
            if (lines.Count == 0)
                return content;

            var hasHostHeader = false;
            var hostKey = HostHeader + ":";
            var comparison = StringComparison.OrdinalIgnoreCase;
            string requestLine;

            // fix up request line if necessary
            if (FixUpHttpRequest(lines[0], out requestLine))
            {
                // replace line one with fixed up request
                lines[0] = requestLine;

                // check if the host header is present
                hasHostHeader = lines.Any(l => l.StartsWith(hostKey, comparison));
            }
            else if (lines.Count > 1)
            {
                // if there's only one line, it can't have a host header; check
                // remaining content. if the header is present and we didn't
                // perform a fix up, then we can leave the content as is
                if (hasHostHeader = lines.Any(l => l.StartsWith(hostKey, comparison)))
                    return content;
            }

            // inject as necessary; order isn't really important here
            if (!hasHostHeader)
                lines.Insert(1, string.Format(CultureInfo.InvariantCulture, "{0}:{1}", HostHeader, host));

            // reassemble the content
            text = string.Join(Environment.NewLine, lines);
            content = new StringContent(text);
            content.Headers.ContentType = new MediaTypeHeaderValue(HttpMediaType)
            {
                Parameters =
                {
                    new NameValueHeaderValue( MessageTypeHeaderParameter, HttpRequestHeaderParameter )
                }
            };

            return content;
        }
#endregion
    }
    public class TextResult : IHttpActionResult
    {
        string _value;
        HttpRequestMessage _request;
        private HttpStatusCode _statuscode;

        public TextResult(string value, HttpRequestMessage request,HttpStatusCode httpStatusCode=HttpStatusCode.OK)
        {
            _value = value;
            _request = request;
            _statuscode = httpStatusCode;
        }
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage()
            {
                Content = new StringContent(_value),
                RequestMessage = _request,
                StatusCode=_statuscode
            };
            return Task.FromResult(response);
        }
    }
}