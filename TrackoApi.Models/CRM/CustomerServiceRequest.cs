using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.CRM
{
    [Table("tCustomerSR", Schema = "crm")]
    public class CustomerServiceRequest: AuditableEntity
    {
        public CustomerServiceRequest()
        {
            this.Services = new List<CustomerServiceRequestLog>();
            //this.DataInfo = new Dictionary<string, object>();
        }
        [MaxLength(200)]
        public string CompanyName { get; set; }
        [MaxLength(2000)]
        public string ComanyAddress { get; set; }
        [MaxLength(200)]
        public string ContactPerson { get; set; }
        [MaxLength(100)]
        public string EmailAddress { get; set; }
        [MaxLength(15)]
        public string PhoneNumber { get; set; }
        public virtual List<CustomerServiceRequestLog> Services { get; set; }
        public string _DataInfo { get; set; }
        [MaxLength(200)]
        public string Source { get; set; }
        //public IDictionary<string, object> DataInfo
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(_DataInfo)) return null;
        //        return JsonConvert.DeserializeObject<Dictionary<string, object>>(_DataInfo);
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            _DataInfo = JsonConvert.SerializeObject(value);
        //        }
        //    }
        //}
        public bool SendInvitation { get; set; }
        public string Subject { get; set; }
        public string MailBody { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RFQStatus RFQStatus { get; set; } = RFQStatus.EnquirySent;
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RFQStatus
    {
        EnquirySent=0,
        Active=0,
        Suspended=5,
        Completed=1,
        CompanyProfileShared=2,
        Approved=3,
        QuatationSend=4
    }
    [Table("tCustomerSRLog", Schema = "crm")]
    public class CustomerServiceRequestLog: AuditableEntity
    {
        public CustomerServiceRequestLog()
        {
            //this.DataInfo = new Dictionary<string, object>();
        }
        public long CSRId { get; set; }
        [ForeignKey("CSRId")]
        public virtual CustomerServiceRequest fk_CSR { get; set; }
        public long? ServiceId { get; set; }
        public string _DataInfo { get; set; }
        //public IDictionary<string,object> DataInfo { 
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(_DataInfo)) return null;
        //        return JsonConvert.DeserializeObject<Dictionary<string, object>>(_DataInfo);
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            _DataInfo = JsonConvert.SerializeObject(value);
        //        }
        //    }
        //}
    }
    [Table("mServiceMaster",Schema ="crm")]
    public class ServiceMaster : AuditableEntity
    {
        [MaxLength(200),Index("IX_ServiceMaster_ServiceUnique",IsUnique =true,Order =1 )]
        public string ServiceName { get; set; }
        /// <summary>
        /// e.g. Primary Transportation Details//Warehouse Space & Equipments Details
        /// </summary>
        [Index("IX_ServiceMaster_ServiceUnique", IsUnique = true, Order = 2)]
        public long CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual ConstantValue fk_Category { get; set; }
        /// <summary>
        /// Constant Values of Type 100
        /// </summary>
        public long? UnitTypeId { get; set; }
        [ForeignKey("UnitTypeId")]
        public virtual ConstantValue fk_UnitType { get; set; }
        /// <summary>
        /// Qty//Weight//Hour//Dimension//Per Day
        /// </summary>
        public long? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual UnitMaster fk_Unit { get; set; }
        public string _DataInfo { get; set; }
        public IDictionary<string, object> DataInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_DataInfo)) return null;
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(_DataInfo);
            }
            set
            {
                if (value != null)
                {
                    _DataInfo = JsonConvert.SerializeObject(value);
                }
            }
        }
        /*
         {
        }
         */
        //DataSource1Id=VehicleType/Brand/Type
        //Caption:
        //Paramter1 ='1,2,3,4'

        //DataSource2Id=null
        //Paramter2 = '2,5,6'


        //DataSource3Id=null
        //Paramter1 = '1,2,3,4'
    }
    [Table("mServiceUnit", Schema = "crm")]
    public class ServiceUnit : AuditableEntity
    {
        [MaxLength(200),Index("IX_ServiceUnit_Unique",IsUnique =true,Order =1)]
        public string UnitName { get; set; }
        /// <summary>
        /// eg. VehicleType 1 or TripType 2 and so on
        /// </summary>
        [Index("IX_ServiceUnit_Unique", IsUnique = true, Order = 2)]
        public long? DataSourceId { get; set; }
        [ForeignKey("DataSourceId")]
        public virtual ConstantValue fk_DataSource { get; set; }        
    }
}
