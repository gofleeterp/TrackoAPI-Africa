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
    public class TrailorMappingCoreLogic : BaseLogic<VehicleTrailorMapping>
    {
        
        public override void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            
            switch (entry.Entity)
            {
                case VehicleTrailorMapping entity:
                    var vehicledb = _db.Set<VehicleMaster>();
                    var vehicle = vehicledb.Find(entity.VehicleId);
                    if (entity.fk_Trailor == null && entity.TrailorId > 0)
                    {
                        entity.fk_Trailor = _db.Set<VehicleMaster>().Find(entity.TrailorId);
                    }
                    switch (entity.ObjectState)
                    {
                        case ObjectState.Added:
                        case ObjectState.Modified:
                            if (_db.GetApiConfig<int>("IsAllowMultipleTrailorOnVehicle") == 0)
                            {
                                if (entity.OffDate == null && DbSet.AsNoTracking().Any(x =>
                                        x.OffDate == null && x.VehicleId == entity.VehicleId &&
                                        entity.Id != x.Id))
                                {
                                    throw new BusinessException(ErrorCode.GLB106,
                                        $"CORE:Trailor is Already Mapped to Vehicle No:{vehicle?.VehicleRegNo}");

                                }
                            }
                            if(vehicle==null)break;
                            if (entity.OffDate == null)
                            {
                                vehicle.TrailorNo = entity.fk_Trailor?.VehicleNo;
                                vehicle.TrailorId = entity.TrailorId;
                                vehicle.ObjectState = ObjectState.Modified;
                                vehicledb.AddOrUpdate(vehicle);
                            }
                            else if(vehicle.TrailorId==entity.TrailorId)
                            {
                                vehicle.TrailorNo = null;
                                vehicle.TrailorId = null;
                                vehicle.ObjectState = ObjectState.Modified;
                                vehicledb.AddOrUpdate(vehicle);
                            }
                            break;
                        case ObjectState.Deleted:
                            if (vehicle != null)
                            {
                                if (vehicle.TrailorId == entity.TrailorId||vehicle.TrailorNo==entity.fk_Trailor?.VehicleNo)
                                {
                                    vehicle.TrailorNo = null;
                                    vehicle.TrailorId = null;
                                    vehicle.ObjectState = ObjectState.Modified;
                                    vehicledb.AddOrUpdate(vehicle);
                                }
                            }                            
                            break;
                    }
                    break;
            }
        }
    }
}
