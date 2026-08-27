using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.AMS;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.AMS
{
    public class ContactBookDbMapping : EntityTypeConfiguration<Contact>
    {
        public ContactBookDbMapping()
        {
            HasRequired(x => x.fk_ContactType).WithMany().HasForeignKey(x => x.ContactTypeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_ContactNature).WithMany().HasForeignKey(x => x.ContactNatureId).WillCascadeOnDelete(false);
        }
    }
}
