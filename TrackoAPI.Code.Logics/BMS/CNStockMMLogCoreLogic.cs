using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Repository.Pattern.DataContext;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;

namespace TrackoAPI.Code.Logics.BMS
{
    public class CNStockMMLogCoreLogic:IBaseLogic
    {
        //protected static CNStockMMLogCoreLogic _Instance;
        //public static CNStockMMLogCoreLogic Instance => _Instance ?? (_Instance = new CNStockMMLogCoreLogic());

        protected IDataContextAsync _db;
        public IBaseLogic Bind(IDataContextAsync db)
        {
            _db = db;
            return this;
        }

        public void Execute(DbEntityEntry entry)
        {
            Execute(entry, false);
        }
        

        public bool SaveAfterPostLogic { get; private set; }

        public void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = true;
            var entity = entry.Entity as CNStockMMLog;
            if (!isPostLogicCall)
            {
                //Chain(entity);
                //UnChain(entity);
            }
        }

        

        private void Chain(CNStockMMLog entity)
        {
            if (entity?.ObjectState == ObjectState.Added && entity.RefStockId.GetValueOrDefault() > 0)
            {
                var previousLog = _db.Set<CNStockMMLog>().Find(entity.RefStockId);
                if (previousLog != null)
                {
                    previousLog.NextLogId = entity.Id;
                    previousLog.fk_NextLog = entity;
                    var pEntry = _db.Entry(previousLog);
                    if (pEntry.State == EntityState.Detached)
                    {
                        _db.Set<CNStockMMLog>().Attach(previousLog);
                        pEntry.State = EntityState.Modified;
                    }
                }
            }
        }

        private void UnChain(CNStockMMLog entity)
        {
            if (entity?.ObjectState == ObjectState.Deleted && entity.RefStockId.GetValueOrDefault() > 0)
            {
                var previousLog = _db.Set<CNStockMMLog>().Find(entity.RefStockId);
                if (previousLog != null)
                {
                    previousLog.NextLogId = null;
                    previousLog.fk_NextLog = null;
                    var pEntry = _db.Entry(previousLog);
                    if (pEntry.State == EntityState.Detached)
                    {
                        _db.Set<CNStockMMLog>().Attach(previousLog);
                        pEntry.State = EntityState.Modified;
                    }
                }
            }
        }
    }
}
