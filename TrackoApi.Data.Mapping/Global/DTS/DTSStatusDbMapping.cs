using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.Global.DTS;

namespace TrackoApi.Data.Mapping.Global.DTS
{
    public class DTSStatusDbMapping : EntityTypeConfiguration<DTSStatus>
    {
        public DTSStatusDbMapping()
        {
            HasOptional(x => x.fk_NextStatus).WithMany().HasForeignKey(x => x.NextStatusId).WillCascadeOnDelete(false);
            //Required Columns
            HasOptional(x => x.fk_FixedCategory).WithMany().HasForeignKey(x => x.FixedCategoryId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Date).WithMany().HasForeignKey(x => x.DateId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Nature).WithMany().HasForeignKey(x => x.NatureId).WillCascadeOnDelete(false);

            //Optional Columns
            HasOptional(x => x.fk_ReportCategory).WithMany().HasForeignKey(x => x.ReportCategoryId).WillCascadeOnDelete(false);
            
            HasOptional(x => x.fk_Monitor).WithMany().HasForeignKey(x => x.MonitorId).WillCascadeOnDelete(false);
            
        }
    }
}
