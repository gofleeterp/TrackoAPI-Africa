using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.Global;

namespace TrackoAPI.Repository
{
    public static class ObjectClassMappingRepository
    {
        public static IQueryable<ObjectClassMap> GetObjectsByCategoryId(this IRepository<ObjectClassMap> repository,
            long categoryId)
        {
            return repository.Queryable().Where(x => x.CategoryId == categoryId);
        }
        public static IQueryable<ObjectClassMap> GetObjectsByClassId(this IRepository<ObjectClassMap> repository,
            long classId)
        {
            return repository.Queryable().Where(x => x.ClassId == classId);
        }
        public static Tuple<long, List<long>> GetObjectsForReport(this IRepository<ObjectClassMap> repository,
            string classIds,long categoryId,string accountIds)
        {
            var data = repository.Queryable();
            var clsids = new List<long>();
            if (!string.IsNullOrWhiteSpace(classIds))
            {
                clsids=classIds.Split(',').Select(long.Parse).ToList();
            }
            var accountids= new List<long>();
            if (!string.IsNullOrWhiteSpace(accountIds))
            {
                accountids= accountIds.Split(',').Select(long.Parse).ToList();
            }
            if (accountids.Any()) data = data.Where(x => accountids.Contains(x.ObjectId));
            if(clsids.Any()) data = data.Where(x => clsids.Contains(x.ClassId));
            var objids= data.Where(x => x.CategoryId == categoryId).Select(x => x.ObjectId).ToList();
            var catRoleId =
                repository.GetRepository<ObjectCategory>()
                    .Queryable()
                    .Where(x => x.Id == categoryId)
                    .Select(x => x.RoleId)
                    .FirstOrDefault();
            return new Tuple<long, List<long>>(catRoleId,objids);
        }
        public static IQueryable<ObjectClassMap> GetObjectClassMap(this IRepository<ObjectClassMap> repository,
            string classIds, long categoryId, string accountIds)
        {
            var data = repository.Queryable();
            var clsids = new List<long>();
            if (!string.IsNullOrWhiteSpace(classIds))
            {
                clsids = classIds.Split(',').Select(long.Parse).ToList();
            }
            var accountids = new List<long>();
            if (!string.IsNullOrWhiteSpace(accountIds))
            {
                accountids = accountIds.Split(',').Select(long.Parse).ToList();
            }
            if (accountids.Any()) data = data.Where(x => accountids.Contains(x.ObjectId));
            if (clsids.Any()) data = data.Where(x => clsids.Contains(x.ClassId));
            return data.Where(x => x.CategoryId == categoryId);
        }
    }
}
