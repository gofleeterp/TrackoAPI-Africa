using Conditions;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;

//using Repository.Pattern.Core.UnitOfWork;

namespace TrackoApi.Core.Helpers
{
	public static class Utilities
	{
		public static TKey GetPropertyValue<TKey>(this object obj, string propertyName)
		{
			var objType = obj.GetType();
			var returnType = typeof(TKey);
			var property = objType.GetProperty(propertyName);
			if (property == null) return default;
			var propertyValue= property?.GetValue(obj, null);
			if (propertyValue == null) return default;
			try
			{
				var objValue = Convert.ChangeType(propertyValue, returnType);
				return (TKey)objValue;
			}
			catch (InvalidCastException)
			{
				TypeConverter tc = TypeDescriptor.GetConverter(returnType);
				return (TKey)tc.ConvertFrom(propertyValue);
			}

		}
		public static decimal Cr(this decimal value) => -Math.Abs(value);
		public static decimal Dr(this decimal value) => Math.Abs(value);
		public static double Cr(this double value) => -Math.Abs(value);
		public static double Dr(this double value) => Math.Abs(value);
		public static decimal Reverse(this decimal value) => value>0?-value:Math.Abs(value);
		public static double Reverse(this double value) => value > 0 ? -value : Math.Abs(value);
		public static object GetPropertyValue(this object obj, string propertyName)
		{
			try
			{
				var objType = obj.GetType();
				var property = objType.GetProperty(propertyName);
				if (property == null) return default;
				var propertyValue = property?.GetValue(obj, null);
				if (propertyValue == null) return default;
				return propertyValue;
			}
			catch
			{
				return null;
			}
		}
		public static DbEntityEntry<TEntity> GetDbEntityEntry<TEntity>(this DbEntityEntry entry) where TEntity : class
		{
			var repositoryType = typeof(DbEntityEntry<>);
			return (DbEntityEntry<TEntity>)Activator.CreateInstance(repositoryType.MakeGenericType(entry.Entity.GetType()), entry.Entity);
		}		
		public static object ToType(this DbEntityEntry propertyValue)
		{
			if (propertyValue.IsNull()) return default;
			var targetType = propertyValue.Entity.GetType();
			try
			{
				var objValue = Convert.ChangeType(propertyValue.Entity, targetType);
				return objValue;
			}
			catch (InvalidCastException ex)
			{
				try
				{
					TypeConverter tc = TypeDescriptor.GetConverter(targetType);
					return tc.ConvertFrom(propertyValue);
				}
				catch (Exception)
				{
					return default;
				}

			}
		}
		public static TKey To<TKey>(object propertyValue)
		{
			if (propertyValue.IsNull()) return default(TKey);
			var returnType = typeof(TKey);
			try
			{
				var objValue = Convert.ChangeType(propertyValue, returnType);
				return (TKey)objValue;
			}
			catch (InvalidCastException ex)
			{
				try
				{
					TypeConverter tc = TypeDescriptor.GetConverter(returnType);
					return (TKey)tc.ConvertFrom(propertyValue);
				}
				catch (Exception)
				{
					return default(TKey);
				}
				
			}
		}
		public static TKey To<TKey>(object propertyValue,TKey defaultValue)
		{
			if (propertyValue.IsNull()) return defaultValue;
			var returnType = typeof(TKey);
			try
			{
				var objValue = Convert.ChangeType(propertyValue, returnType);
				return (TKey)objValue;
			}
			catch (InvalidCastException ex)
			{
				try
				{
					TypeConverter tc = TypeDescriptor.GetConverter(returnType);
					return (TKey)tc.ConvertFrom(propertyValue);
				}
				catch (Exception)
				{
					return defaultValue;
				}

			}
		}
		public static string JoinString<T>(this IEnumerable<T> sequence, string separator, Func<T, string> convertor)
		{
			StringBuilder seed = new StringBuilder();
			sequence.Aggregate(seed, (builder, item) =>
			{
				if (builder.Length > 0&&!string.IsNullOrWhiteSpace(separator))
				{
					builder.Append(separator);
				}
				builder.Append(convertor(item));
				return builder;
			});
			return seed.ToString();
		}

		public static string JoinStrings<T>(this IEnumerable<T> sequence, string separator)
		{
			return JoinString(sequence, separator, t => t.ToString());
		}
		/// <summary>
		/// Helper method to get the odata path for an arbitrary odata uri.
		/// </summary>
		/// <param name="request">The request instance in current context</param>
		/// <param name="uri">OData uri</param>
		/// <returns>The parsed odata path</returns>
		public static ODataPath CreateODataPath(this HttpRequestMessage request, Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}

			var newRequest = new HttpRequestMessage(HttpMethod.Get, uri);
			var route = request.GetRouteData().Route;

			var newRoute = new HttpRoute(
				route.RouteTemplate,
				new HttpRouteValueDictionary(route.Defaults),
				new HttpRouteValueDictionary(route.Constraints),
				new HttpRouteValueDictionary(route.DataTokens),
				route.Handler);
			var routeData = newRoute.GetRouteData(request.GetConfiguration().VirtualPathRoot, newRequest);
			if (routeData == null)
			{
				throw new InvalidOperationException("The link is not a valid odata link.");
			}

			return newRequest.ODataProperties().Path;
		}
		
		/// <summary>
		/// Helper method to get the key value from a uri.
		/// Usually used by $link action to extract the key value from the url in body.
		/// </summary>
		/// <typeparam name="TKey">The type of the key</typeparam>
		/// <param name="request">The request instance in current context</param>
		/// <param name="uri">OData uri that contains the key value</param>
		/// <returns>The key value</returns>
		public static TKey GetKeyValue<TKey>(this HttpRequestMessage request, Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}

			//get the odata path Ex: ~/entityset/key/$links/navigation
			var odataPath = request.CreateODataPath(uri);
			var keySegment = odataPath.Segments.OfType<KeyValuePathSegment>().LastOrDefault();
			if (keySegment == null)
			{
				throw new InvalidOperationException("The link does not contain a key.");
			}

			var value = ODataUriUtils.ConvertFromUriLiteral(keySegment.Value, ODataVersion.V4);
			var type = typeof(TKey);
			var obj = Convert.ChangeType(value, type);
			return (TKey)obj;
		}
		/// <summary>
		///   Retrive key property from OData $ref query
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="request"></param>
		/// <param name="uri"></param>
		/// <returns></returns>
		//public static TKey GetKeyFromUriV4<TKey>(this HttpRequestMessage request, Uri uri) {
		//    if (uri == null) {
		//        throw new ArgumentNullException(nameof(uri));
		//    }

		//    var urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);
		//    var pathHandler = (IODataPathHandler)request.GetRequestContainer().GetService(typeof(IODataPathHandler));

		//    string serviceRoot = urlHelper.CreateODataLink(
		//        request.ODataProperties().RouteName,
		//        pathHandler, new List<ODataPathSegment>());

		//    var odataPath = pathHandler.Parse(serviceRoot, uri.LocalPath, request.GetRequestContainer());
		//    var keySegment = odataPath.Segments.OfType<KeySegment>().FirstOrDefault();
		//    if (keySegment == null) {
		//        throw new InvalidOperationException("The link does not contain a key.");
		//    }

		//    var value = keySegment.Keys.FirstOrDefault().Value;
		//    return (TKey)value;
		//}
		public static TKey GetKeyFromUri<TKey>(this HttpRequestMessage request, Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}

			var urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);

			string serviceRoot = urlHelper.CreateODataLink(
				request.ODataProperties().RouteName,
				request.ODataProperties().PathHandler, new List<ODataPathSegment>());
			var odataPath = request.ODataProperties().PathHandler.Parse(
				request.ODataProperties().Model,
				serviceRoot, uri.LocalPath);

			var keySegment = odataPath.Segments.OfType<KeyValuePathSegment>().FirstOrDefault();
			if (keySegment == null)
			{
				throw new InvalidOperationException("The link does not contain a key.");
			}

			var value = ODataUriUtils.ConvertFromUriLiteral(keySegment.Value, ODataVersion.V4);
			var type = typeof(TKey);
			var obj = Convert.ChangeType(value, type);
			return (TKey)obj;
		}
		public static object GetPropertyValueFromModel(object instance, string propertyName)
		{
			var propertyInfo = instance.GetType().GetProperty(propertyName);
			if (propertyInfo == null)
			{
				throw new HttpException("Didn't find property with name:" + propertyName);
			}
			var propertyValue = propertyInfo.GetValue(instance, new object[] { });

			return propertyValue;
		}

		public static IHttpActionResult GetOkHttpActionResult(this ODataController controller, object propertyValue)
		{
			var okMethod = default(MethodInfo);
			var methods = controller.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
			foreach (var method in methods)
			{
				if (method.Name == "Ok" && method.GetParameters().Length == 1)
				{
					okMethod = method;
					break;
				}
			}

			okMethod = okMethod.MakeGenericMethod(propertyValue.GetType());
			var returnValue = okMethod.Invoke(controller, new[] { propertyValue });
			return (IHttpActionResult)returnValue;
		}

		public static string FileUploadFolder()
		{
			var configuredPath = ConfigurationManager.AppSettings["FileUploadFolderName"];
			if (string.IsNullOrWhiteSpace(configuredPath))
				configuredPath = "~/Files";
			return configuredPath;
		}
		public static string MapPathToServer(string path)
		{
			return HttpContext.Current.Server.MapPath(path);
		}
	}

	public static class ControllerExt
	{
		public static T GetClaimByKey<T>(this ApiController context, string key)
		{
			var type = typeof(T);
			var claim = new ClaimsPrincipal(context.RequestContext.Principal).Claims.FirstOrDefault(x => x.Type == key);
			if (claim == null) return (default(T));
			var obj = Convert.ChangeType(claim.Value, type);
			return (T)obj;
		}
		public static ResponseMessageResult ApiResponse(this ApiController context, HttpStatusCode code, string ReasonPhrase = "", HttpContent content = null)
		{
			var res = new HttpResponseMessage { StatusCode = code, ReasonPhrase = ReasonPhrase, Content = content };
			return new ResponseMessageResult(res);
		}

		public static T GetClaimFromOwinContext<T>(this OwinContext context, string key)
		{
			var type = typeof(T);
			var claim = new ClaimsPrincipal(context.Authentication.User).Claims.FirstOrDefault(x => x.Type == key);
			if (claim == null) return (default(T));
			var obj = Convert.ChangeType(claim.Value, type);
			return (T)obj;
		}
		
	}

}
