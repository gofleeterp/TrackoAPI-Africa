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
    public class TyreMasterCoreLogic : IBaseLogic
    {
        //protected static TyreMasterCoreLogic _Instance;
        //public static TyreMasterCoreLogic Instance => _Instance ?? (_Instance = new TyreMasterCoreLogic());

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
            var tyre = entry.Entity as TyreMaster;
            if(tyre==null)return;
            if (!isPostLogicCall)
            {
                switch (tyre.ObjectState)
                {
                    case ObjectState.Modified:
                        try
                        {
                            if(entry.State!=EntityState.Modified)entry.State=EntityState.Modified;
                            var tyreno = entry.Property("TyreNo");
                            if (tyreno.CurrentValue != tyreno.OriginalValue)
                            {
                                if (string.IsNullOrWhiteSpace(tyre.TyreNo))
                                {
                                    throw new BusinessException(ErrorCode.GLB106, "CE:Tyre No can't be Empty or null");
                                }
                                var alltls = _db.Set<TyreLog>().Where(x => x.TyreId == tyre.Id).ToList();
                                foreach (var log in alltls)
                                {
                                    log.TyreNo = tyre.TyreNo;
                                    log.ObjectState = ObjectState.Modified;
                                    _db.Set<TyreLog>().AddOrUpdate(log);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            
                        }
                        
                        break;
                    case ObjectState.Deleted:
                        _db.Database.ExecuteSqlCommand("DELETE [dbo].[tTyreMillageLog] WHERE TyreId=@p0", tyre.Id);
                        break;
                }
            }
            
            
        }
        
        public bool SaveAfterPostLogic { get; private set; }
    }
}
