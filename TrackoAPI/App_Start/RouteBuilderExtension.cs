using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Batch;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace TrackoAPI.App_Start
{
    public static class RouteBuilderExtension
    {
        public static void MapODataServiceRouteBase(
            this HttpConfiguration config,
            string routeName= "ODataRoute",
            string routePrefix= "odata",
            Action<ODataConventionModelBuilder> modelBuilder = null,ODataBatchHandler batchHandler=null)
        {
            var asm = Assembly.GetCallingAssembly();

            //TODO: add action for OData ModelBuilder
            config.MapODataServiceRoute(routeName, routePrefix, GetEdmModel(asm, modelBuilder));
        }

        private static IEdmModel GetEdmModel(Assembly asm, Action<ODataConventionModelBuilder> modelBuilder = null)
        {
            var builder = new ODataConventionModelBuilder();

            var classes = asm.DefinedTypes.Where(t => t.BaseType?.Name == typeof(ODataController).Name);

            foreach (var @class in classes)
            {
                var type = @class.BaseType?.GenericTypeArguments[0];
                builder.AddEntitySet(GetControllerNameFromType(@class.Name), builder.AddEntityType(type));
            }

            modelBuilder?.Invoke(builder);

            return builder.GetEdmModel();
        }

        private static string GetControllerNameFromType(string typeName)
        {
            return typeName.Replace("Controller", string.Empty);
        }
    }
}