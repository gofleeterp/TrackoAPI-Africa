using System.Data.Entity.ModelConfiguration;
using System.Runtime.InteropServices;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class SpareLogDbMapping : EntityTypeConfiguration<SpareLog>
    {
        public SpareLogDbMapping()
        {
            HasOptional(x => x.fk_TSL).WithMany().HasForeignKey(x => x.TSLId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_Voucher).WithMany().HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Spare).WithMany().HasForeignKey(x => x.SparePartId).WillCascadeOnDelete(false);
            HasOptional(x => x.ExtraInfo).WithMany().HasForeignKey(x => x.ExtraInfoId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_PurchaseOrder).WithMany().HasForeignKey(x => x.PurchaseOrderId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_JobCard).WithMany().HasForeignKey(x => x.JobCardId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_Reference).WithMany().HasForeignKey(x => x.ReferenceId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Reference).WithMany(x => x.IssuedLogs).HasForeignKey(x => x.ReferenceId).WillCascadeOnDelete(false);
            
            HasOptional(x => x.fk_VoucherType).WithMany().HasForeignKey(x => x.VoucherTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Unit).WithMany().HasForeignKey(x => x.UnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HireVehicle).WithMany().HasForeignKey(x => x.HireVehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_UnitType).WithMany().HasForeignKey(x => x.UnitTypeId).WillCascadeOnDelete(false);
            
            Ignore(x => x.Data);
            
        }
    }

    public class SpareExtraInfoDbMapping : EntityTypeConfiguration<SpareLogExtraInfo>
    {
        public SpareExtraInfoDbMapping()
        {
            //HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Voucher).WithMany().HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ORM).WithMany().HasForeignKey(x => x.OrmId).WillCascadeOnDelete(false);
            HasMany(x => x.SpareLogs).WithRequired(x => x.ExtraInfo).HasForeignKey(x => x.ExtraInfoId).WillCascadeOnDelete(true);
            HasMany(x => x.BillSpareLogs).WithOptional(x => x.fk_Bill).HasForeignKey(x => x.BillExtraInfoId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_RelatedVoucher).WithMany().HasForeignKey(x => x.RelatedVoucherId).WillCascadeOnDelete(false);
            HasKey(x => x.Id);
            HasOptional(x => x.fk_IGSTAC).WithMany().HasForeignKey(x => x.IGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SGSTAC).WithMany().HasForeignKey(x => x.SGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CGSTAC).WithMany().HasForeignKey(x => x.CGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TDSAccount).WithMany().HasForeignKey(x => x.TDSAccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TDSVoucher).WithMany().HasForeignKey(x => x.TDSVoucherId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HireVehicle).WithMany().HasForeignKey(x => x.HireVehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PartyGSTOffice).WithMany().HasForeignKey(x => x.PartyGSTOfficeId).WillCascadeOnDelete(false);
            
            Ignore(x => x.Data);
        }
    }

    public class RepairLabourLogDbMapping : EntityTypeConfiguration<RepairLabourLog>
    {
        public RepairLabourLogDbMapping()
        {
            HasOptional(x => x.fk_TSL).WithMany().HasForeignKey(x => x.TSLId).WillCascadeOnDelete(false);

            HasOptional(x => x.ExtraInfo).WithMany().HasForeignKey(x => x.ExtraInfoId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_Voucher).WithMany().HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_Labor).WithMany().HasForeignKey(x => x.LaborId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_POLog).WithMany().HasForeignKey(x => x.POLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HireVehicle).WithMany().HasForeignKey(x => x.HireVehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_JobCard).WithMany().HasForeignKey(x => x.JobCardId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);
            Ignore(x => x.Data);
        }
    }
}
