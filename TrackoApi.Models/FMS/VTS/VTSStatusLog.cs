using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Base.Attributes;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.DTS;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tVTSStatusLog")]
    public class VTSStatusLog : AuditableEntity,IValidatableObject
    {
        public VTSStatusLog() 
        {
            VTSStatusLogsub = new List<VTSStatusLogsub>();
        }   
        [Column("VehicleId"), ForeignKey("fk_Vehicle")]
        public long? VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }
        
        public virtual List<VTSStatusLogsub> VTSStatusLogsub { get; set; }

        [Column("DriverId"), ForeignKey("fk_Driver")]
        public long? DriverId { get; set; }
        public virtual DriverMaster fk_Driver { get; set; }
        public long DTSStatusId { get; set; }
        [ForeignKey("DTSStatusId")]
        public virtual DTSStatus fk_DTSStatus { get; set; }
        [Column("StartDate"),Required]
        public DateTime StartDate { get; set; }

        [Column("EndDate")]
        public DateTime? EndDate { get; set; }
        [Column("LastAckDate")]
        public DateTime? LastAckDate { get; set; }

        //[Column("CityId"), ForeignKey("fk_City"), Required]
        //public long CityId { get; set; }
        //public virtual VTSStatusDefinition fk_City { get; set; }
        public long? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public virtual CityMaster fk_Location { get; set; }
        [MaxLength(1000)]

        public string GPSLocation { get; set; }

        //[Column("SortingId"),SqlDefaultValue(DefaultValue ="0")]
        //public decimal? SortingId { get; set; }

        [Column("DelayMinutes")]
        public long? DelayMinutes { get; set; }

        [Column("ConsumedMinutes")]
        public long? ConsumedMinutes { get; set; }

        [Column("Remark")]
        [MaxLength(500)]
        public string Remark { get; set; }

        [Column("TriplogId"), ForeignKey("fk_Triplog")]
        public long? TriplogId { get; set; }
        public virtual VehicleMovementLog fk_Triplog { get; set; }

        //[Column("IsPrimary")]
        //public bool IsPrimary { get; set; }

        //[Column("GPSVendorId"), ForeignKey("fk_GPSVendor")]
        //public long? GPSVendorId { get; set; }
        //public virtual GenericMaster fk_GPSVendor { get; set; }
        [Column("GPSVendorId")]
        public long? GPSVendorId { get; set; }
        [ForeignKey("GPSVendorId")]
        public virtual Ledger fk_GPSVendor { get; set; }

        [Column("SupervisorId")]
        public long? SupervisorId { get; set; }
        [ForeignKey("SupervisorId")]
        public virtual GenericMaster fk_Supervisor { get; set; }

        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public virtual VTSStatusLog fk_NextLog { get; set; }
        public long? PreviousLogId { get; set; }
        [ForeignKey("PreviousLogId")]
        public virtual VTSStatusLog fk_PreviousLog { get; set; }
        public bool IsAuto { get; set; } = false;

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        [Timestamp]
        public byte[] TimeStamp { get; set; }

        [Column("HireVehicleId")]
        public long? HireVehicleId { get; set; }
        [ForeignKey("HireVehicleId")]
        public virtual HireVehicle fk_HireVehicle { get; set; }

        public VTSStatusLog Clone()
        {
            return (VTSStatusLog) this.MemberwiseClone();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate!=null&&EndDate < StartDate)
            {
                yield return new ValidationResult("Status End Date Should be Greater than or equal to Status Date");
            }

            if (LastAckDate == null||LastAckDate<StartDate)
            {
                LastAckDate = StartDate;
            }
        }
        [Column("DataProps")]
        public string DataProps { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(ExtraProperties)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(ExtraProperties)): _dt;
            get
            {
                try
                {
                    if (DataProps == "{}") DataProps = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>(DataProps ?? (DataProps = "[]")));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }

            }
            set
            {
                _dt = value;
                DataProps = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }
        }
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((DataProps ?? "{}") == "{}") DataProps = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((DataProps ?? (DataProps = "[]")));
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                DataProps = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                DataProps = "[]";
            }
        }
    }

    [Table("tVTSStatusLogSub")]
    public class VTSStatusLogsub : AuditableEntity
    {
        public long VTSLogId { get; set; }
        [ForeignKey("VTSLogId")]
        public virtual VTSStatusLog fk_VTSStatusLog { get; set; }

        public long DTSStatusId { get; set; }
        [ForeignKey("DTSStatusId")]
        public virtual DTSStatus fk_DTSStatus { get; set; }

        [Column("StartDate"), Required]
        public DateTime StartDate { get; set; }

        public long? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public virtual CityMaster fk_Location { get; set; }

        [Column("Remark")]
        [MaxLength(1500)]
        public string Remark { get; set; }
    }
}