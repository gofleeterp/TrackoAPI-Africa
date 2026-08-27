using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.AMS;
using TrackoApi.Models.BMS;

namespace TrackoAPI.Repository.Finance
{
   public static class ContactRepository
    {
        public static IQueryable<Contact> GetAllContactBookList(this IRepository<Contact> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
