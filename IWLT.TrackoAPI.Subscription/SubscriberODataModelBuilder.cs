using System;

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

using Tenant.Models;

namespace IWLT.TrackoAPI.Subscription.Models
{
    public static class ODataHelpers
    {
        public static IEdmModel GetEdmModel()
        {
            //app.UseODataBatching();
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<TenantApplicationMapping>("AppMappings").EntityType
                .Filter()
                .Count()
                .Expand()
                .OrderBy()
                .Page()
                .Select();
            return builder.GetEdmModel();
        }
    }
}
