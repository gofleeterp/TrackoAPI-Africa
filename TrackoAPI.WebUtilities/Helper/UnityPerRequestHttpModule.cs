using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace TrackoAPI.WebUtilities
{
    /// <summary>
    /// Implementation of the <see cref="T:System.Web.IHttpModule" /> interface that provides support for using the
    /// <see cref="T:TrackoAPI.WebUtilities.PerRequestLifetimeManager" /> lifetime manager, and enables it to
    /// dispose the instances after the HTTP request ends.
    /// </summary>
    public class UnityPerRequestHttpModule : IHttpModule
    {
        public UnityPerRequestHttpModule()
        {
            
        }
        private static readonly object ModuleKey = new object();

        internal static object GetValue(object lifetimeManagerKey)
        {
            Dictionary<object, object> dictionary = GetDictionary(HttpContext.Current);
            if (dictionary != null)
            {
                object obj = null;
                if (dictionary.TryGetValue(lifetimeManagerKey, out obj))
                    return obj;
            }
            return null;
        }

        internal static void SetValue(object lifetimeManagerKey, object value)
        {
            Dictionary<object, object> dictionary = GetDictionary(HttpContext.Current);
            if (dictionary == null)
            {
                dictionary = new Dictionary<object, object>();
                HttpContext.Current.Items[ModuleKey] = dictionary;
            }
            dictionary[lifetimeManagerKey] = value;
        }

        /// <summary>Disposes the resources used by this module.</summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication" /> that provides access to the methods, properties,
        /// and events common to all application objects within an ASP.NET application.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Validated with Guard class", MessageId = "0")]
        public void Init(HttpApplication context)
        {
            if(context==null)throw new ArgumentNullException(nameof(context));
            context.EndRequest += OnEndRequest;
        }

        private void OnEndRequest(object sender, EventArgs e)
        {
            Dictionary<object, object> dictionary = GetDictionary(((HttpApplication)sender).Context);
            if (dictionary == null)
                return;
            foreach (IDisposable disposable in dictionary.Values.OfType<IDisposable>())
                disposable.Dispose();
        }

        private static Dictionary<object, object> GetDictionary(HttpContext context)
        {
            if (context == null)
                throw new InvalidOperationException("ErrorHttpContextNotAvailable");
            return (Dictionary<object, object>)context.Items[ModuleKey];
        }
    }
}
