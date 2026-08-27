using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mVehicleConfigLog")]
    public class VehicleConfigurationLog : AuditableEntity
    {
        [Column("TypeId"), Index("IDX_mVehicleConfigLog_Unique", 1, IsUnique = true)]
        public long TypeId { get; set; }/*VehicleConfiguration//IncentiveConfiguration*/
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique", 2, IsUnique = true)]
        public long? SubTypeId { get; set; }/*Incentive Type*/
        [ForeignKey("SubTypeId")]
        public virtual ConstantValue fk_SubType { get; set; }

        [Required, Index("IDX_mVehicleConfigLog_Unique", 3, IsUnique = true)]
        public DateTime EffectiveDate { get; set; }
        [Index("IDX_mVehicleConfigLog_Unique", 4, IsUnique = true)]
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique", 5, IsUnique = true)]
        public long? VehicleTypeId { get; set; }
        [ForeignKey("VehicleTypeId")]
        public virtual GenericMaster fk_VehicleType { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique",6, IsUnique = true)]
        public long? RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual RouteMaster fk_Route { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique", 14, IsUnique = true)]
        public long? SecondaryRouteId { get; set; }
        [ForeignKey("SecondaryRouteId")]
        public virtual RouteMaster fk_SecondaryRoute { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique", 7, IsUnique = true)]
        public long? ZoneId { get; set; }
        [ForeignKey("ZoneId")]
        public virtual GenericMaster fk_Zone { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique", 8, IsUnique = true)]
        public long? StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual GenericMaster fk_State { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique",9, IsUnique = true)]
        public long? PartyId { get; set; }
        [ForeignKey("PartyId")]
        public virtual Ledger fk_Party { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique",10, IsUnique = true)]
        [Column("MaterialId"), ForeignKey("fk_Material")]
        public long? MaterialId { get; set; } = null;
        public virtual MaterialMaster fk_Material { get; set; }
        [Index("IDX_mVehicleConfigLog_Unique", 16, IsUnique = true)]
        public long? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual CityMaster fk_City { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique", 11, IsUnique = true)]
        [Column("TripNatureId"), ForeignKey("fk_TripNature")]
        public long? TripNatureId { get; set; } //Loaded//Empty//ORM       
        /// <summary>
        /// Gets or sets the FK_ trip nature.
        /// Constant TypeId 72
        /// </summary>
        /// <value>The FK_ trip nature.</value>
        public virtual ConstantValue fk_TripNature { get; set; }


        /// <summary>
        /// Could be any thing from generic master
        /// e.g. [TripMode]//Express/Highway/Normal or eny other Generic Master 
        /// </summary>
        /// 
        [Index("IDX_mVehicleConfigLog_Unique", 18, IsUnique = true)]
        [Column("GenericRef1")]
        public long? GenericRef1Id { get; set; }
        [ForeignKey("GenericRef1Id")]
        public virtual GenericMaster fk_GenericRef1 { get; set; }
        /// <summary>
        /// Could be any thing from generic master
        /// e.g. [TripMode]//Express/Highway/Normal or eny other Generic Master
        /// </summary>
        /// 
        [Index("IDX_mVehicleConfigLog_Unique", 19, IsUnique = true)]
        [Column("GenericRef2")]
        public long? GenericRef2Id { get; set; }
        [ForeignKey("GenericRef2Id")]
        public virtual GenericMaster fk_GenericRef2 { get; set; }
        [Index("IDX_mVehicleConfigLog_Unique", 12, IsUnique = true)]
        public decimal LowerRange { get; set; } = 0;
        [Index("IDX_mVehicleConfigLog_Unique", 13, IsUnique = true)]
        public decimal UpperRange { get; set; } = 0;
        
        public decimal Value1 { get; set; } = 0;
        public decimal Value2 { get; set; } = 0;
        public decimal Value3 { get; set; } = 0;
        public decimal Value4 { get; set; } = 0;
        public decimal Value5 { get; set; } = 0;
        public decimal Value6 { get; set; } = 0;
        public decimal Value7 { get; set; } = 0;
        public decimal Value8 { get; set; } = 0;
        public decimal Value9 { get; set; } = 0;
        public decimal Value10 { get; set; } = 0;
        public bool Checked1 { get; set; } = false;
        public bool Checked2 { get; set; } = false;
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;
        public long? ScriptId { get; set; }
        [ForeignKey("ScriptId")]
        public virtual ApiWorkFlowScript fk_Script { get; set; }

        //public decimal PerDayKmLoaded { get; set; } = 0;
        //public decimal PerDayKmEmpty { get; set; } = 0;

        //public decimal FuelAvgLoaded { get; set; } = 0;
        //public decimal FuelAvgEmpty { get; set; } = 0;

        //public decimal IncentiveValue { get; set; } = 0;

        public long? ViewId { get; set; }

        public long? ObjectClassId { get; set; }
        [ForeignKey("ObjectClassId")]
        public virtual ObjectClass fk_ObjectClass { get; set; }
        
        [Index("IDX_mVehicleConfigLog_Unique", 15, IsUnique = true)]
        [Column("ExpenseId")]
        public long? ExpenseId { get; set; } = null;
        [ForeignKey("ExpenseId")]
        public virtual ExpenseMaster fk_Expense { get; set; }

        [Index("IDX_mVehicleConfigLog_Unique", 17, IsUnique = true)]
        public long? SpareId { get; set; }
        [ForeignKey("SpareId")]
        public virtual SpareMaster fk_Spare { get; set; }


        [Index("IDX_mVehicleConfigLog_Unique", 20, IsUnique = true)]
        [Column("AccountGroupId")]
        public long? AccountGroupId { get; set; } = null;
        [ForeignKey("AccountGroupId")]
        public virtual AccountGroup fk_AccountGroup { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
