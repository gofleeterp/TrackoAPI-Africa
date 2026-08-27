using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tenant.Models
{
    [Table("mFuelCompany")]
    public class FuelCompany
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None), MaxLength(50)]
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public virtual List<StateMaster> States { get; set; }
        public virtual List<IOCPump> Pumps { get; set; }
    }
    [Table("mStateMaster")]
    public class StateMaster
    {
        [Key]
        public long Id { get; set; }
        [Index("IDX_mStateMaster_Unique",IsUnique = true,Order = 1), MaxLength(50)]
        public string StateCode { get; set; }
        public string StateName { get; set; }
        [Index("IDX_mStateMaster_Unique", IsUnique = true, Order = 2),MaxLength(50)]
        public string CompanyCode { get; set; }
        [ForeignKey("CompanyCode")]
        public virtual FuelCompany Company { get; set; }
        public DateTime? LastRateUpdated { get; set; }
        public string LastError { get; set; }
        public DateTime? LastErrorTime { get; set; }
        public virtual List<IOCPump> Pumps { get; set; }
        public virtual List<HPCLTown> Towns { get; set; }
    }
    [Table("mPump")]
    public class IOCPump
    {
        public IOCPump()
        {
            RateLogs=new List<RateLog>();
        }
        [Key]
        public long PumpId { get; set; }
        [Index("IDX_mPump_Unique",IsUnique = true,Order = 1),MaxLength(500)]
        public string PumpName { get; set; }
        [Index("IDX_mPump_Unique1",IsUnique = true,Order = 1), MaxLength(50)]
        public string PumpCode { get; set; }
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
        public string Address { get; set; }
        public double PetrolPrice { get; set; } = 0;
        public double DieselPrice { get; set; } = 0;
        public string Owner { get; set; }
        public string OwnerPhone { get; set; }
        public string District { get; set; }
        public string AreaSalesOfficeContact { get; set; }
        public long StateId { get; set; }
        [Index("IDX_mPump_Unique", IsUnique = true, Order = 2), MaxLength(50)]
        [Index("IDX_mPump_Unique1", IsUnique = true, Order = 2)]
        public string CompanyCode { get; set; }
        [ForeignKey("StateId")]
        public virtual StateMaster State { get; set; }
        [ForeignKey("CompanyCode")]
        public virtual FuelCompany Comany { get; set; }
        public DateTime? LastRateUpdated { get; set; }
        public string LastError { get; set; }
        public DateTime? LastErrorTime { get; set; }
        public virtual List<RateLog> RateLogs { get; set; }
        public long? TownId { get; set; }
        [ForeignKey("TownId")]
        public virtual HPCLTown Town { get; set; }
    }
    [Table("mHPCLTown")]
    public class HPCLTown
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual StateMaster State { get; set; }
        public string TownCode { get; set; }
        public string TownName { get; set; }
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
        public double PetrolPrice { get; set; } = 0;
        public double DieselPrice { get; set; } = 0;
        public DateTime? LastRateUpdated { get; set; }
        public string LastError { get; set; }
        public DateTime? LastErrorTime { get; set; }
        public virtual List<RateLog> RateLogs { get; set; }
        public List<IOCPump> Pumps { get; set; }
    }
    [Table("tRateLog")]
    public class RateLog
    {
        [Key]
        public long Id { get; set; }
        public long? PumpId { get; set; }
        [ForeignKey("PumpId")]
        public virtual IOCPump IocPump { get; set; }
        public long? TownCode { get; set; }
        [ForeignKey("TownCode")]
        public virtual HPCLTown HPCLTown { get; set; }
        public DateTime LogDate { get; set; } = DateTime.Now;
        public double PetrolPrice { get; set; } = 0;
        public double DieselPrice { get; set; } = 0;
    }
    [Table("mTollPlaza")]
    public class TollPlaza
    {
        public string __type { get; set; }
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TollPlazaID { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int ComulativeRevenue { get; set; }
        public int Traffic { get; set; }
        public int TargetTrafic { get; set; }
        public int DesignCapacity { get; set; }
        public int CarRate_single { get; set; }
        public int CarRate_multi { get; set; }
        public int CarRate_mth { get; set; }
        public double LCVRate_single { get; set; }
        public int LCVRate_multi { get; set; }
        public int LCVRate_Mth { get; set; }
        public int BusRate_multi { get; set; }
        public int BusRate_Mth { get; set; }
        public int MultiAxleRate_single { get; set; }
        public int MultiAxleRate_multi { get; set; }
        public int MultiAxleRate_Mth { get; set; }
        public int HCM_EME_Single { get; set; }
        public string PlazaImage { get; set; }
        public string Location { get; set; }
        public int CapitalCost { get; set; }
        public string HtmlPopup { get; set; }
        public string TravelHTML { get; set; }
        public int InchargeContactDetail { get; set; }
        public string ProjectType { get; set; }
        public int FourToSixExel_Single { get; set; }
        public int SevenOrmoreExel_Single { get; set; }
        public string TollName { get; set; }
        public string SearchLoc { get; set; }
        public string Concession { get; set; }
    }

    public class RootObject
    {
        public List<TollPlaza> d { get; set; }
    }
}
