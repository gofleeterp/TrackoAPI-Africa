using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using System.Web.OData.Builder;

using Tenant.Models;
using Tenant.OData;

using TrackoApi.Core.Helpers;

using Unity;

namespace TrackoApi.OData
{
    public class GenericHttpControllerSelector : DefaultHttpControllerSelector
    {
        private readonly IDictionary<string, HttpControllerDescriptor> _controllerMappings;
        public GenericHttpControllerSelector(HttpConfiguration configuration,string routename,IEnumerable<EntitySetConfiguration> entitySets, IUnityContainer container):base(configuration)
        {
            _controllerMappings = GenerateMappings(configuration, routename, entitySets, container);
        }
        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            //Get request and route data
            if (request == null)
            {
                return base.SelectController(null);
            }
            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                return base.SelectController(request);
            }            
            var path = request.RequestUri.LocalPath.Split('/','(');
            string controllerName = GetControllerName(request);
            if (_controllerMappings.ContainsKey(path[2])) { return _controllerMappings[path[1]]; }
            return base.SelectController(request);
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            var basecontrollers = base.GetControllerMapping();
            return (IDictionary<string, HttpControllerDescriptor>)_controllerMappings.Union(basecontrollers);
        }
        private IDictionary<string, HttpControllerDescriptor> GenerateMappings(HttpConfiguration config,string routeName, IEnumerable<EntitySetConfiguration> entitySets, IUnityContainer container)
        {
            IDictionary<string, HttpControllerDescriptor> dictionary = new Dictionary<string, HttpControllerDescriptor>();
            if (routeName == "tenant")
            {
                Debugger.Break();
                if (!Helper.HostedOnPremise) {
                    foreach (EntitySetConfiguration set in entitySets)
                    {
                        try
                        {
                            var genericControllerDescription = new HttpControllerDescriptor(config, set.Name, typeof(TenantODataController<>).MakeGenericType(set.ClrType));
                            dictionary.Add(set.Name, genericControllerDescription);
                        }
                        catch(Exception ex)
                        {
                            //Ignore
#if DEBUG
                            Debug.WriteLine(ex.ToStringDemystified());
#endif
                        }
                    }
                }
                //config.Initializer(config);
            }
            //else
            //{
            //    foreach (EntitySetConfiguration set in entitySets)
            //    {
            //        var genericControllerDescription = new HttpControllerDescriptor(config, set.Name, typeof(BaseODataController<>).MakeGenericType(set.ClrType));
            //        dictionary.Add(set.Name, genericControllerDescription);

            //    }
            //}
            return dictionary;
        }
        private static T GetRouteVariable<T>(IHttpRouteData routeData, string name)
        {
            object result = null;
            if (routeData.Values.TryGetValue(name, out result))
            {
                return (T)result;
            }
            return default(T);
        }
    }
}
