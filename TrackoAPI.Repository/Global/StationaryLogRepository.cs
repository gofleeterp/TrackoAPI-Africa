using System.Linq;
using AutoMapper;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoAPI.Repository
{
    public static class StationaryLogRepository
    {
        public static bool UpdatePageUse(this IRepository<StationeryBookLog> repository,
            long transactionId,long pageId)
        {
            var page = repository.Find(pageId);
            if (page == null) return false;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<StationeryBookLog, StationeryBookLogArchive>());
            var mapper = config.CreateMapper();
            var archive = mapper.Map<StationeryBookLogArchive>(page);
            archive.ObjectState = ObjectState.Added;
            page.ObjectState = ObjectState.Deleted;
            repository.GetRepository<StationeryBookLogArchive>().Insert(archive);
            return true;
        }

        public static bool RevokePageUse(this IRepository<StationeryBookLogArchive> repository,
            long transactionId, long pageId)
        {
            var archive = repository.Queryable().FirstOrDefault(x => x.Id == pageId);
            if (archive == null) return false;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<StationeryBookLogArchive, StationeryBookLog>());
            var mapper = config.CreateMapper();
            var page = mapper.Map<StationeryBookLog>(archive);
            page.ObjectState = ObjectState.Added;
            archive.ObjectState = ObjectState.Deleted;
            repository.Delete(archive);
            repository.GetRepository<StationeryBookLog>().Insert(page);
            return true;
        }
    }
}
