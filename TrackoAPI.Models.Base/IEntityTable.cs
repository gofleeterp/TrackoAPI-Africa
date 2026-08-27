using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Models.Validations
{
    public interface IEntityTable<T> where T:class 
    {
        DbSet<T> Table { get; }
        Database Database { get; }
        TResult GetBySqlQuery<TResult>(string query, params object[] parameters);
        T GetBySqlQuery(string query, params object[] parameters);
    }
}
