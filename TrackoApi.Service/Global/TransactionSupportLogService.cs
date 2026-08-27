using Service.Pattern;
using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;
using Repository.Pattern.Core.UnitOfWork;
using System.Data;

namespace TrackoApi.Service
{
    public interface ITransactionSupportLogService : IService<TransactionSupportLog>
    {
        IQueryable<TransactionSupportLog> GetAllTransactionSupportLogList(int id);
    }
    public class TransactionSupportLogService : Service<TransactionSupportLog>, ITransactionSupportLogService
    {
        private readonly IRepositoryAsync<TransactionSupportLog> _repository;
        public TransactionSupportLogService(IRepositoryAsync<TransactionSupportLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<TransactionSupportLog> GetAllTransactionSupportLogList(int brandid)
        {
            return _repository.GetAllTransactionSupportLogList(brandid);
        }
        //public void CreateUpdateTSL(TransactionSupportLog advance,long RecordId)
        //{
        //    if (advance.fk_Voucher == null)
        //    {
        //        if (advance.VoucherId > 0)
        //        {
        //            advance.fk_Voucher = _repository.GetRepository<Voucher>().Queryable().Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).FirstOrDefault();
        //        }
        //        if (advance.fk_Voucher == null)
        //        {
        //            advance.fk_Voucher = new Voucher();
        //        }

        //    }
        //    advance.fk_Voucher.IsCCRequired = true;
        //    advance.fk_Voucher.ConstCurTypeId = advance.ConstCurTypeId;
        //    advance.fk_Voucher.CurTypeId = advance.CurTypeId;
        //    advance.fk_Voucher.CurRate = advance.CurRate;

        //    advance.fk_Voucher.OfficeId = advance.OfficeId;
        //    advance.fk_Voucher.VoucherNo = advance.VoucherNo;
        //    advance.fk_Voucher.VoucherDate = advance.AdvanceDate;
        //    advance.fk_Voucher.VoucherDateTime = advance.AdvanceDate;
        //    advance.fk_Voucher.ObjectState = advance.fk_Voucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;
        //    advance.fk_Voucher.VoucherAmount = (advance.Amount * 1) + advance.fk_Voucher.Amount7;
        //    advance.fk_Voucher.VoucherTypeId = advance.AdvanceTypeId.GetValueOrDefault(0);
        //    advance.fk_Voucher.Account1Id = advance.DebitAccountId.GetValueOrDefault(0);
        //    advance.fk_Voucher.Account2Id = advance.CreditAccountId.GetValueOrDefault(0);
        //    advance.fk_Voucher.Account3Id = advance.IGSTAccountId;
        //    advance.fk_Voucher.Account4Id = advance.CGSTAccountId;
        //    advance.fk_Voucher.Account5Id = advance.SGSTAccountId;
        //    advance.fk_Voucher.Account6Id = advance.RoundUpAccountId;
        //    advance.fk_Voucher.Amount1 = advance.Amount * 1;
        //    advance.fk_Voucher.Amount2 = (advance.LoanAdjusted > 0 ? advance.PaidAmount : advance.BasicAmt > 0 ? advance.BasicAmt : advance.Amount) * -1;
        //    advance.fk_Voucher.Amount3 = advance.IGSTAmt;
        //    advance.fk_Voucher.Amount4 = advance.CGSTAmt;
        //    advance.fk_Voucher.Amount5 = advance.SGSTAmt;
        //    advance.fk_Voucher.Amount6 = advance.RoundUp;
        //    advance.fk_Voucher.UserRemark = advance.Remark;
        //    //TODO:Setup Account Narration from Template located with VoucherType
        //    advance.fk_Voucher.AccountingRemark = "";

        //    /*Currency*/
        //    advance.fk_Voucher.CurRate = ((advance.fk_Voucher.ConstCurTypeId == advance.fk_Voucher.CurTypeId) || advance.fk_Voucher.CurRate <= 0) ? 1 : advance.fk_Voucher.CurRate;

        //    if (advance.fk_Voucher.CurTypeId != advance.fk_Voucher.ConstCurTypeId & advance.fk_Voucher.CurTypeId.GetValueOrDefault() > 0 && advance.fk_Voucher.CurRate <= 0)
        //    {
        //        throw new BusinessException(ErrorCode.CUR100, "V1: Currency Rate need to be defined!!");
        //    }
        //}
    }
}
