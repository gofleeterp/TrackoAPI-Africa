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
using TrackoApi.Models.FMS;

namespace TrackoAPI.Code.Logics.FMS
{
    public class BatteryMasterCoreLogic : IBaseLogic
    {
        //protected static BatteryMasterCoreLogic _Instance;
        //public static BatteryMasterCoreLogic Instance => _Instance ?? (_Instance = new BatteryMasterCoreLogic());

        protected IDataContextAsync _db;
        public IBaseLogic Bind(IDataContextAsync db)
        {
            _db = db;
            return this;
        }

        public void Execute(DbEntityEntry entry)
        {
            Execute(entry,false);
        }

        public void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = false;
            var battery = entry.Entity as BatteryMaster;
            if(battery==null)return;
            if (!isPostLogicCall)
            {
                switch (battery.ObjectState)
                {
                    case ObjectState.Modified:
                        try
                        {
                            if(entry.State!=EntityState.Modified)entry.State=EntityState.Modified;
                            var batteryno = entry.Property("BatterySerialNo");
                            if (batteryno.CurrentValue != batteryno.OriginalValue)
                            {
                                if (string.IsNullOrWhiteSpace(battery.BatterySerialNo))
                                {
                                    throw new BusinessException(ErrorCode.GLB106, "CE:battery No can't be Empty or null");
                                }
                                var alltls = _db.Set<BatteryLog>().Where(x => x.BatteryId == battery.Id).ToList();
                                foreach (var log in alltls)
                                {
                                    log.BatterySerialNo = battery.BatterySerialNo;
                                    log.ObjectState = ObjectState.Modified;
                                    _db.Set<BatteryLog>().AddOrUpdate(log);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            
                        }
                        
                        break;
                    case ObjectState.Deleted:
                        //_db.Database.ExecuteSqlCommand("DELETE [dbo].[tbatteryMillageLog] WHERE batteryId=@p0", battery.Id);
                        break;
                }
            }
            
            
        }
        
        public bool SaveAfterPostLogic { get; private set; }
    }
}
