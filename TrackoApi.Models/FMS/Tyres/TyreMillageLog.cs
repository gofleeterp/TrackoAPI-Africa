using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS.Tyres
{
    [Table("tTyreMillageLog")]
    public class TyreMillageLog:AuditableEntity
    {
        public long TransactionId { get; set; }
        /// <summary>
        /// Gets or sets the source type identifier.
        /// </summary>
        /// <remarks>Constant TypeId is 117</remarks>
        /// <example>1483:Manual//1484:TripLog//1485:JobCard</example>
        /// <value>The source type identifier.</value>
        public long SourceTypeId { get; set; }
        [ForeignKey("SourceTypeId")]
        public virtual ConstantValue fk_SourceType { get; set; }

        public long TyreId { get; set; }
        [ForeignKey("TyreId")]
        public virtual TyreMaster fk_Tyre { get; set; }

        public int Life { get; set; }
        public long VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }
        [DataType(DataType.Date)]
        public DateTime OnDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime OutDate { get; set; }
        public decimal KMRun { get; set; }


    }
}
