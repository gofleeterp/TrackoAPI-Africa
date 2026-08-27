using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class EventStorageDbMapping: EntityTypeConfiguration<EventStorage>
    {
        public EventStorageDbMapping()
        {
            Ignore(x => x.EventData);
            Ignore(x => x.EventDataArray);
            Property(x => x._Properties).HasColumnName("Properties");
        }
    }
}