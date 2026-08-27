using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.DataContext;

namespace TrackoAPI.Code.Logics
{
    public interface IBaseLogic
    {
        bool SaveAfterPostLogic { get; }
        IBaseLogic Bind(IDataContextAsync db);
        void Execute(DbEntityEntry entry);
        void Execute(DbEntityEntry entry, bool isPostLogicCall);
        
    }

    public interface IBaseLogic<T>:IBaseLogic where T : class
    {
        DbSet<T> DbSet { get;} 
    }

    public abstract class BaseLogic<T> : IBaseLogic<T> where T : class
    {
        protected IDataContextAsync _db;

        public virtual IBaseLogic Bind(IDataContextAsync db)
        {
            _db = db;
            return this;
        }
        public virtual void Execute(DbEntityEntry entry)
        {
            Execute(entry, false);
            SaveAfterPostLogic = false;
        }
        public abstract void Execute(DbEntityEntry entry, bool isPostLogicCall);
        public  bool SaveAfterPostLogic { get; set; }
        public DbSet<T> DbSet => _db.Set<T>();
    }
}
