using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class RouteMasterDbMapping : EntityTypeConfiguration<RouteMaster>
    {
        public RouteMasterDbMapping()
        {
            //HasOptional(x => x.fk_FromPlace).WithOptionalPrincipal().WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_ToPlace).WithOptionalPrincipal().WillCascadeOnDelete(false);

            HasRequired(x => x.fk_ToPlace).WithMany().HasForeignKey(x => x.ToPlaceId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_FromPlace).WithMany().HasForeignKey(x => x.FromPlaceId).WillCascadeOnDelete(false);
            HasMany(x=>x.WayPoints).WithRequired(x=>x.fk_Route).HasForeignKey(x=>x.RouteId).WillCascadeOnDelete(true);
            HasMany(x => x.Budgets).WithRequired(x => x.fk_Route).HasForeignKey(x => x.RouteId).WillCascadeOnDelete(true);
            //HasMany(x => x.ChildRoutes).WithMany().Map(x =>
            //{
            //    x.MapLeftKey("ParentRouteId");
            //    x.MapRightKey("ChileRouteId");
            //    x.ToTable("mChildRoutes");
            //});
            HasMany(x=>x.ChildRoutes).WithRequired(x=>x.fk_Parent).HasForeignKey(x=>x.ParentRouteId).WillCascadeOnDelete(true);
            HasMany(x => x.ParentRoutes).WithRequired(x => x.fk_Child).HasForeignKey(x => x.ChildRouteId).WillCascadeOnDelete(false);
            HasMany(x => x.AllowedVehicleTypes).WithRequired(x => x.fk_Route).HasForeignKey(x => x.RouteId).WillCascadeOnDelete(true);
            Ignore(x => x.Data);
        }
    }

    public class RouteWayPointDbMapping : EntityTypeConfiguration<RouteWayPoint>
    {
        public RouteWayPointDbMapping()
        {
            HasRequired(x=>x.fk_City).WithMany().HasForeignKey(x=>x.CityId).WillCascadeOnDelete(false);
        }
    }
}
