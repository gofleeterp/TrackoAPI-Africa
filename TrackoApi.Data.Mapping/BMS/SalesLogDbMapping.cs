using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    internal class SalesLogDbMapping : EntityTypeConfiguration<SalesLog>
    {
        public SalesLogDbMapping()
        {
            //Property(x => x.NetFreight).HasPrecision(18, 5);
            HasOptional(x => x.fk_CN).WithMany().HasForeignKey(x => x.CNId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TripLog).WithMany().HasForeignKey(x => x.TripLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ChallanCN).WithMany().HasForeignKey(x => x.ChallanCNId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Route).WithMany().HasForeignKey(x => x.RouteId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_BillingParty).WithMany().HasForeignKey(x => x.BillingPartyId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_BillOffice).WithMany().HasForeignKey(x => x.BillingOfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SalesOffice).WithMany().HasForeignKey(x => x.SalesOfficeId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_ActualWeightUnit).WithMany().HasForeignKey(x => x.ActualWeightUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ChargeWeightUnit).WithMany().HasForeignKey(x => x.ChargeWeightUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ChargeQtyUnit).WithMany().HasForeignKey(x => x.ChargeQtyUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ActualQtyUnit).WithMany().HasForeignKey(x => x.ActualQtyUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_IGSTAC).WithMany().HasForeignKey(x => x.IGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SGSTAC).WithMany().HasForeignKey(x => x.SGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CGSTAC).WithMany().HasForeignKey(x => x.CGSTACId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_GSTPaidBy).WithMany().HasForeignKey(x => x.GSTPaidById).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GSTServiceType).WithMany().HasForeignKey(x => x.GSTServiceTypeId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_DeliveryType).WithMany().HasForeignKey(x => x.DeliveryTypeId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_Bill).WithMany().HasForeignKey(x => x.BillId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Rate).WithMany().HasForeignKey(x => x.RateId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_RateChart).WithMany().HasForeignKey(x => x.RateChartId).WillCascadeOnDelete(false);
        }
    }
}
