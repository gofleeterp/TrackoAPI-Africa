using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service
{
    public interface ICardMasterService : IService<CardMaster>
    {
       
    }
    public class CardMasterService : Service<CardMaster>, ICardMasterService
    {
        private readonly IRepositoryAsync<CardMaster> _repository;
        public CardMasterService(IRepositoryAsync<CardMaster> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
