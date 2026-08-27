using Repository.Pattern.Core.Repositories;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;

namespace TrackoAPI.Repository
{
    public static class CNBillPaymentLogRepository
    {
        public static Task UpdateOnAccountBalanceAsync(this IRepositoryAsync<CNBillPaymentLog> repository, long? onAccountPaymentId)
        {
            
            if (onAccountPaymentId.GetValueOrDefault() == 0) return Task.CompletedTask;
            return repository.ExecuteSqlAsync(@"update b 
SET b.OnAccBalAmt=b.Amount-ISNULL(p.Amount,0) 
FROM tCNBillPaymentLog b 
LEFT JOIN(
	SELECT OnAccountRefId, Amount = SUM(OnAcAdjustedAmt) 
	FROM  tCNBillPaymentLog  
	WHERE OnAccountRefId IS NOT NULL GROUP BY OnAccountRefId 
) as p ON b.Id = p.OnAccountRefId where @p0 = b.Id AND b.TypeId in(1434,1443)

if(EXISTS(SELECT 1 FROM tCNBillPaymentLog b WHERE @p0 = b.Id AND b.OnAccBalAmt<0))
BEGIN
DECLARE @error nvarchar(max)='On Account Amount Adjustment should not be exceeded then On Account Amount. Hint:'+CAST(@p0 as nvarchar)
        RAISERROR(@error,11,0)
END", onAccountPaymentId);
        }
        public static Task UpdateBalanceAsync(this IRepositoryAsync<CNBillPaymentLog> repository, long? billLogId)
        {
            if (billLogId.GetValueOrDefault() == 0) return Task.CompletedTask;
            return repository.ExecuteSqlAsync(@"update b SET b.BalanceAmount=b.TotalBillAmount-ISNULL(p.Amount,0) FROM tCNBillLog b 
                LEFT JOIN(
	                SELECT BillLogId, Amount = SUM(Amount) 
	                FROM  tCNBillPaymentLog WHERE TypeId<>1433  GROUP BY BillLogId
	                ) as p ON b.Id = p.BillLogId where @p0 = b.Id
                    DECLARE @error nvarchar(max)
	                if(EXISTS(SELECT 1 FROM tCNBillLog b WHERE @p0 = b.Id and isnull(b.BalanceAmount,0)<0))
                    BEGIN
                    SET @error='Total Payment against Bill Item cannot exceed it''s Total Bill Item Amount. Hint:'+CAST(@p0 as nvarchar)
                        RAISERROR(@error,11,0)
                    END

	                if(EXISTS(SELECT 1 FROM tCNBillLog b WHERE @p0 = b.Id AND b.TotalBillAmount<ISNULL((SELECT SUM(p.Amount) FROM tCNBillPaymentLog p WHERE p.BillLogId=b.Id and p.TypeId=1433),0)))
                    BEGIN
                    SET @error='Total Deduction against Bill Item cannot exceed it''s Total Bill Item Amount. Hint:'+CAST(@p0 as nvarchar)
                        RAISERROR(@error,11,0)
                    END
", billLogId);
        }

        public static Task UpdateBalanceAsync(this IRepositoryAsync<CNBillPaymentLog> repository, long? billLogId, decimal currentpaymentlogamount, long paymentLogId)
        {
            if (billLogId.GetValueOrDefault() == 0) return Task.CompletedTask;
            return repository.ExecuteSqlAsync(@"update b SET b.BalanceAmount=b.TotalBillAmount-ISNULL(p.Amount,0)-ISNULL(@p1,0) FROM tCNBillLog b LEFT JOIN(SELECT BillLogId, Amount = SUM(Amount) FROM  tCNBillPaymentLog WHERE Id<>@p2 GROUP BY BillLogId) as p ON b.Id = p.BillLogId where @p0 = b.Id

    if(EXISTS(SELECT 1 FROM tCNBillLog b WHERE @p0 = b.Id AND b.TotalBillAmount<ISNULL((SELECT SUM(p.Amount) FROM tCNBillPaymentLog p WHERE p.BillLogId=b.Id),0)))
    BEGIN
    DECLARE @error nvarchar(max)='Total Payment against Bill Item cannot exceed it''s Total Bill Item Amount. Hint:'+CAST(@p0 as nvarchar)
            RAISERROR(@error,11,0)
    END
", billLogId, currentpaymentlogamount, paymentLogId);
        }
    }
}