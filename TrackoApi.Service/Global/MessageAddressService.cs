using System.Linq;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Global.CronJobs;

namespace TrackoApi.Service
{
    public interface IMessageAddressService : IService<MessageAddress>
    {
        IQueryable<MessageAddress> GetAllMessageAddressList(AddressType type);
    }
    public class MessageAddressService : Service<MessageAddress>, IMessageAddressService
    {
        private readonly IRepositoryAsync<MessageAddress> _repository;
        public MessageAddressService(IRepositoryAsync<MessageAddress> repository) : base(repository)
        {
            _repository = repository;
        }


        public IQueryable<MessageAddress> GetAllMessageAddressList(AddressType type)
        {
            return _repository.Queryable().Where(x => x.AddressType == type);
        }
    }
}
