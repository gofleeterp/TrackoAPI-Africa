using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Unity;

namespace TrackoAPI.WebUtilities.Filters
{
    public class WebApiUnityActionFilterProvider : ActionDescriptorFilterProvider, IFilterProvider
    {
        private readonly IUnityContainer container;

        public WebApiUnityActionFilterProvider(IUnityContainer container)
        {
            this.container = container;
        }

        public new IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            //if (Debugger.IsAttached)
            //{
            //    Debugger.Break();
            //}
            var filters = base.GetFilters(configuration, actionDescriptor);
            var filterInfoList = new List<FilterInfo>();

            foreach (var filter in filters)
            {
                container.BuildUp(filter.Instance.GetType(), filter.Instance);
            }

            return filters;
        }
        
        public static void RegisterFilterProviders(HttpConfiguration config)
        {
            //if (Debugger.IsAttached)
            //{
            //    Debugger.Break();
            //}
            // Add Unity filters provider
            var providers = config.Services.GetFilterProviders().ToList();            
            var defaultprovider = providers.First(p => p is ActionDescriptorFilterProvider);
            config.Services.Remove(typeof(System.Web.Http.Filters.IFilterProvider), defaultprovider);
            
            config.Services.Add(typeof(System.Web.Http.Filters.IFilterProvider), new WebApiUnityActionFilterProvider(Unity.Config.UnityCore.Container));

        }
    }    
}
