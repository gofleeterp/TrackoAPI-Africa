using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.DataContext;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;

namespace TrackoAPI.Code.Logics.FMS
{
    public class CardCoreLogic : BaseLogic<VehicleCardMapping>
    {
        //private static CardCoreLogic _sInstance;
        //public static CardCoreLogic Instance => _sInstance ?? (_sInstance = Activator.CreateInstance<CardCoreLogic>());
        //protected IDataContextAsync _db;
        //public override IBaseLogic Bind(IDataContextAsync db)
        //{
        //    _db = db;
        //    return this;
        //}

        //public override void Execute(DbEntityEntry entry)
        //{
        //    Execute(entry, false);
        //}

        public override void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            
            switch (entry.Entity)
            {
                case VehicleCardMapping entity:
                    var vehicledb = _db.Set<VehicleMaster>();
                    var vehicle = vehicledb.Find(entity.VehicleId);
                    if (entity.fk_Card == null && entity.CardId > 0)
                    {
                        entity.fk_Card = _db.Set<CardMaster>().Find(entity.CardId);
                    }
                    switch (entity.ObjectState)
                    {
                        case ObjectState.Added:
                        case ObjectState.Modified:
                            //if()
                            if (entity.OffDate == null && DbSet.AsNoTracking().Any(x =>
                                    x.OffDate == null && x.VehicleId == entity.VehicleId &&
                                    x.CardTypeId == entity.CardTypeId && entity.Id != x.Id && x.OnDate >= entity.OnDate && x.OffDate<=entity.OffDate ))
                            {
                                throw new BusinessException(ErrorCode.GLB106,
                                    $"CORE:Card is Already Mapped to Vehicle No:{vehicle?.VehicleRegNo}");

                            }                          
                            if(vehicle==null)break;
                            if (entity.OffDate == null)
                            {
                                switch (entity.CardTypeId)
                                {
                                    case 1635:/*Fleet Card*/
                                        vehicle.FuelCardNo = entity.fk_Card?.CardNo;
                                        vehicle.FuelCardAcNo= entity.fk_Card?.AccountNo;
                                        vehicle.ObjectState = ObjectState.Modified;
                                        vehicledb.AddOrUpdate(vehicle);
                                        break;
                                    case 1636:/*ATM Card*/
                                        vehicle.DebitCardNo = entity.fk_Card?.CardNo;
                                        vehicle.DebitCardAcNo = entity.fk_Card?.AccountNo;
                                        vehicle.ObjectState = ObjectState.Modified;
                                        vehicledb.AddOrUpdate(vehicle);
                                        break;
                                    case 1656:/*FastTag*/
                                        vehicle.FastTagNo = entity.fk_Card?.CardNo;
                                        vehicle.FastTagAcNo = entity.fk_Card?.AccountNo;
                                        vehicle.ObjectState = ObjectState.Modified;
                                        vehicledb.AddOrUpdate(vehicle);
                                        break;
                                }
                            }

                            if (entity.OffDate != null)
                            {
                                switch (entity.CardTypeId)
                                {
                                    case 1635:/*Fleet Card*/
                                        if (vehicle.FuelCardNo == entity.fk_Card?.CardNo || !entity.IsHotlisted)
                                        {
                                            vehicle.FuelCardNo = null;
                                            vehicle.FuelCardAcNo = null;
                                            vehicle.ObjectState = ObjectState.Modified;
                                            vehicledb.AddOrUpdate(vehicle);
                                        }
                                        
                                        break;
                                    case 1636:/*ATM Card*/
                                        if (vehicle.DebitCardNo == entity.fk_Card?.CardNo || !entity.IsHotlisted)
                                        {
                                            vehicle.DebitCardNo = null;
                                            vehicle.DebitCardAcNo = null;
                                            vehicle.ObjectState = ObjectState.Modified;
                                            vehicledb.AddOrUpdate(vehicle);
                                        }

                                        break;
                                    case 1656:/*FastTag*/
                                        if (vehicle.FastTagNo == entity.fk_Card?.CardNo|| !entity.IsHotlisted)
                                        {
                                            vehicle.FastTagNo = null;
                                            vehicle.FastTagAcNo = null;
                                            vehicle.ObjectState = ObjectState.Modified;
                                            vehicledb.AddOrUpdate(vehicle);
                                        }

                                        break;
                                }
                            }
                            break;
                        case ObjectState.Deleted:
                            if (vehicle != null)
                            {
                                var previouscard = DbSet.Include(x=>x.fk_Card).FirstOrDefault(x =>
                                    x.CardTypeId == entity.CardTypeId && x.VehicleId == entity.VehicleId &&
                                    x.OnDate <= entity.OnDate && x.Id != entity.Id&&!x.IsHotlisted);
                                switch (entity.CardTypeId)
                                {
                                    case 1635:/*Fleet Card*/
                                        vehicle.FuelCardNo = previouscard?.fk_Card?.CardNo;
                                        vehicle.FuelCardAcNo = previouscard?.fk_Card?.AccountNo;
                                        vehicle.ObjectState = ObjectState.Modified;
                                        vehicledb.AddOrUpdate(vehicle);
                                        break;
                                    case 1636:/*ATM Card*/
                                        vehicle.DebitCardNo = previouscard?.fk_Card?.CardNo;
                                        vehicle.DebitCardAcNo = previouscard?.fk_Card?.AccountNo;
                                        vehicle.ObjectState = ObjectState.Modified;
                                        vehicledb.AddOrUpdate(vehicle);
                                        break;
                                    case 1656:/*FastTag*/
                                        vehicle.FastTagNo = previouscard?.fk_Card?.CardNo;
                                        vehicle.FastTagAcNo = previouscard?.fk_Card?.AccountNo;
                                        vehicle.ObjectState = ObjectState.Modified;
                                        vehicledb.AddOrUpdate(vehicle);
                                        break;
                                }

                                if (previouscard != null)
                                {
                                    previouscard.ObjectState = ObjectState.Modified;
                                    previouscard.OffDate = null;
                                }
                            }
                            
                            break;
                    }
                    break;
            }
        }

        //public override bool SaveAfterPostLogic { get; set; }
        //public override DbSet<VehicleCardMapping> DbSet => _db.Set<VehicleCardMapping>();
    }
}
