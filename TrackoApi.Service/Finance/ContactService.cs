using System.Linq;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.AMS;
using TrackoApi.Models.BMS;
using TrackoAPI.Repository;
using TrackoAPI.Repository.Finance;

namespace TrackoApi.Service.Finance
{
    public interface IContactService : IService<Contact>
    {
        IQueryable<Contact> GetAllContactBookList(int contacttypeid);
    }
    public class ContactService : Service<Contact>, IContactService
    {
        private readonly IRepositoryAsync<Contact> _repository;
        public ContactService(IRepositoryAsync<Contact> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<Contact> GetAllContactBookList(int contacttypeid)
        {
            return _repository.GetAllContactBookList(contacttypeid);
        }
    }
}
