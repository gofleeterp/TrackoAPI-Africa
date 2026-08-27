using AutoMapper;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using Service.Pattern;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoAPI.Code.Logics.BMS;
using TrackoAPI.ViewModels.BMS;

namespace TrackoApi.Service
{
    public interface ICNBillPaymentService : IService<CNBillPayment>
    {
        Task PreparePaymentVoucherAsync(CNBillPayment payment, IUnitOfWorkAsync uow);
    }
    public class CNBillPaymentService : Service<CNBillPayment>, ICNBillPaymentService
    {
        private readonly IRepositoryAsync<CNBillPayment> _repository;
        private ITripAdvanceLogService _advRepo;
        public CNBillPaymentService(IRepositoryAsync<CNBillPayment> repository, ITripAdvanceLogService advanceService) : base(repository)
        {
            _repository = repository;
            _advRepo = advanceService;
        }

        public override CNBillPayment Insert(CNBillPayment entity)
        {
            return base.Insert(entity);
        }

        public override void Update(CNBillPayment entity)
        {
            base.Update(entity);
        }

        public async Task PreparePaymentVoucherAsync(CNBillPayment payment,IUnitOfWorkAsync uow)
        {
            VoucherDetail vd1 = null;
            List<long> accountrefs = new List<long>();
            payment.ConstCurTypeId = Helper.ConstCurTypeId;
            if (payment.GenerateVoucherOnServer)
            {
                var vrepo = uow.RepositoryAsync<VoucherDetail>();
                var vdrrepo = uow.RepositoryAsync<VoucherDetailReference>();
                if (payment.VoucherId > 0)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tCNBillPaymentLog] SET VDRId=NULL WHERE PaymentId={payment.Id}");
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVoucherVD] WHERE VoucherId={payment.VoucherId}");
                }
                //var vds = payment.VoucherId>0? await vrepo.Queryable().Where(x => x.VoucherId == payment.VoucherId).ToListAsync():new List<VoucherDetail>();
                //foreach (var vd in vds)
                //{
                //    vd.ObjectState = ObjectState.Deleted;
                //    vrepo.Delete(vd);
                //}
                var narration =
                    $"Freight Advance Payment Received  dt: {payment.DocumentDate.Date:dd-MMM-yyyy} of Rs.{payment.AdviceAmount} User Remark:{payment.Remark}";
                // 
                if (payment.VoucherId > 0 && payment.fk_Voucher == null)
                {
                    payment.fk_Voucher = vrepo.GetRepository<Voucher>().Find(payment.VoucherId);
                }
                if (payment.fk_Voucher == null)
                {
                    payment.fk_Voucher = new Voucher();
                }
                var voucher = payment.fk_Voucher;
                voucher.ConstCurTypeId = payment.ConstCurTypeId;
                voucher.CurTypeId = payment.CurTypeId;
                voucher.CurRate = payment.CurRate;
                voucher.Id = payment.VoucherId.GetValueOrDefault();
                voucher.OfficeId = payment.OfficeId;
                voucher.VoucherNo = payment.DocumentNo;
                voucher.VoucherDate = payment.DocumentDate.Date;
                voucher.VoucherDateTime = payment.DocumentDate;
                voucher.VoucherTypeId = payment.VoucherTypeId.GetValueOrDefault();
                voucher.VoucherAmount = payment.AdviceAmount;
                voucher.Account1Id = payment.ClientAcId;
                voucher.Amount1 = -payment.AdviceAmount;
                voucher.Account2Id = payment.BankCashAccountId;
                voucher.Amount2 = payment.BankCashAmount;
                voucher.Account3Id = payment.TDSLedgerAcId;
                voucher.Amount3 = payment.TDSAmount;
                voucher.Amount4 = payment.OtherAmount;
                voucher.Account4Id = payment.Other1AcId;
                voucher.IsAudited = false;
                voucher.IsAccepted = false;
                voucher.IsAccountsVisiblity = false;
                voucher.PageId = null;
                voucher.ViewId = payment.ViewId;
                voucher.AccountingRemark = narration;
                voucher.UserRemark = payment.Remark;
                voucher.ObjectState = payment.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                accountrefs =await uow.RepositoryAsync<Ledger>().Queryable().Where(x =>
                   (x.Id == payment.ClientAcId || x.Id == payment.BankCashAccountId ||
                    x.Id == payment.TDSLedgerAcId || x.Id == payment.Other1AcId) && x.ReferenceFlag).Select(x => x.Id).ToListAsync();
                payment.VoucherId = payment.fk_Voucher.Id;
                if (voucher.Account1Id.GetValueOrDefault() > 0 && voucher.Amount1 != 0)
                {
                    vd1 = new VoucherDetail //Billing Party VD
                    {
                        Id = 0,
                        OfficeId = voucher.OfficeId,
                        VoucherId = voucher.Id,
                        AccountId = voucher.Account1Id.GetValueOrDefault(),
                        Amount = voucher.Amount1,
                        OrderId = 1,
                        ObjectState = ObjectState.Added,
                        Voucher = voucher,
                        ConstCurTypeId = voucher.ConstCurTypeId,
                        CurTypeId = voucher.CurTypeId,
                        CurRate = voucher.CurRate
                    };
                    voucher.VoucherDetails.Add(vd1);
                    vrepo.Insert(vd1);
                }

                var vd2 = new VoucherDetail //Bank Cash VD
                {
                    Id = 0,
                    OfficeId = voucher.OfficeId,
                    ChequeBank = payment.DraweeBank,
                    ChequeDate = payment.ChequeDate,
                    ChequeNo = payment.ChequeNo,
                    VoucherId = voucher.Id,
                    AccountId = voucher.Account2Id.GetValueOrDefault(),
                    Amount = voucher.Amount2,
                    OrderId = 2,
                    ObjectState = ObjectState.Added,
                    Voucher = voucher,
                    ConstCurTypeId = voucher.ConstCurTypeId,
                    CurTypeId = voucher.CurTypeId,
                    CurRate = voucher.CurRate
                };
                voucher.VoucherDetails.Add(vd2);
                vrepo.Insert(vd2);
                if (voucher.Account3Id > 0 && voucher.Amount3 != 0)
                {
                    var vd3 = new VoucherDetail //TDS VD
                    {
                        Id = 0,
                        OfficeId = voucher.OfficeId,
                        VoucherId = voucher.Id,
                        AccountId = voucher.Account3Id.GetValueOrDefault(),
                        Amount = voucher.Amount3,
                        OrderId = 3,
                        ObjectState = ObjectState.Added,
                        Voucher = voucher,
                        Account1Id = payment.ClientAcId,
                        Amount1=payment.AdviceAmount,
                        ConstCurTypeId = voucher.ConstCurTypeId,
                        CurTypeId = voucher.CurTypeId,
                        CurRate = voucher.CurRate
                    };
                    voucher.VoucherDetails.Add(vd3);
                    vrepo.Insert(vd3);
                    if (accountrefs.Any(x => x == vd3.AccountId))
                    {
                        var vdr = new VoucherDetailReference  //TDS VDR
                        {
                            Id = 0,
                            VoucherDetailId = vd3.Id,
                            fk_VoucherDetail = vd3,
                            Amount = vd3.Amount,
                            VDRTypeId = 1013,//New Ref
                            DueDate = payment.DocumentDate,
                            ReferenceNo = payment.DocumentNo,
                            AccountId = vd3.AccountId,
                            ObjectState = ObjectState.Added,
                            ConstCurTypeId = vd3.ConstCurTypeId,
                            CurTypeId = vd3.CurTypeId,
                            CurRate = vd3.CurRate
                        };
                        vd3.VoucherDetailReferences.Add(vdr);
                        vdrrepo.Insert(vdr);
                    }
                }
                if (voucher.Account4Id > 0 && voucher.Amount4 != 0)
                {
                    var vd4 = new VoucherDetail//Extra Dr Ledger VD
                    {
                        Id = 0,
                        OfficeId = voucher.OfficeId,
                        VoucherId = voucher.Id,
                        AccountId = voucher.Account4Id.GetValueOrDefault(),
                        Amount = voucher.Amount4,
                        OrderId = 4,
                        ObjectState = ObjectState.Added,
                        Voucher = voucher,
                        ConstCurTypeId = voucher.ConstCurTypeId,
                        CurTypeId = voucher.CurTypeId,
                        CurRate = voucher.CurRate
                    };

                    voucher.VoucherDetails.Add(vd4);
                    vrepo.Insert(vd4);
                    if (accountrefs.Any(x => x == vd4.AccountId))
                    {
                        var vdr4 = new VoucherDetailReference  // Extra Dr Ledger VDR
                        {
                            Id = 0,
                            VoucherDetailId = vd4.Id,
                            fk_VoucherDetail = vd4,
                            Amount = vd4.Amount,
                            VDRTypeId = 1013,//New Ref
                            DueDate = voucher.VoucherDate,
                            ReferenceNo = payment.DocumentNo,
                            ObjectState = ObjectState.Added,
                            AccountId = vd4.AccountId,
                            ConstCurTypeId = voucher.ConstCurTypeId,
                            CurTypeId = voucher.CurTypeId,
                            CurRate = voucher.CurRate
                        };
                        vd4.VoucherDetailReferences.Add(vdr4);
                        vdrrepo.Insert(vdr4);
                    }
                }

                await uow.SaveChangesAsync();
            }
            var logs = payment.BulkLog;
            if (logs != null && logs.Any())
            {
                
                var repo = uow.RepositoryAsync<CNBillPaymentLog>();
                var vdrrepo = uow.RepositoryAsync<VoucherDetailReference>();
                var trepo = uow.RepositoryAsync<TripAdvanceLog>();
                //var plogs = Mapper.Map<List<vwBillPaymentLog>, List<CNBillPaymentLog>>(logs);
                var _mapper =
                    new MapperConfiguration(cfg => cfg.CreateMap<vwBillPaymentLog, CNBillPaymentLog>())
                        .CreateMapper();
                foreach (var log in logs)
                {
                    log.VDRId = null;
                    var plog = _mapper.Map<vwBillPaymentLog, CNBillPaymentLog>(log);
                    plog.ObjectState = log.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    plog.VDRId = null;
                    plog.PaymentId = payment.Id;
                    if (payment.GenerateVoucherOnServer&& vd1 != null &&
                        accountrefs.Any(x => x == vd1.AccountId))
                    {
                        var vdr = new VoucherDetailReference  // Billing Party VDR
                        {
                            Id = 0,
                            VoucherDetailId = vd1.Id,
                            fk_VoucherDetail = vd1,
                            Amount = vd1.Amount,
                            VDRTypeId = 1449,
                            DueDate = payment.DocumentDate,
                            ReferenceNo = payment.DocumentNo + "-" + log.CNNo,
                            ObjectState=ObjectState.Added,
                            ConstCurTypeId = vd1.ConstCurTypeId,
                            CurTypeId = vd1.CurTypeId,
                            CurRate = vd1.CurRate
                        };
                        vd1.VoucherDetailReferences.Add(vdr);
                        plog.fk_VDR = vdr;
                        vdrrepo.Insert(vdr);
                        if (log.DriverAdvAmt > 0)
                        {
                            if (log.DriverId.GetValueOrDefault() <= 0)
                            {
                                throw new BusinessException(ErrorCode.GLB106, $"Driver Name should be selected for CN No Advance {log.CNNo} as Driver Advance Amount is greater than zero");
                            }
                            if (log.VehicleId.GetValueOrDefault() <= 0)
                            {
                                throw new BusinessException(ErrorCode.GLB106, $"VehicleNum should be selected for CN No Advance {log.CNNo} as Driver Advance Amount is greater than zero");
                            }
                            if (log.DriverAdvDrAccountId.GetValueOrDefault() <= 0)
                            {
                                throw new BusinessException(ErrorCode.GLB106, $"Driver Advance Debit Account should be provided for CN No Advance {log.CNNo} as Driver Advance Amount is greater than zero");
                            }
                            if (log.TripAdvanceId > 0)
                            {
                                await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET VDRId=NULL WHERE Id={log.TripAdvanceId}");
                            }
                            var driverAdvance = (log.TripAdvanceId > 0
                                                    ? await _advRepo.Queryable().Where(x=>x.Id== log.TripAdvanceId).Include(y=>y.fk_Voucher).FirstOrDefaultAsync()
                                                    : null) ?? new TripAdvanceLog();                            
                            if (log.TripAdvanceId > 0)
                            {
                                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVoucherVD] WHERE VoucherId={driverAdvance.VoucherId}");
                            }
                            driverAdvance.ConstCurTypeId = payment.ConstCurTypeId;
                            driverAdvance.CurTypeId = payment.CurTypeId;
                            driverAdvance.CurRate = payment.CurRate;
                            driverAdvance.PaidInId = payment.PaymentModeId;
                            driverAdvance.OfficeId = payment.OfficeId;
                            driverAdvance.VehicleId = log.VehicleId;
                            driverAdvance.DriverId = log.DriverId;
                            driverAdvance.AdvanceDate = payment.DocumentDate;
                            driverAdvance.ReferenceNo = "CNADV-" + payment.DocumentNo + "-" + log.CNNo;
                            driverAdvance.CreditAccountId = payment.BankCashAccountId;
                            driverAdvance.DebitAccountId = log.DriverAdvDrAccountId;
                            driverAdvance.ExpenseId = null;
                            driverAdvance.FuelId = null;
                            driverAdvance.FuelQty = 0;
                            driverAdvance.FuelRate = 0;
                            driverAdvance.FuelAmount = 0;
                            driverAdvance.CashAmount=driverAdvance.BasicAmt = log.DriverAdvAmt;
                            driverAdvance.Remark = log.Remark;
                            driverAdvance.AdvanceTypeId = 1;
                            driverAdvance.VoucherNo = "CNADV-" + payment.DocumentNo + "-" + log.CNNo;
                            driverAdvance.VoucherId = driverAdvance.VoucherId.GetValueOrDefault(0);
                            driverAdvance.TripLogId = log.TripLogId;
                            driverAdvance.BalanceQty = 0;
                            driverAdvance.ViewId = payment.ViewId;
                            driverAdvance.Ref1 = "";
                            driverAdvance.ObjectState = driverAdvance.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                            plog.fk_TripAdvance = driverAdvance;
                            _advRepo.PrepareV(driverAdvance);
                            _advRepo.PrepareVD(driverAdvance);
                            foreach (var detail in driverAdvance.fk_Voucher.VoucherDetails)
                            {
                                _advRepo.PrepareVDR(detail,driverAdvance);
                            }
                        }
                        else if (log.TripAdvanceId > 0)
                        {
                            var adv = await trepo.Queryable().Include(x => x.fk_Voucher).FirstOrDefaultAsync(x => x.Id == log.TripAdvanceId);
                            if (adv != null)
                            {
                                adv.ObjectState = ObjectState.Deleted;
                                if (adv.fk_Voucher != null)
                                {
                                    adv.fk_Voucher.ObjectState = ObjectState.Deleted;
                                }
                            }
                        }
                    }
                    payment.PaymentLogs.Add(plog);
                    repo.Insert(plog);
                }
            }
        }
        public override void Delete(CNBillPayment entity)
        {
            if (entity.PaymentLogs != null && entity.PaymentLogs.Any())
            {
                entity.PaymentLogs?.ForEach(x=>x.ObjectState=ObjectState.Deleted);
            }
            else
            {
                var statusid = _repository.UOW.Context.GetDTSStatusIdByDateId(1561);
                //_repository.GetRepository<DTSStatus>()?.Queryable()?
                //               .Where(x => x.DateId == 1561)?
                //               .Select(x => new { x.Id })?
                //               .FromCacheFirstOrDefault()
                //               ?.Id ?? 0;
                var statues =
                    (from l in _repository.GetRepository<CNDTSStatusLog>().Queryable().Where(x => x.StatusId == statusid)
                        join l1 in _repository.GetRepository<CNBillPaymentLog>()
                            .Queryable()
                            .Where(x => x.PaymentId == entity.Id) on l.CNId equals l1.CNId
                        select l).Distinct().ToList();
                foreach (var status in statues)
                {
                    status.ObjectState = ObjectState.Deleted;
                    new CNDTSStatusCoreLogic().Bind(_repository.UOW.Context).Execute(_repository.UOW.Context.Entry(status));
                }
            }
            
            base.Delete(entity);
        }
    }

}
