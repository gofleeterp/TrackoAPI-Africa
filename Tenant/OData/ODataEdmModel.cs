using System;
using System.Web.OData.Builder;

using Tenant.Models;
using Tenant.Models.CRM;

using TrackoAPI.Models.Shared;

namespace Tenant.OData
{
    public class TenantConfigure
    {
        public static ODataConventionModelBuilder GetEdmModelBuilder(Action<ODataConventionModelBuilder> modelBuilder = null)
        {

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder
            {
                Namespace = "TenantManagement",
                ContainerName = "TenantManagementContext",
                DataServiceVersion = new Version(4, 0)

            };
            builder.EnumType<PurchaseType>();
            builder.EnumType<NotificationType>();
            builder.EnumType<ApplicationCategory>();
            builder.EnumType<LogType>();

            builder.EntitySet<NotificationPurchase>("NotificationPurchase");
            builder.EntitySet<NotificationLog>("NotificationLogs");
            builder.EntitySet<WebApiUsage>("WebApiUsage");
            builder.EntitySet<JsonGlobalLog>("JsonGlobalLog");
            builder.EntitySet<ThirdPartyToken>("ThirdPartyToken");
            builder.EntitySet<TenantMaster>("TenantMaster");
            builder.EntitySet<Application>("Application");
            builder.EntitySet<DatabaseBackupLog>("DatabaseBackupLog");
            builder.EntitySet<JobTrack>("JobTrack");
            builder.EntitySet<StateMaster>("StateMaster");
            builder.EntitySet<FuelCompany>("FuelCompany");
            builder.EntitySet<IOCPump>("IOCPump");
            builder.EntitySet<RateLog>("RateLog");
            builder.EntitySet<HPCLTown>("HPCLTown");
            builder.EntitySet<TollPlaza>("TollPlaza");
            builder.EntitySet<Subscriber>("Subscriber");
            builder.EntitySet<IntegrationEventMaster>("IntegrationEventMaster");
            builder.EntitySet<TenantApplicationMapping>("TenantApplicationMapping");
            builder.EntitySet<DPS>("DPS");
            builder.EntitySet<ReleaseNote>("ReleaseNote");
            builder.EntitySet<TenantConstantValue>("TenantConstantValue");
            builder.EntitySet<WorkItem>("WorkItem");
            builder.EntitySet<WorkItemComment>("WorkItemComment");
            builder.EntitySet<WorkItemLog>("WorkItemLog");
            builder.EntitySet<WorkItemReferenceLog>("WorkItemReferenceLog");
            builder.EntitySet<WorkItemStatusMap>("WorkItemStatusMap");
            builder.EntitySet<WorkDeliveryReport>("WorkDeliveryReport");

            modelBuilder?.Invoke(builder);
            return builder;
        }
    }
}
