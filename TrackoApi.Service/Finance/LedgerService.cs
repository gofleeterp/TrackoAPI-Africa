using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ILedgerService : IService<Ledger>
    {
        IQueryable<Ledger> GetAllLedgerByGroupId(int goupId);
        IQueryable<Ledger> GetAllLedgerByGroupCode(string code);
        IQueryable<Ledger> GetLedgerByRoleId(long roleid);
        Task MapLedgerToDefaultRoleClass(long ledgerId, long? newRoleId, long? oldRoleId);

        Task MapLedgerToDefaultGroupClass(long ledgerId, long? newGroupId,long? oldGroupId);


    }
    public class LedgerService : Service<Ledger>, ILedgerService
    {
        private readonly IRepositoryAsync<Ledger> _repository;
        public LedgerService(IRepositoryAsync<Ledger> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<Ledger> GetAllLedgerByGroupId(int goupId)
        {
            return _repository.GetAllLedgerByGroupId(goupId);
        }

        public IQueryable<Ledger> GetAllLedgerByGroupCode(string code)
        {
            return _repository.GetAllLedgerByGroupCode(code);
        }

        public IQueryable<Ledger> GetLedgerByRoleId(long roleid)
        {
            return GetLedgerByRoleId(roleid);
        }

        public async Task MapLedgerToDefaultRoleClass(long ledgerId, long? newRoleId, long? oldRoleId)
        {
            await _repository.MapLedgerToDefaultRoleClass(ledgerId, newRoleId, oldRoleId);
        }

        public async Task MapLedgerToDefaultGroupClass(long ledgerId, long? newGroupId, long? oldGroupId)
        {
            await _repository.MapLedgerToDefaultGroupClass(ledgerId, newGroupId, oldGroupId);
        }
    }
}
