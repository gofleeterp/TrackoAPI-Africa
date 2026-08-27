using System.Data;

namespace IWLT.TrackoAPI.Subscription.Helpers
{
    public interface IDbTransactionFactory
    {
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel);
    }
}
