using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class DriverMasterDbMapping : EntityTypeConfiguration<DriverMaster>
    {
        public DriverMasterDbMapping()
        {
            HasOptional(x => x.fk_BloodGroup).WithMany().HasForeignKey(x => x.BloodGroupId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DriverReligion).WithMany().HasForeignKey(x => x.ReligionId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_IssuePlace).WithMany().HasForeignKey(x => x.IssuingPlaceId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_RefI).WithMany().HasForeignKey(x => x.Ref1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_RefII).WithMany().HasForeignKey(x => x.Ref2Id).WillCascadeOnDelete(false);
            
            HasOptional(x => x.fk_FleetManager).WithMany().HasForeignKey(x => x.FleetManager1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Bank1).WithMany().HasForeignKey(x => x.Bank1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Bank2).WithMany().HasForeignKey(x => x.Bank2Id).WillCascadeOnDelete(false);
            
            HasOptional(x => x.fk_BankAc1).WithMany().HasForeignKey(x => x.BankAc1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_BankAc2).WithMany().HasForeignKey(x => x.BankAc2Id).WillCascadeOnDelete(false);



            HasKey(x => x.Id);
            HasRequired(x=>x.fk_Ledger).WithOptional().WillCascadeOnDelete(true);
            Ignore(x => x.AccountDetail);
            HasMany(x=>x.Relatives).WithRequired(x=>x.fk_Driver).HasForeignKey(x=>x.DriverId).WillCascadeOnDelete(true);
        }
    }
}
