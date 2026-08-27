using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ICNBillPaymentLogService : IService<CNBillPaymentLog>
    {
        Task UpdateBalanceAsync(long? billLogId);
        Task UpdateBalanceAsync(long? billLogId, decimal currentPaymentLogAmount,long currentBillLogId);
        Task UpdateOnAccountBalanceAsync(long? onAccountPaymentId);
    }
    public class CNBillPaymentLogService : Service<CNBillPaymentLog>, ICNBillPaymentLogService
    {
        private readonly IRepositoryAsync<CNBillPaymentLog> _repository;
        public CNBillPaymentLogService(IRepositoryAsync<CNBillPaymentLog> repository) : base(repository)
        {
            _repository = repository;
        }
        public Task UpdateOnAccountBalanceAsync(long? onAccountPaymentId)
        {
            if (onAccountPaymentId.GetValueOrDefault() == 0) return Task.CompletedTask;
            return _repository.UpdateOnAccountBalanceAsync(onAccountPaymentId);
        }
        public Task UpdateBalanceAsync(long? billLogId)
        {
            if (billLogId.GetValueOrDefault() == 0) return Task.CompletedTask;
            return _repository.UpdateBalanceAsync(billLogId);
        }
        public Task UpdateBalanceAsync(long? billLogId, decimal currentPaymentLogAmount, long currentBillLogId)
        {
            if (billLogId.GetValueOrDefault() == 0) return Task.CompletedTask;
            return _repository.UpdateBalanceAsync(billLogId, currentPaymentLogAmount, currentBillLogId);
        }
    }
}
