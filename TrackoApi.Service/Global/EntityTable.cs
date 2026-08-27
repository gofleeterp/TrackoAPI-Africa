using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Data;
using TrackoApi.Models.Validations;
using Unity;

namespace TrackoApi.Service.Global
{
    public class EntityTable<T>:IEntityTable<T> where T : class
    {
        private readonly ITrackoApiDbContext _ctx;
        //[InjectionConstructor]
        public EntityTable(ITrackoApiDbContext ctx)
        {
            _ctx = ctx;
        }

        public DbSet<T> Table => _ctx.Set<T>();
        public Database Database => _ctx.Database;
        public TResult GetBySqlQuery<TResult>(string query,params object[] parameters)
        {
            return _ctx.Database.SqlQuery<TResult>(query, parameters).FirstOrDefault();
        }
        public T GetBySqlQuery(string query, params object[] parameters)
        {
            return Table.SqlQuery(query, parameters).FirstOrDefault();
        }
        public IQueryable<T> Query()
        {
            return Table.AsQueryable<T>();
        }
    }
}
