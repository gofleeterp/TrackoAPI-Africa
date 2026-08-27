using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;
using TrackoAPI.vw.ts;

namespace TrackoApi.Service
{
    public interface IVehicleTripSettlementService : IService<VehicleTripSettlement>
    {
        IQueryable<VehicleTripSettlement> GetAllVehicleTripSettlementList(int id);
        VehicleTripSettlement PrepareSettlement(long key, VehicleTripSettlement settlement);
        Task CreateSettlementV2(VehicleTripSettlement s, IUnitOfWorkAsync uow);
        Task CreateSettlementV3(VehicleTripSettlement sat, IUnitOfWorkAsync uow);
        Task CreateSettlementV4(VehicleTripSettlement sat, IUnitOfWorkAsync uow);
        Task HireSettlementV1(VehicleTripSettlement sat, IUnitOfWorkAsync uow);
    }
    public class VehicleTripSettlementService : Service<VehicleTripSettlement>, IVehicleTripSettlementService
    {
        private readonly IRepositoryAsync<VehicleTripSettlement> _repository;
        public VehicleTripSettlementService(IRepositoryAsync<VehicleTripSettlement> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleTripSettlement> GetAllVehicleTripSettlementList(int brandid)
        {
            return _repository.GetAllVehicleTripSettlementList(brandid);
        }

        /// <exception cref="BusinessException"><code>ErrorCode.TS100</code>One of Advance is with Id 0. Which is not accepted.</exception>
        /// <exception cref="BusinessException"><code>ErrorCode.TS101</code>One of Trip Expanse is with Id 0 & is Marked as deleted.</exception>
        /// <exception cref="BusinessException"><code>ErrorCode.TS102</code>One of Vehicle Movement Log is without identification.</exception>
        /// <exception cref="BusinessException"><code>ErrorCode.TS103</code>One of Fuel Expanse is without parent identification(AdvanceId or TripLogId)</exception>
        public VehicleTripSettlement PrepareSettlement(long key, VehicleTripSettlement settlement)
        {
            if (settlement.SettlementAccountId.HasValue)
            {

            }
            var fed = settlement.vwFuelExpenses.Where(x => x.IsDeleted).Select(x => x.Id).ToList();
            fed.AddRange(settlement.vwTripExpenses.Where(x => x.IsDeleted).Select(x => x.Id));
            if (fed.Any())
            {
                this.ExecuteSql($"DELETE [dbo].[tTripExpenseLog] WHERE Id in ({fed.JoinStrings(",")})");
                settlement.vwTripExpenses?.RemoveAll(x => x.IsDeleted);
                settlement.vwFuelExpenses?.RemoveAll(x => x.IsDeleted);
            }

            if (settlement.vwTripAdvances.Any(x => x.Id == 0)) throw new BusinessException(ErrorCode.TS100, "One of Advance is with Id 0. Which is not accepted");
            if (settlement.vwTripExpenses.Any(x => x.Id == 0 && x.IsDeleted)) throw new BusinessException(ErrorCode.TS101, "One of Trip Expanse is with Id 0 & is Marked as deleted." + Environment.NewLine + " Which is not accepted");
            if (settlement.vwTripLogs.Any(x => x.Id == 0)) throw new BusinessException(ErrorCode.TS102, "One of Vehicle Movement Log is without identification." + Environment.NewLine + " Which is not accepted");
            if (settlement.vwFuelExpenses.Any() && settlement.vwFuelExpenses.Any(x => x.AdvanceId == 0 || !x.TripLogId.HasValue)) throw new BusinessException(ErrorCode.TS103, "One of Fuel Expanse is without parent identification(AdvanceId or TripLogId)." + Environment.NewLine + " Which is not accepted");
            //Cross Check if any Deleted TripLog is Referred in any alive Referrals
            foreach (var x in settlement.vwTripLogs.Where(x => x.IsDeleted))
            {
                if (settlement.vwFuelExpenses.Any(z => !z.IsDeleted && z.TripLogId == x.Id))
                {
                    throw new BusinessException(ErrorCode.TS103, $"One of Fuel Expanse has reference to dead parent (TripLogId: {x.Id}).{Environment.NewLine}. Which is not accepted");
                }
                if (settlement.vwTripExpenses.Any(z => !z.IsDeleted && z.TripLogId == x.Id))
                {
                    throw new BusinessException(ErrorCode.TS101, $"One of Trip Expanse has reference to dead parent (TripLogId: {x.Id}).{Environment.NewLine}. Which is not accepted");
                }
                if (settlement.vwTripAdvances.Any(z => !z.IsDeleted && z.TripLogId == x.Id))
                {
                    throw new BusinessException(ErrorCode.TS100, $"One of Trip Advance has reference to dead parent (TripLogId: {x.Id}).{Environment.NewLine}. Which is not accepted");
                }
            }
            if (settlement.TripAdvances == null)
                settlement.TripAdvances = new List<TripAdvanceLog>();
            if (settlement.TripExpenses == null)
                settlement.TripExpenses = new List<TripExpenseLog>();
            if (settlement.TripLogs == null)
                settlement.TripLogs = new List<VehicleMovementLog>();
            #region TripAdvance Mapping
            var advRepo = this._repository.GetRepository<TripAdvanceLog>();
            foreach (var x in settlement.vwTripAdvances)
            {
                if (x.TripLogId == null)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"One of Trip Advance  doesn't mapped to TripLog");
                }
                TripAdvanceLog adv = null;
                if (settlement.TripAdvances != null && x.Id > 0 && settlement.TripAdvances.Any(a => a.Id == x.Id))
                {
                    adv = settlement.TripAdvances.First(a => a.Id == x.Id);
                    if (x.IsDeleted)
                    {
                        adv.SettlementId = null;
                        adv.fk_Settlement = null;
                        if (x.TypeId == 17)
                        {
                            adv.TripLogId = null;
                            adv.fk_Triplog = null;
                        }
                        adv.ObjectState = ObjectState.Modified;
                    }
                    else
                    {
                        adv.TripLogId = x.TripLogId;
                        adv.ObjectState = ObjectState.Modified;
                        adv.VehicleId = settlement.VehicleId;
                    }

                }
                else if (x.Id > 0 && advRepo.Queryable().Any(a => a.Id == x.Id))
                {
                    adv = advRepo.Queryable().Include(z => z.fk_DebitAccount).FirstOrDefault(a => a.Id == x.Id);
                    if (adv != null)
                    {
                        adv.SettlementId = settlement.Id;
                        adv.fk_Settlement = settlement;
                        adv.TripLogId = x.TripLogId;
                        adv.VehicleId = settlement.VehicleId;
                        adv.ObjectState = ObjectState.Modified;
                    }
                    settlement.TripAdvances.Add(adv);
                }
                if (adv == null)
                {
                    throw new BusinessException(ErrorCode.TS100, $"Advance with id {x.Id} does not exists in system");
                }
            }

            #endregion

            #region Trip Expanses Mapping
            var expRepo = this._repository.GetRepository<TripExpenseLog>();
            foreach (var x in settlement.vwTripExpenses)
            {
                TripExpenseLog exp = null;
                if (x.Id > 0 && settlement.TripExpenses.Any(a => a.Id == x.Id))
                {
                    exp = settlement.TripExpenses.First(a => a.Id == x.Id);
                    if (x.IsDeleted)
                    {
                        exp.SettlementId = null;
                        exp.fk_Settlement = null;
                        exp.ObjectState = ObjectState.Deleted;//settlement.vwTripLogs.Any(e => !e.IsDeleted && e.Id == x.TripLogId) ? ObjectState.Deleted : ObjectState.Modified;
                        continue;
                    }
                }
                else if (x.Id > 0 && expRepo.Queryable().Any(a => a.Id == x.Id))
                {
                    exp = expRepo.Queryable().Include(z => z.fk_ExpenseType.fk_Ledger).FirstOrDefault(e => e.Id == x.Id);
                    settlement.TripExpenses.Add(exp);
                }
                if (exp == null)
                {
                    exp = new TripExpenseLog();
                }
                exp.ClaimAmount = x.ClaimAmt;
                exp.ExpenseTypeId = x.TypeId;
                exp.Remarks = x.Remark;
                exp.TripLogId = x.TripLogId;
                exp.SettledAmount = x.SettledAmt;
                exp.SettlementId = settlement.Id;
                exp.fk_Settlement = settlement;
                exp.FuelQty = x.FuelQty;
                exp.FuelRate = x.Rate;
                exp.TripAdvanceLogId = x.TripAdvanceLogId;
                exp.ObjectState = exp.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                if (exp.ObjectState == ObjectState.Added)
                {
                    settlement.TripExpenses.Add(exp);
                }
            }

            #endregion

            #region Vehicle Movement Log
            var tripLogRepo = this._repository.GetRepository<VehicleMovementLog>();
            foreach (var x in settlement.vwTripLogs)
            {
                VehicleMovementLog tl;
                if (x.Id > 0 && settlement.TripLogs.Any(a => a.Id == x.Id))
                {
                    tl = settlement.TripLogs.First(a => a.Id == x.Id);
                    if (x.IsDeleted)
                    {
                        tl.SettlementId = null;
                        tl.fk_TripSettlement = null;
                        tl.ObjectState = ObjectState.Modified;
                    }
                }
                else if (x.Id > 0 && tripLogRepo.Queryable().Any(a => a.Id == x.Id))
                {
                    tl = tripLogRepo.Queryable().First(a => a.Id == x.Id);
                    tl.SettlementId = settlement.Id;
                    tl.fk_TripSettlement = settlement;
                    tl.ObjectState = ObjectState.Modified;
                    settlement.TripLogs.Add(tl);
                }
                else
                {
                    throw new BusinessException(ErrorCode.TS102, $"Vehicle Movement Log with identification {x.Id} is invalid");
                }

            }

            #endregion
            #region Fuel Expanses Mapping

            var expadvIds =
                settlement.vwFuelExpenses.Where(x => (x.ShortageQty + x.UsedQty) > 0)
                    .Select(x => x.AdvanceId)
                    .Distinct().ToList();
            var feAdvances = advRepo.Queryable().Include(f => f.FuelExpanses.Select(y => y.fk_ExpenseType.fk_Ledger)).Where(x => expadvIds.Contains(x.Id)).ToList();
            foreach (IGrouping<long, FuelExpense> expanses in settlement.vwFuelExpenses.Where(x => (x.ShortageQty + x.UsedQty) > 0).GroupBy(x => x.AdvanceId))
            {
                TripAdvanceLog adv = null;
                adv = settlement.TripAdvances.Any(a => a.Id == expanses.Key) ? settlement.TripAdvances.First(a => a.Id == expanses.Key) : feAdvances.FirstOrDefault(a => a.Id == expanses.Key);
                //Through Error if Provided AdvanceId is Wrong
                if (adv == null)
                {
                    throw new BusinessException(ErrorCode.TS103, $"AdvanceId {expanses.Key} is wrong");
                }
                ////If Fuel Expanses is empty for advance then retry to fatch them direct from DataBase
                //if (adv.FuelExpanses == null || !adv.FuelExpanses.Any())
                //{
                //    var exps = expRepo.Queryable().Include(z => z.fk_ExpenseType.fk_Ledger).Where(z => z.TripAdvanceLogId == adv.Id);
                //    if (exps.Any())
                //    {
                //        adv.FuelExpanses = new List<TripExpenseLog>();
                //        adv.FuelExpanses.AddRange(exps);
                //    }
                //}
                if (adv.FuelExpanses != null && adv.FuelExpanses.Any())
                {
                    //var tls = settlement.TripLogs.Where(x=>x.SettlementId.HasValue).Select(x => x.Id);
                    var tls = settlement.TripLogs.Select(x => x.Id);
                    foreach (var x in adv.FuelExpanses.Where(x => !tls.Contains(x.TripLogId)))
                    {
                        x.ObjectState = ObjectState.Unchanged;
                    }
                }
                foreach (FuelExpense expanse in expanses)
                {
                    var exp = adv.FuelExpanses.FirstOrDefault(x => x.Id == expanse.Id);
                    if (exp == null || exp == default(TripExpenseLog))
                    {
                        exp = expRepo.Queryable().Include(z => z.fk_ExpenseType.fk_Ledger).FirstOrDefault(a => a.Id == expanse.Id && a.TripAdvanceLogId == adv.Id);
                        if (exp == null || exp == default(TripExpenseLog))
                        {
                            exp = new TripExpenseLog();
                        }
                    }
                    if (expanse.IsDeleted)
                    {
                        exp.SettlementId = null;
                        exp.fk_Settlement = null;
                        exp.fk_TripAdvanceLog = null;
                        exp.fk_TripAdvanceLog = null;
                        exp.fk_TripLog = null;
                        exp.fk_TripLog = null;
                        exp.ObjectState = ObjectState.Deleted;
                        adv.BalanceQty = adv.FuelExpanses.Where(a => a.ObjectState != ObjectState.Deleted).Sum(a => a.FuelQty + a.ShortFuelQty);
                        adv.ObjectState = ObjectState.Modified;
                        expRepo.Delete(exp);
                    }
                    else
                    {
                        decimal otherConsume = 0;
                        if (adv.FuelExpanses != null && adv.FuelExpanses.Any())
                        {
                            otherConsume = expanse.Id > 0 ? adv.FuelExpanses.Where(a => a.Id != expanse.Id).Sum(r => r.FuelQty + r.ShortFuelQty) : adv.FuelExpanses.Sum(r => r.FuelQty + r.ShortFuelQty);
                        }

                        if (adv.FuelQty <= otherConsume)
                            throw new BusinessException(ErrorCode.TS103, $"The Total Fuel Qty is already adjusted against advancelog id:{expanse.AdvanceId}");
                        if ((expanse.UsedQty + expanse.ShortageQty) > (adv.FuelQty - otherConsume))
                            throw new BusinessException(ErrorCode.TS103, $"The Consumed Fuel Qty {expanse.UsedQty + expanse.ShortageQty} Exceeded than Balance Qty {adv.FuelQty - otherConsume} against advancelog no:{adv.ReferenceNo}");
                        exp.FuelQty = expanse.UsedQty;
                        exp.TripAdvanceLogId = expanse.AdvanceId;
                        exp.fk_TripAdvanceLog = adv;
                        exp.ExpenseTypeId = exp.ExpenseTypeId; //26
                        exp.Remarks = expanse.Remark;
                        exp.SettledAmount = expanse.UsedAmt;
                        exp.SettlementId = settlement.Id;
                        exp.fk_Settlement = settlement;
                        exp.ObjectState = exp.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        exp.ClaimAmount = 0;
                        exp.TripLogId = expanse.TripLogId.GetValueOrDefault(0);
                        exp.ShortFuelQty = expanse.ShortageQty;
                        exp.ShortFuelAmt = expanse.ShortageAmt;
                        adv.BalanceQty = adv.FuelQty - adv.FuelExpanses.Where(a => a.ObjectState != ObjectState.Deleted).Sum(a => a.FuelQty + a.ShortFuelQty);
                        adv.ObjectState = ObjectState.Modified;
                        //Through Error if Provided AdvanceId is not mapped with any TripLog in currant Settlement
                        if (settlement.TripLogs.All(e => e.Id != expanse.TripLogId.GetValueOrDefault(0)))
                        {
                            throw new BusinessException(ErrorCode.TS103, $"TripLogId {expanse.TripLogId} is wrong against advance id:{expanse.AdvanceId}");
                        }
                        if (settlement.TripExpenses.All(x => x.Id != exp.Id))
                        {
                            settlement.TripExpenses.Add(exp);
                        }

                    }
                }
            }
            #endregion
            #region Other Properties
            settlement.OfficeId = settlement.OfficeId;
            settlement.StartDate = settlement.StartDate;
            settlement.TripSheetNo = settlement.TripSheetNo;
            settlement.EndDate = settlement.EndDate;
            settlement.SettleDate = settlement.SettleDate;
            settlement.VehicleId = settlement.VehicleId;
            settlement.Driver1Id = settlement.Driver1Id;
            settlement.TripRoute = settlement.TripRoute;
            settlement.Remarks = settlement.Remarks;
            #endregion

            #region KM Run Calculation
            settlement.StartKm = settlement.StartKm;
            settlement.EndKm = settlement.EndKm;
            settlement.RunKm = settlement.RunKm;
            settlement.AddRunKm = settlement.AddRunKm;
            settlement.TotalKmRun = settlement.TotalKmRun;
            settlement.TotalReferHour = settlement.TotalReferHour;
            #endregion

            #region Diesel/Fuel Calculation

            settlement.FuelQuantity = settlement.FuelQuantity;
            settlement.ReferQuantity = settlement.ReferQuantity;
            settlement.ExtraFuelQty = settlement.ExtraFuelQty;
            //settlement.ShortageQty = (settlement.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0 && x.ObjectState != ObjectState.Deleted).Sum(x => x.ShortFuelQty));
            #endregion

            #region Driver/Settlement Accounting

            //settlement.ShortageFuelAmt = settlement.ShortageFuelAmt;
            settlement.DriverPayment = settlement.DriverPayment;
            //settlement.SettledAmount = settlement.SettledAmount;
            settlement.Compute();
            #endregion
            PrepareVoucher(settlement);
            return settlement;
        }

        /// <summary>
        /// Prepares the voucher.
        /// </summary>
        /// <param name="s">The Settlement Class</param>
        /// <exception cref="BusinessException"><code>ErrorCode.TS101</code>If s.TripExpanses.Count==0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count" />. </exception>
        public void PrepareVoucher(VehicleTripSettlement s)
        {
            if (s.SettleDate.HasValue)
            {
                var voucherRepo = this._repository.GetRepository<Voucher>();
                if (s.fk_Voucher != null && s.fk_Voucher.Id > 0)
                {
                    s.VoucherId = s.fk_Voucher.Id;
                }
                else if (s.VoucherId > 0 && (s.fk_Voucher == null || s.fk_Voucher == default(Voucher)))
                {
                    if (voucherRepo.Queryable().Any(x => x.Id == s.VoucherId))
                    {
                        s.fk_Voucher = voucherRepo.Queryable().First(x => x.Id == s.VoucherId);
                    }
                    else
                    {
                        s.VoucherId = null;
                        s.fk_Voucher = new Voucher();
                    }
                }
                else
                {
                    s.fk_Voucher = new Voucher();
                }
                s.fk_Voucher.ViewId = s.ViewId;
                s.fk_Voucher.OfficeId = s.OfficeId;
                PrepareVoucherDetails(s, s.fk_Voucher);

                s.fk_Voucher.VoucherNo = s.TripSheetNo;
                s.fk_Voucher.VoucherDate = s.SettleDate.Value.Date;
                s.fk_Voucher.VoucherDateTime = s.SettleDate.Value;
                s.fk_Voucher.ObjectState = s.fk_Voucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                s.fk_Voucher.VoucherAmount = s.SettledAmount;//s.fk_Voucher.VoucherDetails.Sum(x => x.Amount);
                s.fk_Voucher.VoucherTypeId = 18;
                var order1 = s.fk_Voucher.VoucherDetails.Where(x => x.OrderId == 1 && x.ObjectState != ObjectState.Deleted).ToList();
                if (order1.Any() && order1.FirstOrDefault() != default(VoucherDetail)) //Trip Expenses
                {
                    s.fk_Voucher.Account1Id = order1?.FirstOrDefault()?.AccountId;
                    s.fk_Voucher.Amount1 = order1.Sum(x => x.Amount);
                }
                else
                {
                    s.fk_Voucher.Account1Id = null;
                    s.fk_Voucher.Amount1 = 0;
                }
                var order2 = s.fk_Voucher.VoucherDetails.Where(x => x.OrderId == 2 && x.ObjectState != ObjectState.Deleted).ToList();
                if (order2.Any() && order2.FirstOrDefault() != default(VoucherDetail))//Fuel Expenses
                {
                    s.fk_Voucher.Account2Id = order2?.FirstOrDefault()?.AccountId;
                    s.fk_Voucher.Amount2 = order2.Sum(x => x.Amount);
                }
                else
                {
                    s.fk_Voucher.Account2Id = null;
                    s.fk_Voucher.Amount2 = 0;
                }
                var order3 = s.fk_Voucher.VoucherDetails.Where(x => x.OrderId == 3 && x.ObjectState != ObjectState.Deleted).ToList();
                if (order3.Any() && order3.FirstOrDefault() != default(VoucherDetail))//Trip Advances
                {
                    s.fk_Voucher.Account3Id = order3?.FirstOrDefault()?.AccountId;
                    s.fk_Voucher.Amount3 = order3.Sum(x => x.Amount);
                }
                else
                {
                    s.fk_Voucher.Account3Id = null;
                    s.fk_Voucher.Amount3 = 0;
                }
                var order4 = s.fk_Voucher.VoucherDetails.Where(x => x.OrderId == 4 && x.ObjectState != ObjectState.Deleted).ToList();
                if (order4.Any() && order4.FirstOrDefault() != default(VoucherDetail))//Settled Amount
                {
                    s.fk_Voucher.Account4Id = order4?.FirstOrDefault()?.AccountId;
                    s.fk_Voucher.Amount4 = order4.Sum(x => x.Amount);
                }
                else
                {
                    s.fk_Voucher.Account4Id = null;
                    s.fk_Voucher.Amount4 = 0;
                }
                var order5 = s.fk_Voucher.VoucherDetails.Where(x => x.OrderId == 5 && x.ObjectState != ObjectState.Deleted).ToList();
                if (order5.Any() && order5.FirstOrDefault() != default(VoucherDetail))//Settled Amount
                {
                    s.fk_Voucher.Account5Id = order5?.FirstOrDefault()?.AccountId;
                    s.fk_Voucher.Amount5 = order5.Sum(x => x.Amount);
                }
                else
                {
                    s.fk_Voucher.Account5Id = null;
                    s.fk_Voucher.Amount5 = 0;
                }
                //s.fk_Voucher.Account2Id = s.fk_Voucher.VoucherDetails[1].AccountId;
                //s.fk_Voucher.Amount1 = s.fk_Voucher.VoucherDetails[0].Amount*1;
                //s.fk_Voucher.Amount2 = s.fk_Voucher.VoucherDetails[1].Amount*-1;
                s.fk_Voucher.UserRemark = s.Remarks;
                //TODO:Setup Account Narration from Template located with VoucherType
                s.fk_Voucher.AccountingRemark = "";
                if (s.fk_Voucher.VoucherDetails.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.Amount) != 0)
                {
                    throw new BusinessException(ErrorCode.VCH104, $"Amount does not tailied for Trip Settlement No {s.TripSheetNo}");
                }
                s.fk_Voucher.VoucherAmount = s.fk_Voucher.VoucherDetails
                    .Where(x => x.ObjectState != ObjectState.Deleted && x.Amount > 0)
                    .Sum(x => x.Amount);
            }
            else if (s.VoucherId.HasValue)
            {
                //If Settlement don't have Settled Date make sure to delete AccountVoucher
                s.VoucherId = null;
                if (s.fk_Voucher != null)
                {
                    s.fk_Voucher.ObjectState = ObjectState.Deleted;
                }
                s.SetlBalVoucherId = null;
                if (s.fk_SetlBalVoucher != null)
                {
                    s.fk_SetlBalVoucher.ObjectState = ObjectState.Deleted;
                }
            }
        }

        /// <summary>
        /// Prepares the voucher details.
        /// </summary>
        /// <param name="s">The Settlement Class.</param>
        /// <param name="v">The Voucher Class.</param>
        /// <exception cref="BusinessException"><code>ErrorCode.TS101</code>If s.TripExpanses.Count==0.</exception>
        /// <exception cref="BusinessException"><code>ErrorCode.TS101</code>If Accounting Ledger is not Mapped to Expanse Type.</exception>
        public void PrepareVoucherDetails(VehicleTripSettlement s, Voucher v)
        {
            if (!s.TripExpenses.Any() || s.TripExpenses.Count(x => x.ObjectState != ObjectState.Deleted) == 0)
            {
                throw new BusinessException(ErrorCode.TS101, @"No Trip Expanses were attached to Settlement");
            }
            //Get All Expanse Names to whom the Ledger is not Mapped
            var invalidExpNames = string.Empty;
            var ids = s.TripExpenses?.Where(x => x.fk_ExpenseType == null).Select(x => x.ExpenseTypeId).ToList() ?? new List<long>();
            var fts = _repository.GetRepository<ExpenseMaster>().Queryable().Include(x => x.fk_Ledger).Where(x => ids.Contains(x.Id)).ToList();
            if (s.TripExpenses != null)
            {
                foreach (var x in s.TripExpenses)
                {
                    if (x.fk_ExpenseType == null)
                    {
                        x.fk_ExpenseType = fts.FirstOrDefault(y => y.Id == x.ExpenseTypeId);
                    }
                    if (x.ExpenseTypeId == 0 || x.fk_ExpenseType == null || x.fk_ExpenseType.LedgerId.GetValueOrDefault(0) == 0) invalidExpNames = invalidExpNames + (x.fk_ExpenseType.Name + ",");
                }
            }
            if (!string.IsNullOrWhiteSpace(invalidExpNames))
            {
                throw new BusinessException(ErrorCode.TS101, $"Accounting Ledger is not Mapped to Expanse Type(s) :{invalidExpNames}");
            }
            var vdRepo = this._repository.GetRepository<VoucherDetail>();
            var vdrRepo = this._repository.GetRepository<VoucherDetailReference>();
            if (v.VoucherDetails == null || !v.VoucherDetails.Any())
            {
                v.VoucherDetails = new List<VoucherDetail>();
                if (v.Id > 0)
                {
                    var d = vdRepo.Queryable().Where(x => x.VoucherId == v.Id);
                    if (d.Any())
                    {
                        v.VoucherDetails.AddRange(d);
                    }
                }
                foreach (var x in v.VoucherDetails)
                {
                    x.ObjectState = ObjectState.Deleted;
                    foreach (var y in x.VoucherDetailReferences)
                    {
                        y.ObjectState = ObjectState.Deleted;
                    }
                    x.Voucher = null;
                    x.VoucherId = 0;
                    x.fk_Account = null;
                }
            }
            #region Trip Expanses Logic Debit Voucher Detail&VoucherDetailRefrence
            //Group By LedgerId
            foreach (var eg in s.TripExpenses.Where(x => x.fk_TripAdvanceLog?.AdvanceTypeId != 3 && x.ObjectState != ObjectState.Deleted).GroupBy(x => x.fk_ExpenseType.LedgerId))
            {
                //Then Group By OfficeId
                //Commented two maintain 
                //foreach (var gp in eg.GroupBy(x=>x.fk_ExpenseType.fk_Ledger.OfficeId))
                //{
                var vd1 = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = eg.Key.GetValueOrDefault(0),
                    Amount = eg.Sum(x => x.SettledAmount),
                    ObjectState = ObjectState.Added,
                    OfficeId = v.OfficeId,
                    OrderId = 1,
                    VoucherId = v.Id,
                    fk_Account = eg.First().fk_ExpenseType.fk_Ledger
                };
                if (eg.First().fk_ExpenseType.fk_Ledger.ReferenceFlag)
                {
                    vd1.VoucherDetailReferences = new List<VoucherDetailReference>()
                    {
                        new VoucherDetailReference()
                        {
                            Amount = vd1.Amount,
                            ObjectState = ObjectState.Added,
                            ReferenceNo = $"TE-{s.TripSheetNo}-{eg.Key.GetValueOrDefault(0)}",
                            VDRTypeId = 1013,//New Reference
                            VoucherDetailId = vd1.Id,
                            fk_VoucherDetail = vd1
                        }
                    };
                }
                v.VoucherDetails.Add(vd1);
                //}

            }
            #endregion
            #region Fuel Expanses
            //Create Debit and Credit Voucher Details and VoucherDetailReference for Fuel Expanses
            foreach (var log in s.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0 && x.fk_TripAdvanceLog?.AdvanceTypeId == 3 && x.ObjectState != ObjectState.Deleted).GroupBy(x => x.fk_ExpenseType.LedgerId))
            {

                //Create Credit Voucher Detail
                if (log.Sum(x => x.ShortFuelAmt) == 0) continue;
                var vd3 = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = log.Key.GetValueOrDefault(0),
                    ObjectState = ObjectState.Added,
                    Amount = log.Sum(x => x.ShortFuelAmt) * -1,
                    OfficeId = v.OfficeId,
                    OrderId = 2,//Old was 3
                    VoucherId = v.Id,
                    fk_Account = log.First().fk_ExpenseType.fk_Ledger
                };
                //Create AgainstReference Credit VoucherDetailReference for each Trip Advance
                foreach (var adv in log.GroupBy(x => x.fk_TripAdvanceLog))
                {
                    var a = vdrRepo.Queryable().Include(x => x.fk_VoucherDetail).Where(x => x.fk_VoucherDetail.VoucherId == adv.Key.VoucherId && x.ReferenceNo == adv.Key.ReferenceNo).Select(x => x.Id).FirstOrDefault();
                    if (a != default(long))
                    {
                        //throw new BusinessException(ErrorCode.VCH107, "Vdr Reference not Found");
                        var vdr = new VoucherDetailReference()
                        {
                            Amount = adv.Sum(x => x.ShortFuelAmt) * -1,
                            ObjectState = ObjectState.Added,
                            RefId = a,
                            VDRTypeId = 1014,//Against Reference
                            VoucherDetailId = vd3.Id,
                            fk_VoucherDetail = vd3,
                            ReferenceNo = adv.Key.ReferenceNo,
                        };
                        vd3.VoucherDetailReferences.Add(vdr);
                    }

                }
                v.VoucherDetails.Add(vd3);
            }
            #endregion
            #region Trip Advance Against Reference

            foreach (IGrouping<long?, TripAdvanceLog> tp in s.TripAdvances.Where(x => (x.AdvanceTypeId == 1 || x.AdvanceTypeId == 2 || x.AdvanceTypeId == 16) && x.ObjectState != ObjectState.Deleted).GroupBy(x => x.AdvanceTypeId))
            {
                foreach (IGrouping<long?, TripAdvanceLog> dbt in tp.GroupBy(x => x.DebitAccountId))
                {
                    var vd3 = new VoucherDetail
                    {
                        Voucher = v,
                        AccountId = dbt.Key.GetValueOrDefault(0),
                        ObjectState = ObjectState.Added,
                        Amount = dbt.Sum(x => x.Amount) * -1,
                        OfficeId = v.OfficeId,
                        OrderId = 3,
                        VoucherId = v.Id,
                        VoucherDetailReferences = new List<VoucherDetailReference>()
                    };
                    if (dbt.First().fk_DebitAccount.ReferenceFlag)
                    {
                        foreach (var al in dbt)
                        {

                            var parentVdr =
                                vdrRepo.Queryable()
                                    .Include(x => x.fk_VoucherDetail)
                                    .Where(
                                        x =>
                                            x.fk_VoucherDetail.VoucherId == al.VoucherId &&
                                            x.ReferenceNo == al.ReferenceNo).Select(c => new { c.Id, c.Amount, Balance = c.AgainstReferences.Where(vv => vv.fk_VoucherDetail.VoucherId != v.Id).Sum(a => (decimal?)a.Amount) }).FirstOrDefault();

                            if (parentVdr?.Id > 0)
                            {
                                //throw new BusinessException(ErrorCode.VCH107, "Vdr Reference not Found");
                                if ((parentVdr.Balance * -1) < al.Amount)
                                {
                                    throw new BusinessException(ErrorCode.VCH107, $"Vdr Against Reference amount {al.Amount} exceded the Balance Amount {parentVdr.Balance.GetValueOrDefault(0)} for Advance No {al.ReferenceNo}");
                                }
                                var vdr = new VoucherDetailReference()
                                {
                                    Amount = al.Amount * -1,
                                    ObjectState = ObjectState.Added,
                                    RefId = parentVdr.Id,
                                    VDRTypeId = 1014,//Against Reference
                                    VoucherDetailId = vd3.Id,
                                    fk_VoucherDetail = vd3,
                                    ReferenceNo = al.ReferenceNo,
                                };
                                vd3.VoucherDetailReferences.Add(vdr);
                            }

                        }
                    }
                    v.VoucherDetails.Add(vd3);
                }
            }
            #endregion
            #region Cash Deposit VD
            if (s.CashDeposited > 0)
            {
                var vd5 = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = s.SettlementAccountId.GetValueOrDefault(0),
                    Amount = s.CashDeposited,
                    ObjectState = ObjectState.Added,
                    OfficeId = v.OfficeId,
                    OrderId = 5,
                    VoucherId = v.Id
                };
                v.VoucherDetails.Add(vd5);
            }
            if (s.CashPaid > 0)
            {
                var vd5 = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = s.SettlementAccountId.GetValueOrDefault(0),
                    Amount = -s.CashPaid,
                    ObjectState = ObjectState.Added,
                    OfficeId = v.OfficeId,
                    OrderId = 6,
                    VoucherId = v.Id
                };
                v.VoucherDetails.Add(vd5);
            }
            #endregion
            #region Settled Amount Voucher Details

            if (s.SettledAmount != 0)
            {
                var config =
                    this._repository.GetRepository<ApiConfiguration>()
                        .Queryable()
                        .Where(x => x.Key == "SettledAccountType" || x.Key == "DefaultSettledAccountId")
                        .Select(x => new { x.Value, x.Key })
                        .ToList();
                var settledBalance =
                    config.Where(x => x.Key == "DefaultSettledAccountId").Select(x => x.Value).FirstOrDefault();
                long settledBalanceAcId;
                if (!long.TryParse(settledBalance, out settledBalanceAcId))
                {
                    settledBalanceAcId = 0;
                }
                //0:CashBook,1:Driver,2:Other
                var settledAccountType =
                    config.Where(x => x.Key == "SettledAccountType").Select(x => x.Value).FirstOrDefault();

                int configStatus;
                if (!int.TryParse(settledAccountType, out configStatus))
                {
                    configStatus = 0;
                }
                var ledgerRepo = _repository.GetRepository<Ledger>().Queryable();
                bool referenceFlag = false;
                switch (configStatus)
                {
                    //case 0:
                    //    //0:CashBook i.e Account Role Id=1037//Credit or Debit could be only done in Cash Book Ledger
                    //    var zero =
                    //        ledgerRepo.Where(x => x.Id == s.SettlementAccountId.Value)
                    //            .Select(x => new //(x.OfficeId == s.OfficeId&&x.AccountRoleId== 1037)||
                    //            {
                    //                x.Id,
                    //                x.ReferenceFlag,
                    //                x.AccountRoleId
                    //            }).FirstOrDefault();
                    //    if (zero != null)
                    //    {
                    //        s.SettlementAccountId = zero.Id;
                    //        referenceFlag = zero.ReferenceFlag;
                    //    }
                    //    if (zero.AccountRoleId.Value != 1037)
                    //    {
                    //        throw new BusinessException(ErrorCode.GLB106,"Only Cash Book can be selected in Settled Account Name"); 
                    //    }
                    //    break;
                    case 1:
                        //1:Driver//Credit or Debit could be onle done in Driver Ledger
                        var one = ledgerRepo.Where(x => x.Id == s.Driver1Id).Select(x => new
                        {
                            x.Id,
                            x.ReferenceFlag,
                            x.AccountRoleId
                        }).FirstOrDefault();
                        if (one != null)
                        {
                            settledBalanceAcId = one.Id;
                            referenceFlag = one.ReferenceFlag;
                        }
                        if (one.AccountRoleId.Value != 1085)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Only Driver can be selected in Settled Account Name");
                        }
                        break;
                    //case 2:
                    //    //2:Other
                    //    var two = ledgerRepo.Where(x => x.Id == s.SettlementAccountId).Select(x => new
                    //    {
                    //        x.Id,
                    //        x.ReferenceFlag,
                    //        x.AccountRoleId
                    //    }).FirstOrDefault();
                    //    if (two != null)
                    //    {
                    //        s.SettlementAccountId = two.Id;
                    //        referenceFlag = two.ReferenceFlag;
                    //    }
                    //    break;
                    case 3:

                        //2:Auto Only Cash Account and Control Account are Allowed
                        var three = ledgerRepo.Where(x => x.Id == settledBalanceAcId).Select(x => new
                        {
                            x.Id,
                            x.ReferenceFlag
                        }).FirstOrDefault();
                        //if (three.AccountRoleId != 1037)
                        //{
                        //    var settledAc = ledgerRepo.Where(x => x.Id == settledBalanceAcId).Select(x => new
                        //    {
                        //        x.Id,
                        //        x.ReferenceFlag,
                        //        x.AccountRoleId
                        //    }).FirstOrDefault();
                        //    if (settledAc == null)
                        //    {
                        //        throw new BusinessException(ErrorCode.GLB103,"Default Cash Advance Control Account not Configured.");
                        //    }
                        //    three = settledAc;
                        //}
                        if (three != null)
                        {
                            referenceFlag = three.ReferenceFlag;
                        }
                        else
                        {
                            throw new BusinessException(ErrorCode.GLB103, "Default Cash Advance Control Account not Configured.");
                        }
                        break;
                }
                var vd4 = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = s.SettlementAccountId.GetValueOrDefault(0),
                    Amount = s.SettledAmount,
                    ObjectState = ObjectState.Added,
                    OfficeId = v.OfficeId,
                    OrderId = 4,
                    VoucherId = v.Id
                };

                if (referenceFlag)
                {
                    vd4.VoucherDetailReferences = new List<VoucherDetailReference>()
                    {
                        new VoucherDetailReference()
                        {
                            Amount = vd4.Amount,
                            ObjectState = ObjectState.Added,
                            ReferenceNo = $"STLD-{s.TripSheetNo}",
                            VDRTypeId = 1013, //New Reference
                            VoucherDetailId = vd4.Id,
                            fk_VoucherDetail = vd4
                        }
                    };
                }
                v.VoucherDetails.Add(vd4);

                #endregion

            #region Settled Advance Voucher

                if (configStatus != 3 || vd4.Amount <= 0) return;
                var advRepo = this._repository.GetRepository<TripAdvanceLog>();
                this.ExecuteSql($"UPDATE VDR SET RefID=NULL FROM dbo.tVoucherVDR VDR JOIN dbo.tVoucherVD VD ON VDR.VDId=VD.Id WHERE VD.VoucherId={(s.SetlBalVoucherId.GetValueOrDefault(0) == 0 ? "NULL" : s.SetlBalVoucherId.GetValueOrDefault().ToString())} AND VD.OrderId=2");
                var adv =
                    advRepo.Queryable()
                        .Include(x => x.fk_Voucher.VoucherDetails.Select(y => y.VoucherDetailReferences.Select(z => z.AgainstReferences)))
                        .FirstOrDefault(x => x.VoucherId == s.SetlBalVoucherId) ?? new TripAdvanceLog();
                if (adv.SettlementId.HasValue) throw new BusinessException(ErrorCode.TADV105, $"Settlement Balance Transaction Number {adv.ReferenceNo} has been settled. So you cannot update this Settlement.");
                adv.ReferenceNo = "TSBL-" + s.TripSheetNo;
                adv.AdvanceDate = s.SettleDate.Value;
                adv.VoucherNo = "TSBL-" + s.TripSheetNo;
                adv.ObjectState = adv.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                adv.FuelAmount = 0;
                adv.FuelQty = 0;
                adv.AdvanceTypeId = 17; //Trip Settlement Balance
                adv.CashAmount = vd4.Amount > 0 ? vd4.Amount : (-vd4.Amount);
                adv.OfficeId = s.OfficeId;
                adv.CreditAccountId = vd4.AccountId;
                adv.FuelRate = 0;
                adv.DebitAccountId = settledBalanceAcId;
                adv.DriverId = s.Driver1Id;
                adv.FuelId = null;
                adv.Remark =
                    "Trip Settlement Balance Carry Forwarded as driver was unable to pay cash back to company.";
                adv.TripLogId = null;
                adv.VehicleId = null;
                adv.IsBulkEntry = false;
                PrepareAdvanceV(adv);
                PrepareAdvanceVD(adv);
                foreach (var detail in adv.fk_Voucher.VoucherDetails)
                {
                    PrepareAdvanceVDR(detail, adv, vd4);
                }
                s.fk_SetlBalVoucher = adv.fk_Voucher;
                s.SetlBalVoucherId = adv.fk_Voucher.Id;
                if (adv.Id > 0)
                {
                    advRepo.Update(adv);
                }
                else
                {
                    advRepo.Insert(adv);
                }
            #endregion
            }


        }

        /// <summary>
        /// Prepares the v.
        /// </summary>
        /// <param name="advance">The advance.</param>
        /// <param name="vd4"></param>
        public void PrepareAdvanceV(TripAdvanceLog advance)
        {
            advance.fk_Voucher = advance.fk_Voucher ?? new Voucher();
            advance.fk_Voucher.OfficeId = advance.OfficeId;
            advance.fk_Voucher.VoucherNo = advance.VoucherNo;
            advance.fk_Voucher.VoucherDate = advance.AdvanceDate;
            advance.fk_Voucher.VoucherDateTime = advance.AdvanceDate;
            advance.fk_Voucher.ObjectState = advance.fk_Voucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            advance.fk_Voucher.VoucherAmount = advance.Amount;
            advance.fk_Voucher.VoucherTypeId = advance.AdvanceTypeId.GetValueOrDefault(0);
            advance.fk_Voucher.Account1Id = advance.DebitAccountId.GetValueOrDefault(0);
            advance.fk_Voucher.Account2Id = advance.CreditAccountId.GetValueOrDefault(0);
            advance.fk_Voucher.Amount1 = advance.Amount * 1;
            advance.fk_Voucher.Amount2 = advance.Amount * -1;
            advance.fk_Voucher.UserRemark = advance.Remark;
            //TODO:Setup Account Narration from Template located with VoucherType
            advance.fk_Voucher.AccountingRemark = "";
        }

        /// <summary>
        /// Prepares the vd.
        /// </summary>
        /// <param name="advance">The advance.</param>
        /// <param name="vd4"></param>
        public void PrepareAdvanceVD(TripAdvanceLog advance)
        {
            if (advance.fk_Voucher == null)
            {
                throw new BusinessException(ErrorCode.GLB106, "Unable to Create Settlement Balance Advance.Cause V:NULL");
            }
            foreach (var x in advance.fk_Voucher.VoucherDetails)
            {
                if (x.VoucherDetailReferences.Any(y => y.AgainstReferences == null || y.AgainstReferences.Count > 0))
                {
                    throw new BusinessException(ErrorCode.TADV105, "Cannot Modify Settled Trip Settlement Balance Voucher");
                }
                if (x.OrderId == 2)
                {
                    foreach (var voucherDetailReference in x.VoucherDetailReferences)
                    {
                        voucherDetailReference.RefId = null;
                        voucherDetailReference.fk_ParentReference = null;
                    }
                }
                x.ObjectState = ObjectState.Deleted;
            }
            var vdDr = new VoucherDetail()
            {
                OfficeId = advance.fk_Voucher.OfficeId,
                AccountId = advance.fk_Voucher.Account1Id.GetValueOrDefault(),
                OrderId = 1,
                Amount = advance.fk_Voucher.Amount1,
                Narration = advance.fk_Voucher.UserRemark,
                ObjectState = ObjectState.Added,
                VoucherId = advance.fk_Voucher.Id
            };
            var vdCr = new VoucherDetail()
            {
                OfficeId = advance.fk_Voucher.OfficeId,
                AccountId = advance.fk_Voucher.Account2Id.GetValueOrDefault(),
                OrderId = 2,
                Amount = advance.fk_Voucher.Amount2,
                Narration = advance.fk_Voucher.UserRemark,
                ObjectState = ObjectState.Added,
                VoucherId = advance.fk_Voucher.Id
            };
            advance.fk_Voucher.VoucherDetails.Add(vdCr);
            advance.fk_Voucher.VoucherDetails.Add(vdDr);
        }

        /// <summary>
        /// Prepares the VDR.
        /// </summary>
        /// <param name="vd">The vd.</param>
        /// <param name="advance">The advance.</param>
        /// <param name="vd4"></param>
        public void PrepareAdvanceVDR(VoucherDetail vd, TripAdvanceLog advance, VoucherDetail vd4)
        {
            vd.VoucherDetailReferences.ForEach(x => x.ObjectState = ObjectState.Deleted);
            if (vd.ObjectState == ObjectState.Added)
            {
                var isRefEnabled =
                 _repository.GetRepository<Ledger>()
                     .Queryable()
                     .Where(x => x.Id == vd.AccountId)
                     .Select(y => new { y.ReferenceFlag })
                     .FirstOrDefault();
                if (isRefEnabled == null || !isRefEnabled.ReferenceFlag) return;
                var vdr = new VoucherDetailReference()
                {
                    ObjectState = ObjectState.Added,
                    Amount = vd.Amount,
                    ReferenceNo = advance.ReferenceNo,
                    VDRTypeId = 1013,
                    VoucherDetailId = vd.Id
                };
                if (vd.OrderId == 2 && vd4.VoucherDetailReferences != null && vd4.VoucherDetailReferences.Any(x => x.ObjectState != ObjectState.Deleted))
                {
                    var vdrRef = vd4.VoucherDetailReferences.FirstOrDefault(x => x.ObjectState != ObjectState.Deleted);
                    vdr.ReferenceNo = null;
                    vdr.VDRTypeId = 1014;
                    vdr.fk_ParentReference = vdrRef;
                }
                vd.VoucherDetailReferences.Add(vdr);
            }
        }

        public async Task CreateSettlementV2(VehicleTripSettlement sat, IUnitOfWorkAsync uow)
        {
            bool isNew = sat.Id == 0;
            var advRepo = _repository.GetRepository<TripAdvanceLog>();
            var ledgerRepo = _repository.GetRepository<Ledger>().Queryable();
            var vRepo = _repository.GetRepository<Voucher>();
         
            var teRepo = _repository.GetRepository<TripExpenseLog>();
            var expTypeRepo = uow.RepositoryAsync<ExpenseMaster>();

            /*2022-07-04 Jo expense create hi nahi huye woh delete ke liye kyun aayenge server per*/
            sat.vwFuelExpenses?.RemoveAll(x => x.IsDeleted && x.Id==0);

            var v = sat.fk_Voucher ?? (sat.VoucherId > 0 ? await vRepo.Queryable().FirstOrDefaultAsync(x => x.Id == sat.VoucherId) ?? new Voucher() : new Voucher());
            var fed = sat.vwFuelExpenses.Where(x => x.IsDeleted).Select(x => x.Id).ToList();
            fed.AddRange(sat.vwTripExpenses.Where(x => x.IsDeleted).Select(x => x.Id));
            if (fed.Any())
            {
                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripExpenseLog] WHERE Id in ({fed.JoinStrings(",")})");
                sat.vwTripExpenses?.RemoveAll(x => x.IsDeleted);
                sat.vwFuelExpenses?.RemoveAll(x => x.IsDeleted);
                var invalidfe = sat.vwFuelExpenses.Where(x => x.AdvanceId == 0 || x.TripLogId.GetValueOrDefault(0) == 0).Select(x=>$"UsedQty:{x.UsedQty}[{x.Remark}]");
                if (sat.vwFuelExpenses.Any() && sat.vwFuelExpenses.Any(x => x.AdvanceId == 0 || x.TripLogId.GetValueOrDefault(0)==0)) throw new BusinessException(ErrorCode.TS103, $"Few Fuel Stock Consumption are either not mapped to Trip or are not Mapped to Any Fuel Stock Entry.{Environment.NewLine}{(string.Join(",",invalidfe))}");
            }
            
            #region NetBalance Vd
            bool RefFlag = false;
            long DefaultTruckControlAccountId = 0;
            var netbalance = sat.FuelAmountDifference/*Value<0 Pay Value>0 Receive*/ + sat.SettledAmount /*Value<0 Pay Value>0 Receive*/- sat.CashDeposited/*Always Value>0*/+sat.CashPaid;
            var SettPayoffRule = _repository.GetConfigValue<long>("SettlementNetBalancePayoffRule");/*0:Payoff,1:MaintainButNoAdvance, 2: MaintainInSettledAc, 3: MaintainInOtherThenSettlement*/
            var netbalanceadvances = _repository.GetConfigValue<long>("CreateSettlementNetBalanceAmount");
            var DefaultSettlementNetBalancePayoffAccount = _repository.GetConfigValue<long>("DefaultSettlementNetBalancePayoffAccount");
            if (SettPayoffRule == 0/*0:PayOff*/)
            {
                sat.NetBalancePending = false;
            }
            if (SettPayoffRule == 3 && DefaultSettlementNetBalancePayoffAccount == 0)
            {
                throw new BusinessException(ErrorCode.GLB103, "Missing Default Settlement NetBalance Payoff Account");
            }
            var vd5 = new VoucherDetail();
            var settlacctype = _repository.GetConfigValue<long>("SettledAccountType");
            var generateCashPaidAdvance = _repository.GetConfigValue<long>("GenerateCashPaidAdvance");
            var tladvmappingflag= _repository.GetClientConfigValue<long>("ShowAutoTripOnAdvance");
            switch (settlacctype)
            {
                case 0:
                    if (sat.SettlementAccountId != null)
                    {
                        //2:Auto Only Cash Account and Control Account are Allowed
                        var zero = await ledgerRepo.Where(x => x.Id == sat.SettlementAccountId).Select(x => new
                        {
                            x.Id,
                            x.ReferenceFlag
                        }).FirstOrDefaultAsync();
                        if (zero == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Wrong Settled Account Name Selected");
                        }
                        DefaultTruckControlAccountId = zero.Id;
                        RefFlag = zero.ReferenceFlag;
                    }
                    break;

                case 1:
                    //1:Driver//Credit or Debit could be only done in Driver Ledger
                    var one = await ledgerRepo.Where(x => x.Id == sat.Driver1Id).Select(x => new
                    {
                        x.Id,
                        x.ReferenceFlag
                    }).FirstOrDefaultAsync();
                    if (one == null)
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Only Driver can be selected in Settled Account Name");
                    }
                    DefaultTruckControlAccountId = one.Id;
                    RefFlag = one.ReferenceFlag;
                    break;
                case 3:
                    //1:Driver//Credit or Debit could be only done in DefaultControlAccount
                    DefaultTruckControlAccountId = _repository.GetConfigValue<long>("DefaultSettledAccountId");
                    if (DefaultTruckControlAccountId == 0)
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Default truck control account not configured");
                    }
                    var three = await ledgerRepo.Where(x => x.Id == DefaultTruckControlAccountId).Select(x => new
                    {
                        x.Id,
                        x.ReferenceFlag
                    }).FirstOrDefaultAsync();
                    if (three == null)
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Invalid Truck Control account configured ");
                    }
                    RefFlag = three.ReferenceFlag;
                    break;
            }

            if (DefaultTruckControlAccountId == 0)
            {
                DefaultTruckControlAccountId = _repository.GetConfigValue<long>("DefaultSettledAccountId");
            }

            if (netbalance != 0)
            {   
                vd5 = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = sat.SettlementAccountId.GetValueOrDefault()>0? sat.SettlementAccountId.GetValueOrDefault():DefaultTruckControlAccountId,//netbalance<0?/*If Pay[Negative]*/sat.SettlementAccountId.GetValueOrDefault() :/*If Receive[Possitive]*/ DefaultTruckControlAccountId,
                    Amount = netbalance,
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 5,
                    VoucherId = v.Id
                };
                v.VoucherDetails.Add(vd5);
                if (vd5.AccountId > 0 && await _repository.GetRepository<Ledger>().Queryable().AnyAsync(x => x.Id == vd5.AccountId && x.ReferenceFlag))
                {
                    var vdr5 = new VoucherDetailReference()
                    {
                        Amount = vd5.Amount,
                        ObjectState = ObjectState.Added,
                        VDRTypeId = 1013,   //new Reference
                        VoucherDetailId = vd5.Id,
                        fk_VoucherDetail = vd5,
                        ReferenceNo = $"{sat.TripSheetNo}-BAL",
                        AccountId = vd5.AccountId,
                        DueDate = sat.SettleDate ?? sat.EndDate ?? DateTime.Now
                    };
                    vd5.VoucherDetailReferences.Add(vdr5);
                }
            }
            #endregion

            #region Deleting Old Voucher

            if (!isNew)
            {
                if(sat.CashPaidAdvId.GetValueOrDefault()>0&&await advRepo.Queryable().AnyAsync(x=>x.Id== sat.CashPaidAdvId&&x.RequestStatusId==1597))
                {
                    throw new BusinessException(ErrorCode.TADV108, "The Balance TripAdvance for this Trip Settlement has been Disburshed.");
                }
                
                /*Delete All the VD of Settlement*/
                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVoucherVD] WHERE VoucherId={v.Id}");
                /**/
                var balvids = new List<long?>() { sat.SetlBalFuelVoucherId.GetValueOrDefault(), sat.SetlBalVoucherId.GetValueOrDefault(), sat.NetBalVoucherId.GetValueOrDefault() }.Where(x => x > 0).ToList();

                if (balvids.Any())
                {
                    if(await advRepo.Queryable().AnyAsync(x => x.VoucherId>0 && x.SettledAdvances.Any() && balvids.Contains(x.VoucherId)))
                    {
                        throw new BusinessException(ErrorCode.TADV106, "Balance of this settlement has been settled in any other settlement or has been reversed");
                    }                    
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripAdvanceLog] WHERE VoucherId IN ({(balvids.JoinStrings(","))})");
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripSettlement] SET SetlBalFuelVoucherId=NULL, SetlBalVoucherId=NULL, NetBalVoucherId=NULL,CashPaidAdvId=NULL WHERE Id={sat.Id}");
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVouchers] WHERE Id IN ({balvids.JoinStrings(",")})");
                }

                if (sat.CashPaidAdvId.GetValueOrDefault() > 0)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripSettlement] SET CashPaidAdvId=NULL WHERE Id={sat.Id}");
                    var voucherid = advRepo.Queryable().Where(x => x.Id == sat.CashPaidAdvId).Select(x => x.VoucherId).FirstOrDefault();
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripAdvanceLog] WHERE Id=@p0", sat.CashPaidAdvId);
                    if (voucherid > 0)
                    {
                        await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVouchers] WHERE Id=@p0", voucherid);
                    }
                }
                sat.SetlBalFuelVoucherId = null;
                sat.SetlBalVoucherId = null;
                sat.NetBalVoucherId = null;
                sat.CashPaidAdvId = null;
            }

            #endregion

            #region Trip Expenses
            var existingexpids = sat.vwTripExpenses.Where(x => x.Id > 0&&!x.IsDeleted).Select(y => y.Id).Distinct().ToList();
            //if (!isNew)
            //{
            //    var todbedeleted= sat.vwTripExpenses.Where(x => x.Id > 0 && x.IsDeleted).Select(y => y.Id).Distinct().ToList();
            //    var exists = existingexpids.JoinStrings(",");
            //    string query = "";
            //    var both = (existingexpids.Any() || todbedeleted.Any());
            //    var p1 = existingexpids.Any();
            //    var p2 = todbedeleted.Any();
            //    var conddelete = $"{(both ? "AND(":"")}{(p1?$"Id not in ({ exists})" : "")} {(both?" OR ":"")} {(p2 ? $"Id in ({  todbedeleted.JoinStrings(",")})" : "")}{(both ? ")" : "")}";
            //    await uow.ExecSqlQueryAsync($"DELETE  FROM [dbo].[tTripExpenseLog] WHERE SettlementId={sat.Id} AND IsBudgeted<>1 {conddelete} AND NOT EXISTS(SELECT 1 FROM tTripAdvanceLog tl WHERE ISNULL([dbo].[tTripExpenseLog].TripAdvanceLogId,0)=tl.Id AND tl.AdvanceTypeId=3)");
            //}
            var expansetypeids = sat.vwTripExpenses?.Where(x => !x.IsDeleted).Select(x => x.TypeId).Distinct().ToList();
            var expaccounts = await expTypeRepo.Queryable().Where(x => expansetypeids.Contains(x.Id)).Select(x => new
            {
                x.LedgerId,
                x.Id,
                x.NatureId
            }).Distinct().ToListAsync();
            if (expaccounts.Any())
            {
                sat.vwTripExpenses?.Where(x => !x.IsDeleted).ToList().ForEach(x =>
                {
                    var acid = expaccounts.FirstOrDefault(y => y.Id == x.TypeId);
                    x.AccountId = acid?.LedgerId;
                    x.ExpNatureId = acid?.NatureId;
                });
            }
            
            var existingexps = existingexpids.Any() ? await teRepo.Queryable().Where(x => existingexpids.Contains(x.Id)).ToListAsync() : null;
            foreach (var item in sat.vwTripExpenses.Where(x => !x.IsDeleted))
            {  
                /* Prepare TripExpenseLog for all received Expense*/
                var texp = (item.Id > 0?existingexps.FirstOrDefault(x=>x.Id==item.Id):null)??new TripExpenseLog();

                //if (item.Id > 0 && existingexps.Any(x => x.Id == item.Id))
                //{
                //    texp.Id = item.Id;
                //    teRepo.Update(texp);
                //    var entry = this._repository.UOW.Context.Entry(texp);
                //    entry.State = EntityState.Modified;
                //    entry.Property("RowVersion").OriginalValue = item.RowVersion;
                //    //texp.RowVersion = item.RowVersion;
                //    if (texp.RowVersion == null)
                //    {
                //        var db = existingexps.FirstOrDefault(x => x.Id == texp.Id)?.RowVersion; ;
                //        entry.Property("RowVersion").OriginalValue = db;
                //        //texp.RowVersion = db;

                //    }
                //}
                if(texp.Id <=0)
                {
                    texp.ObjectState = ObjectState.Added;
                    teRepo.Insert(texp);
                }
                else
                {
                    texp.ObjectState = ObjectState.Modified;
                    teRepo.Update(texp);
                }
                texp.TripLogId = item.TripLogId;
                texp.SettlementId = sat.Id;
                if (!texp.IsBudgeted)
                {
                    texp.ClaimAmount = item.ClaimAmt;
                }
                texp.SettledAmount = item.SettledAmt;
                texp.Remarks = item.Remark;
                texp.ExpenseTypeId = item.TypeId;
                texp.TripAdvanceLogId = item.TripAdvanceLogId;
                texp.FuelRate = item.Rate;
                texp.FuelQty = item.FuelQty;
                texp.ViewId = sat.ViewId;
                texp.ObjectState = texp.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                sat.TripExpenses.Add(texp);
                if (sat.vwTripLogs.All(x => !x.IsDeleted && x.Id != texp.TripLogId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "One of trip expense has triplog which is not getting settled in this settlement");
                }
            }

            if (isNew)
            {
                sat.ObjectState = ObjectState.Added;
                _repository.Insert(sat);
                await uow.SaveChangesAsync();
            }
            else
            {
                sat.ObjectState = ObjectState.Modified;
                _repository.Update(sat);
                await uow.SaveChangesAsync();
            }
            #endregion
            #region Fuel Expanses Mapping
            #region Start new code for Fuel Stock
                var expenseMaster = await uow.RepositoryAsync<ExpenseMaster>().Queryable().FirstOrDefaultAsync(x => x.NatureId == 1479);
            
                if (expenseMaster == null) {
                    throw new BusinessException(ErrorCode.TS103, $"Expense Master with nature Fuel is not defined.");
                }

                if (expenseMaster.LedgerId.GetValueOrDefault() <= 0) {
                    throw new BusinessException(ErrorCode.TS103, $"Expense Master is not mapped with Expense Ledger Hint.Expense Name:{expenseMaster.Name}");
                }

                var expadvIds =
                    sat.vwFuelExpenses.Where(x => (x.ShortageQty + x.UsedQty) > 0)
                        .Select(x => x.AdvanceId)
                        .Distinct().ToList();
                var feAdvances = await advRepo.Queryable().Include(f => f.FuelExpanses.Select(y => y.fk_ExpenseType.fk_Ledger)).Where(x => expadvIds.Contains(x.Id)).ToListAsync();
                foreach (var fsac in feAdvances.GroupBy(x=>x.DebitAccountId))//each debit account in Fuel Stock Advances
                {
                    /*VD for Fuel Exp*/
                    var vd = new VoucherDetail()
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = fsac.Key.GetValueOrDefault(),
                        OrderId = 7,                   
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id,
                        Voucher = v
                    };
                    v.VoucherDetails.Add(vd);
                    var fe_expvd = new VoucherDetail()
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = expenseMaster.LedgerId.GetValueOrDefault(),//?? Expense Account Kaise Pata kare
                        OrderId = 9,
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id
                    };

                    ////var fe_shortagevd = new VoucherDetail()
                    ////{
                    ////    OfficeId = sat.OfficeId,
                    ////    AccountId = sat.Driver1Id.GetValueOrDefault(),
                    ////    OrderId = 10,
                    ////    ObjectState = ObjectState.Added,
                    ////    VoucherId = v.Id
                    ////};
               
                    /*
                     VD1 Control A/c Credit 3000
                     VDRS => All the Against Refs 2000 4 Against
                     VDR => All the Non Against Ref 1000
                    VD2 ExpenseA/c 2500
                    Vd3 Driver Shortage A/c 500
                
                     */
                    foreach (var ad in fsac)//each advance in debit account group
                    {
                        var expenses = sat.vwFuelExpenses.Where(x => x.AdvanceId== ad.Id);
                        var shortageamt = expenses.Where(x => !x.IsDeleted).Sum(x => x.ShortageAmt);
                        var totalexpamt = expenses.Where(x => !x.IsDeleted).Sum(x => x.UsedAmt + x.ShortageAmt);
                        vd.Amount += -totalexpamt;
                        fe_expvd.Amount += totalexpamt - shortageamt;
                       //if(shortageamt>0) fe_shortagevd.Amount += shortageamt;
                    }
                //if (fe_shortagevd.Amount > 0) {
                //    fe_shortagevd.Voucher = v;
                //    v.VoucherDetails.Add(fe_shortagevd);                    
                //}
                if (fe_expvd.Amount > 0) {
                    fe_expvd.Voucher = v;
                    v.VoucherDetails.Add(fe_expvd); 
                }
                }
            #endregion
            foreach (IGrouping<long, FuelExpense> expanses in sat.vwFuelExpenses.Where(x => (x.ShortageQty + x.UsedQty) > 0).GroupBy(x => x.AdvanceId))
            {
                var shortageamt = expanses.Where(x=>!x.IsDeleted).Sum(x => x.ShortageAmt);
                var totalexpamt = expanses.Where(x => !x.IsDeleted).Sum(x => x.UsedAmt + x.ShortageAmt);

                var adv = sat.TripAdvances.Any(a => a.Id == expanses.Key) ? sat.TripAdvances.First(a => a.Id == expanses.Key) : feAdvances.FirstOrDefault(a => a.Id == expanses.Key);
                //Through Error if Provided AdvanceId is Wrong
                if (adv == null)
                {
                    throw new BusinessException(ErrorCode.TS103, $"Advance Reference Mentioned in one of Fuel Expense is wrong. Hint: AdvKey_{expanses.Key}");
                }
                ////If Fuel Expanses is empty for advance then retry to fatch them direct from DataBase
                //if (adv.FuelExpanses == null || !adv.FuelExpanses.Any())
                //{
                //    var exps = expRepo.Queryable().Include(z => z.fk_ExpenseType.fk_Ledger).Where(z => z.TripAdvanceLogId == adv.Id);
                //    if (exps.Any())
                //    {
                //        adv.FuelExpanses = new List<TripExpenseLog>();
                //        adv.FuelExpanses.AddRange(exps);
                //    }
                //}
                if (adv.FuelExpanses != null && adv.FuelExpanses.Any())
                {
                    //var tls = settlement.TripLogs.Where(x=>x.SettlementId.HasValue).Select(x => x.Id);
                    var tls = sat.TripLogs.Select(x => x.Id);
                    foreach (var x in adv.FuelExpanses.Where(x => !tls.Contains(x.TripLogId)))
                    {
                        x.ObjectState = ObjectState.Unchanged;
                    }
                }

                foreach (FuelExpense expanse in expanses)
                {
                    var exp = adv.FuelExpanses.FirstOrDefault(x => x.Id == expanse.Id);
                    if (exp == null)
                    {
                        exp = teRepo.Queryable().Include(z => z.fk_ExpenseType.fk_Ledger).FirstOrDefault(a => a.Id == expanse.Id && a.TripAdvanceLogId == adv.Id);
                        if (exp == null)
                        {
                            exp = new TripExpenseLog();
                        }
                    }
                    if (expanse.IsDeleted)
                    {
                        exp.SettlementId = null;
                        exp.fk_Settlement = null;
                        exp.fk_TripAdvanceLog = null;
                        exp.fk_TripAdvanceLog = null;
                        exp.fk_TripLog = null;
                        exp.fk_TripLog = null;
                        exp.ObjectState = ObjectState.Deleted;

                        adv.BalanceQty = adv.FuelExpanses.Where(a => a.ObjectState != ObjectState.Deleted).Sum(a => a.FuelQty + a.ShortFuelQty);
                        adv.ObjectState = ObjectState.Modified;

                        teRepo.Delete(exp);
                    }
                    else
                    {
                        decimal otherConsume = 0;
                        if (adv.FuelExpanses != null && adv.FuelExpanses.Any())
                        {
                            otherConsume = expanse.Id > 0 ? adv.FuelExpanses.Where(a => a.Id != expanse.Id).Sum(r => r.FuelQty + r.ShortFuelQty) : adv.FuelExpanses.Sum(r => r.FuelQty + r.ShortFuelQty);
                        }

                        if (adv.FuelQty <= otherConsume)
                            throw new BusinessException(ErrorCode.TS103, $"The Total Fuel Qty is already adjusted against advancelog id:{expanse.AdvanceId}");
                        
                        if ((expanse.UsedQty + expanse.ShortageQty) > (adv.FuelQty - otherConsume))
                            throw new BusinessException(ErrorCode.TS103, $"The Consumed Fuel Qty {expanse.UsedQty + expanse.ShortageQty} Exceeded than Balance Qty {adv.FuelQty - otherConsume} against advancelog no:{adv.ReferenceNo}");
                        
                        exp.FuelQty = expanse.UsedQty;
                        exp.TripAdvanceLogId = expanse.AdvanceId;
                        exp.fk_TripAdvanceLog = adv;
                        exp.Remarks = expanse.Remark;
                        exp.SettledAmount = expanse.UsedAmt;
                        exp.SettlementId = sat.Id;
                        exp.fk_Settlement = sat;
                        exp.ObjectState = exp.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        exp.ClaimAmount = 0;
                        exp.TripLogId = expanse.TripLogId.GetValueOrDefault(0);
                        exp.ShortFuelQty = expanse.ShortageQty;
                        exp.ShortFuelAmt = expanse.ShortageAmt;
                        adv.BalanceQty = adv.FuelQty - adv.FuelExpanses.Where(a => a.ObjectState != ObjectState.Deleted).Sum(a => a.FuelQty + a.ShortFuelQty);
                        adv.ObjectState = ObjectState.Modified;
                        //Through Error if Provided AdvanceId is not mapped with any TripLog in currant Settlement
                        if (sat.vwTripLogs.Where(x=>!x.IsDeleted).All(e => e.Id != expanse.TripLogId.GetValueOrDefault(0)))
                        {
                            throw new BusinessException(ErrorCode.TS103, $"TripLogId {expanse.TripLogId} is wrong against advance id:{expanse.AdvanceId}");
                        }

                        if (exp.ExpenseTypeId <= 0)
                        {
                            exp.ExpenseTypeId= _repository.GetConfigValue<long>("DefaultFuelStockExpenseId");
                            if (exp.ExpenseTypeId <= 0)
                            {
                                throw new BusinessException(ErrorCode.GLB103, "Default Fuel Stock Consumption Expense name not Configured Hind:Key=>DefaultFuelStockExpenseId");
                            }
                        }

                        if (sat.TripExpenses.All(x => x.Id != exp.Id||exp.Id==0))
                        {
                            sat.TripExpenses.Add(exp);
                            if (exp.Id > 0)
                            {
                                teRepo.Update(exp);
                            }
                            else
                            {
                                teRepo.Insert(exp);
                            }
                        }
                    }
                    adv.BalanceQty =adv.FuelQty- adv.FuelExpanses.Where(a => a.ObjectState != ObjectState.Deleted).Sum(a => a.FuelQty + a.ShortFuelQty);
                    adv.ObjectState = ObjectState.Modified;
                    advRepo.Update(adv);

                    //if (Math.Abs(totalexpamt) != 0)
                    //{
                    //    var fuelexpvd = new VoucherDetail
                    //    {
                    //        OfficeId = sat.OfficeId,
                    //        AccountId = adv.DebitAccountId.GetValueOrDefault(),
                    //        OrderId = 7,
                    //        Amount = -totalexpamt,
                    //        ObjectState = ObjectState.Added,
                    //        VoucherId = v.Id,
                    //        Voucher = v
                    //    };
                    //    v.VoucherDetails.Add(fuelexpvd);
                    //}
                }
            }
            #endregion
            #region Advance Reveresal
            await uow.ExecSqlQueryAsync($"UPDATE [tTripAdvanceLog] SET SettlementId=NULL {(tladvmappingflag > 0 ?"": " ,TripLogId=NULL ")} WHERE SettlementId=@id AND AdvanceTypeId=94", new SqlParameter("id", sat.Id));
            var reverse = sat.vwTripAdvances?.Where(x => !x.IsDeleted && x.TypeId == 94).ToList();
            if (reverse.Any())
            {
                foreach (var r in reverse)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [tTripAdvanceLog] SET SettlementId={sat.Id},TripLogId={r.TripLogId} WHERE Id={r.Id} AND AdvanceTypeId=94");
                }
                
            }
            #endregion
            #region 1:Driver Cash Advances/Settled Balances Type Advances 1590: driver Cash Advance

            var cashadvids = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 1 || x.TypeId == 17 || x.TypeId == 16 || x.TypeId == 91)).Select(x => x.Id).ToList() ?? new List<long>();
            var cashadvances = cashadvids.Any() ? await advRepo.Queryable().Include(x => x.SettledAdvances).Where(x => cashadvids.Contains(x.Id)).ToListAsync() : new List<TripAdvanceLog>();
            if (cashadvids.Any())//Cash Advance and Cash Settlement Balance
            {
                foreach (var loggroup in cashadvances.GroupBy(x => x.DebitAccountId))
                {
                    var settledamt = loggroup.SelectMany(x => x.SettledAdvances ?? new List<TripAdvanceLog>(), (p, c) => new { c.CashAmount }).Sum(x => x.CashAmount);

                    var vd = new VoucherDetail
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = loggroup.Key.GetValueOrDefault(),
                        OrderId = 1,
                        Amount = -(loggroup.Sum(x => x.CashAmount) - settledamt),
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id,
                        Voucher = v
                    };
                    decimal onaccount = 0;
                    foreach (var log in loggroup)
                    {
                        var sadv = sat.vwTripAdvances.FirstOrDefault(x => x.Id == log.Id);
                        if (log.SettlementId > 0 && log.SettlementId != sat.Id)
                        {
                            throw new BusinessException(ErrorCode.TADV106, $"Reference No {log.ReferenceNo}");
                        }
                        log.SettlementId = sat.Id;
                        log.TripLogId = sadv?.TripLogId;
                        log.ObjectState = ObjectState.Modified;
                        if (log.VDRId > 0)
                        {
                            var vdr = new VoucherDetailReference()
                            {
                                Amount = -(log.CashAmount - log.SettledAdvances.Sum(x => x.CashAmount)),
                                ObjectState = ObjectState.Added,
                                RefId = log.VDRId,
                                VDRTypeId = 1014,   //Against Reference
                                VoucherDetailId = vd.Id,
                                fk_VoucherDetail = vd,
                                ReferenceNo = log.ReferenceNo,
                                AccountId = vd.AccountId,
                                DueDate = sat.SettleDate ?? log.AdvanceDate
                            };
                            vd.VoucherDetailReferences.Add(vdr);
                        }
                        else
                        {
                            onaccount += (log.CashAmount - log.SettledAdvances.Sum(x => x.CashAmount));
                        }
                        sat.TripAdvances.Add(log);
                    }

                    if (onaccount > 0)
                    {
                        var onaccountvdr = new VoucherDetailReference()
                        {
                            Amount = -onaccount,
                            ObjectState = ObjectState.Added,
                            RefId = null,
                            VDRTypeId = 1448,   //On Account
                            VoucherDetailId = vd.Id,
                            fk_VoucherDetail = vd,
                            ReferenceNo = null,
                            AccountId = vd.AccountId,
                            DueDate = sat.SettleDate ?? DateTime.Now
                        };
                        vd.VoucherDetailReferences.Add(onaccountvdr);
                    }

                    if (vd.VoucherDetailReferences.Any() && vd.Amount != vd.VoucherDetailReferences.Sum(x => x.Amount))//I am facing error 
                    {

                        throw new BusinessException(ErrorCode.VCH106, $"[ErrorSource:VTSS]Few Cash Advance/SettledBalance are not having Against Ref Transaction. Hint: OrderId:{vd.OrderId},AccountId:{vd.AccountId}");
                    }
                    v.VoucherDetails.Add(vd);
                }
            }
            if (!isNew)
            {
                await uow.ExecSqlQueryAsync(
                    $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId=NULL  {(tladvmappingflag > 0 ? "" : " ,TripLogId=NULL ")} WHERE SettlementId={sat.Id} AND AdvanceTypeId in(1,17,16,91) {(cashadvids.Any() ? $"AND Id NOT IN({cashadvids.JoinStrings(",")})" : "")}");
            }
            #endregion

            #region 88:e-Toll Payment

            var etollexpmaster = await uow.RepositoryAsync<ExpenseMaster>().Queryable().FirstOrDefaultAsync(x => x.NatureId == 1617);

            if (etollexpmaster == null)
            {
                throw new BusinessException(ErrorCode.TS103, $"eToll Expense Master is not defined.");
            }

            if (etollexpmaster.LedgerId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.TS103, $"eToll Expense Master is not mapped with Expense Ledger Hint.Expense Name:{etollexpmaster.Name}");
            }

            var eTotaladvids = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 88)).ToList() ?? new List<Advance>();
            if (eTotaladvids.Any())
            {
                if (sat.vwTripAdvances.Any(x => !x.IsDeleted && x.TypeId == 88 && x.TripLogId.GetValueOrDefault() == 0))
                {
                    throw new BusinessException(ErrorCode.GLB106, "One of eToll is not mapped to any of TripLog included this settlement.");
                }

                var etollids = eTotaladvids.Select(y => y.Id).ToList();
                var etoll = (await advRepo.Queryable().Where(x => etollids.Contains(x.Id))
                    .SumAsync(x => (decimal?)x.CashAmount)) ?? 0;
                var etollexp = sat.vwTripExpenses.Where(x => !x.IsDeleted && x.ExpNatureId == 1617)
                    .Sum(x => (decimal?)x.SettledAmt) ?? 0;
                if (etollexp !=
                    etoll)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"eToll Expense Amount does not match with sum of eToll Payments included in this Settlement.\n eToll Expense Amount is Rs.{etollexp} and eToll Payment Total is Rs.{etoll}");
                }
                var etollidscomma = etollids.JoinStrings(",");
                await _repository.UOW.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET TripLogId=NULL,SettlementId=NULL WHERE SettlementId=@arg1 and AdvanceTypeId=88 and Id not in({etollidscomma})", new SqlParameter("arg1", sat.Id));


                foreach (var l in eTotaladvids.GroupBy(x => x.TripLogId))
                {
                    var ids = l.Select(x => x.Id).ToArray().JoinStrings(",");
                    await _repository.UOW.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET TripLogId=@arg1,SettlementId=@arg2 WHERE AdvanceTypeId=88 AND Id in({ids})", new SqlParameter("arg1", ((object)l.Key ?? DBNull.Value)), new SqlParameter("arg2", sat.Id));
                }
                var etolladv = advRepo.Queryable().Where(x => etollids.Contains(x.Id)).ToList();
                foreach (var fsac in etolladv.GroupBy(y => y.DebitAccountId))
                {
                    /*VD for eToll Control A/c (-)*/
                    var vd = new VoucherDetail()
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = fsac.Key.GetValueOrDefault(),
                        OrderId = 11,
                        Amount=-fsac.Sum(x=>x.Amount),
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id,
                        Voucher = v
                    };
                    v.VoucherDetails.Add(vd);
                }
                var etoll_expvd = new VoucherDetail()
                {
                    OfficeId = sat.OfficeId,
                    AccountId = etollexpmaster.LedgerId.GetValueOrDefault(),//?? Expense Account Kaise Pata kare
                    OrderId = 12,
                    ObjectState = ObjectState.Added,
                    VoucherId = v.Id,
                    Voucher = v,
                    Amount=etoll
                };
                v.VoucherDetails.Add(etoll_expvd);
            }
            #endregion

            #region 6:Driver Settlement Net Balance Payoff 

            var cashnetbalances = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 86));
            var drvsetnetbalpayoff = cashnetbalances.Select(x => x.Id).ToList() ?? new List<long>();
            var query86 = advRepo.Queryable().Where(x => drvsetnetbalpayoff.Contains(x.Id) && (!x.SettledAdvances.Any() || (x.CashAmount - x.SettledAdvances.Sum(y => y.CashAmount + y.FuelAmount)) > 0));
            var settnetbalpayoff = drvsetnetbalpayoff.Any() ? await query86.ToListAsync() : new List<TripAdvanceLog>();
            var balances86 = drvsetnetbalpayoff.Any() ?await query86.Select(x => new { x.Id,x.CreditAccountId, Amount = (x.FuelQty >0 ? x.FuelAmount : x.CashAmount), SettledAmt = x.SettledAdvances.Sum(y => (decimal?)(y.FuelQty > 0 ? y.FuelAmount : y.CashAmount))??0 }).ToListAsync() : null;
            if (drvsetnetbalpayoff.Count() != settnetbalpayoff.Count())
            {
                throw new BusinessException(ErrorCode.GLB106, "One or more Driver Net balance deposit was not found or was partially intiated using Driver Deposit Refund.");
            }
            if (drvsetnetbalpayoff.Any())
            {
                foreach (var loggroup in settnetbalpayoff.GroupBy(x => x.CreditAccountId))
                {
                    var firstQuery = balances86?.Where(x => x.CreditAccountId == loggroup.Key);
                    var vd = new VoucherDetail()
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = loggroup.Key.GetValueOrDefault(),
                        OrderId = 6,
                        Amount = /*loggroup.Sum(x => x.CashAmount)-*/ (firstQuery?.Sum(x => x.Amount - x.SettledAmt) ?? loggroup.Sum(x => x.CashAmount)),
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id,
                        Voucher = v
                    };
                    decimal onaccount = 0;
                    foreach (var log in loggroup)
                    {
                        var first = balances86?.FirstOrDefault(x => x.Id == log.Id);
                        var sadv = sat.vwTripAdvances.FirstOrDefault(x => x.Id == log.Id);
                        if (log.SettlementId > 0 && log.SettlementId != sat.Id)
                        {
                            throw new BusinessException(ErrorCode.TADV106, $"Reference No {log.ReferenceNo}");
                        }
                        log.SettlementId = sat.Id;
                        log.TripLogId = sadv?.TripLogId;
                        log.ObjectState = ObjectState.Modified;
                        if (log.VDRId > 0)
                        {
                            var vdr = new VoucherDetailReference()
                            {
                                Amount = (first?.Amount-first?.SettledAmt)??log.Amount,
                                ObjectState = ObjectState.Added,
                                RefId = log.VDRId,
                                VDRTypeId = 1014,   //Against Reference
                                VoucherDetailId = vd.Id,
                                fk_VoucherDetail = vd,
                                ReferenceNo = log.ReferenceNo,
                                AccountId = vd.AccountId,
                                DueDate = sat.SettleDate ?? log.AdvanceDate
                            };
                            vd.VoucherDetailReferences.Add(vdr);
                        }
                        else
                        {
                            onaccount += (first?.Amount - first?.SettledAmt) ?? log.Amount;
                        }
                        sat.TripAdvances.Add(log);
                    }
                    if (onaccount > 0 && vd.VoucherDetailReferences.Any())
                    {
                        var onaccountvdr = new VoucherDetailReference()
                        {
                            Amount = -onaccount,
                            ObjectState = ObjectState.Added,
                            RefId = null,
                            VDRTypeId = 1448,   //On Account
                            VoucherDetailId = vd.Id,
                            fk_VoucherDetail = vd,
                            ReferenceNo = null,
                            AccountId = vd.AccountId,
                            DueDate = sat.SettleDate ?? DateTime.Now
                        };
                        vd.VoucherDetailReferences.Add(onaccountvdr);
                    }
                    if (vd.VoucherDetailReferences.Any() && vd.Amount != vd.VoucherDetailReferences.Sum(x => x.Amount))
                    {
                        throw new BusinessException(ErrorCode.VCH106, "[ErrorSource:VTSS]Few NetBalance Cash Deposit Payoff's are not having Against Ref Transaction.");
                    }
                    v.VoucherDetails.Add(vd);
                }
            }
            if (!isNew)
            {
                await uow.ExecSqlQueryAsync(
                    $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId=NULL  {(tladvmappingflag > 0 ? "" : " ,TripLogId=NULL ")}  WHERE SettlementId={sat.Id} AND AdvanceTypeId=86 {(drvsetnetbalpayoff.Any() ? $"AND Id NOT IN({drvsetnetbalpayoff.JoinStrings(",")})" : "")}");
            }
            #endregion

            #region 2: Driver Fuel Advances: 1591 & Urea Issue (112 & 110)
            var fueladv = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 2|| x.TypeId == 112 || x.TypeId == 110 || x.TypeId == 85)).Select(x => x.Id).ToList() ?? new List<long>();
            if (!isNew)//iF SETTLEMENT IS GETTING UPDATED THAN UNMAP ALL FUEL ADVANCES THOSE WERE PREVIOUSALY MAPPED BUT NOT NOW
            {
                await uow.ExecSqlQueryAsync(
                $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId=NULL  {(tladvmappingflag > 0 ? "" : " ,TripLogId=NULL ")}  WHERE SettlementId={sat.Id} AND AdvanceTypeId in(2,85) {(fueladv.Any() ? $"AND Id NOT IN({fueladv.JoinStrings(",")})" : "")}");
            }
            if (fueladv.Any())
            {                
                var fueladvs = await advRepo.Queryable().Include(x => x.SettledAdvances).Where(x => fueladv.Contains(x.Id)).ToListAsync();                
                
                var crfueladv = fueladvs.Where(x => x.FuelAmount-x.SettledAdvances.Sum(z=>z.FuelAmount) > sat.vwTripExpenses.Where(y => y.TripAdvanceLogId == x.Id).Sum(y => y.SettledAmt)).ToList();
                var drfueladv = fueladvs.Where(x => x.FuelAmount - x.SettledAdvances.Sum(z => z.FuelAmount) < sat.vwTripExpenses.Where(y => y.TripAdvanceLogId == x.Id).Sum(y => y.SettledAmt)).ToList();
                if (crfueladv.Any())//If Fuel Paid was greater than fuel expense
                {
                    foreach (var fuelgroup in crfueladv.GroupBy(x => x.DebitAccountId))
                    {
                        var settledamt = fuelgroup.SelectMany(x => x.SettledAdvances ?? new List<TripAdvanceLog>(), (p, c) => new { c.FuelAmount }).Sum(x => x.FuelAmount);
                        var tobesettledexp = sat.vwTripExpenses.Where(x => fuelgroup.Select(y => (long?)y.Id).Contains(x.TripAdvanceLogId));
                        var amt1 = fuelgroup.Sum(x => x.FuelAmount)- settledamt;
                        var amt2 = tobesettledexp.Sum(x => x.SettledAmt);
                        var vd = new VoucherDetail()
                        {
                            OfficeId = sat.OfficeId,
                            AccountId = fuelgroup.Key.GetValueOrDefault(),
                            OrderId = 2,
                            Amount = -(amt1 - amt2), //-(fuelgroup.Sum(x => x.FuelAmount)-tobesettledexp.Sum(x=>x.SettledAmt)),
                            ObjectState = ObjectState.Added,
                            VoucherId = v.Id,
                            Voucher = v
                        };
                        foreach (var log in fuelgroup)
                        {
                            var sadv = tobesettledexp.FirstOrDefault(x => x.TripAdvanceLogId == log.Id);
                            var existingadv = sat.vwTripAdvances.FirstOrDefault(x => x.Id == log.Id);
                            if (log.SettlementId > 0 && log.SettlementId != sat.Id)
                            {
                                throw new BusinessException(ErrorCode.TADV106, $"Reference No {log.ReferenceNo}");
                            }
                            log.SettlementId = sat.Id;
                            if (existingadv?.TripLogId > 0)
                            {
                                log.TripLogId = existingadv.TripLogId;
                            }
                            log.ObjectState = ObjectState.Modified;
                            if (log.VDRId > 0)
                            {
                                var vdramt1 = log.FuelAmount;
                                var vdramt2 = sadv?.SettledAmt ?? 0;
                                var vdr = new VoucherDetailReference()
                                {
                                    Amount = -(vdramt1 - vdramt2), //-(log.FuelAmount- sadv?.SettledAmt??0),
                                    RefId = log.VDRId,
                                    VDRTypeId = 1014,//Against Reference
                                    VoucherDetailId = vd.Id,
                                    fk_VoucherDetail = vd,
                                    ReferenceNo = log.ReferenceNo,
                                    AccountId = vd.AccountId,
                                    DueDate = sat.SettleDate ?? log.AdvanceDate,
                                    ObjectState = ObjectState.Added
                                };
                                vd.VoucherDetailReferences.Add(vdr);
                            }
                            sat.TripAdvances.Add(log);
                        }
                        v.VoucherDetails.Add(vd);
                    }
                }

                if (drfueladv.Any())//If Fuel Paid was less than fuel expense
                {
                    Dictionary<long, decimal> amts = new Dictionary<long, decimal> { };

                    var fuelexpgroup = sat.vwTripExpenses.Where(x => !x.IsDeleted && drfueladv.Select(y => (long?)y.Id).Contains(x.TripAdvanceLogId)).GroupBy(x => x.AccountId);
                    foreach (var expgr in fuelexpgroup)
                    {
                        if (expgr.Key.GetValueOrDefault() == 0)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "One of Expense does not have Expense Account Mapped");
                        }                        
                        var advids = expgr.Select(x => x.TripAdvanceLogId.GetValueOrDefault()).Distinct();
                        decimal vdamount = 0;
                        foreach (var item in advids)
                        {
                            var thisgrpexp = expgr.Where(x => x.TripAdvanceLogId == item).Sum(x => x.SettledAmt);
                            if (!amts.ContainsKey(item))
                            {
                                var log = drfueladv.FirstOrDefault(x => x.Id == item);
                                if (log.SettlementId > 0 && log.SettlementId != sat.Id)
                                {
                                    throw new BusinessException(ErrorCode.TADV106, $"Reference No {log.ReferenceNo}");
                                }
                                log.SettlementId = sat.Id;
                                log.TripLogId = sat.vwTripAdvances.FirstOrDefault(x => x.Id == log.Id)?.TripLogId;
                                log.ObjectState = ObjectState.Modified;
                                sat.TripAdvances.Add(log);
                                var thisadvexpamt = sat.vwTripExpenses.Where(x => !x.IsDeleted && advids.Contains(x.TripAdvanceLogId.GetValueOrDefault())).Sum(x => x.SettledAmt);

                                var thisadvtotaldiff = Math.Abs(log.FuelAmount - thisadvexpamt);
                                amts[log.Id] = thisadvtotaldiff;
                            }
                            decimal amt = 0;
                            if (amts.ContainsKey(item) && amts[item] - thisgrpexp >= 0) amt = thisgrpexp;
                            else if (amts.ContainsKey(item) && amts[item] - thisgrpexp < 0) amt = amts[item];
                            if (amt == 0) continue;
                            if (amts.ContainsKey(item))
                            {
                                amts[item] -= amt;
                                vdamount += amt;
                            }

                        }
                        if (vdamount > 0)
                        {
                            var vd = new VoucherDetail()
                            {
                                OfficeId = sat.OfficeId,
                                AccountId = expgr.Key.GetValueOrDefault(),
                                OrderId = 2,
                                Amount = Math.Abs(vdamount),
                                ObjectState = ObjectState.Added,
                                VoucherId = v.Id,
                                Voucher = v
                            };
                            v.VoucherDetails.Add(vd);
                        }

                    }
                }
                foreach (var adv in fueladvs)
                {
                    var fadv = sat.vwTripAdvances?.FirstOrDefault(x => x.Id == adv.Id);
                    if (fadv?.TripLogId > 0)
                    {
                        adv.TripLogId = fadv.TripLogId;
                    }
                    adv.SettlementId = sat.Id;
                    adv.ObjectState = ObjectState.Modified;
                }
            }
            #endregion

            #region 3:Trip Expances Voucher Details
            var excludedtypes = new long[] { 2, 85 };
            if(sat.vwTripExpenses.Any(x => (x.TripAdvanceLogId.GetValueOrDefault() == 0|| sat.vwTripAdvances.Any(y => y.Id == x.TripAdvanceLogId && !excludedtypes.Contains(y.TypeId))) && x.ExpNatureId != 1617 && !x.IsDeleted && x.AccountId.GetValueOrDefault() == 0))
            {
                throw new BusinessException(ErrorCode.GLB106, "Account is not mapped on expance type");
            }

            var expgroup = sat.vwTripExpenses?.Where(x => (x.TripAdvanceLogId.GetValueOrDefault() == 0 || sat.vwTripAdvances.Any(y => y.Id == x.TripAdvanceLogId && !excludedtypes.Contains(y.TypeId))) && x.ExpNatureId != 1617 && !x.IsDeleted)?.GroupBy(x => x.AccountId).ToList();
            if(expgroup!=null)
            {
                foreach (var exp in expgroup)
                {
                    var vd = new VoucherDetail()
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = exp.Key.GetValueOrDefault(),
                        OrderId = 3,
                        Amount = exp.Sum(x => x.SettledAmt),
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id,
                        Voucher = v
                    };
                    v.VoucherDetails.Add(vd);
                }
            }
            #endregion

            #region Trip Log Mapping  : 1:VehicleMovementlog
            var tlids = sat.vwTripLogs?.Where(x => !x.IsDeleted).Select(x => x.Id).ToList();
            if (tlids != null && tlids.Any())
            {
                var skipedadvtypes = new long[] { 17, 86, 85 };
                
                var invalids = sat.TripAdvances.Where(x => !skipedadvtypes.Contains(x.AdvanceTypeId.Value) && (x.TripLogId == null || !tlids.Contains((long)x.TripLogId))).ToList();
                if (invalids.Any())
                {
                    var invalidrefs = invalids.Select(x => x.ReferenceNo).JoinStrings(",");
                    throw new BusinessException(ErrorCode.GLB106, $"{invalidrefs} Attached Trip Cash Advance is either not mapped to any TripLog or is Mapped to TripLog that is not in this Settlement.");
                }
                if (!isNew)
                {
                    await uow.ExecSqlQueryAsync(
                        $"UPDATE [dbo].[tVehicleMovementLog] SET SettlementId=NULL WHERE SettlementId={sat.Id};");
                }
                foreach (var tl in sat.vwTripLogs?.Where(x => !x.IsDeleted).ToList())
                {
                    await uow.ExecSqlQueryAsync(
                 $"UPDATE [dbo].[tVehicleMovementLog] SET SettlementId={sat.Id},KmRunAdd={tl.AddKM} WHERE Id={tl.Id}");
                }
                
            }

            #endregion

            #region Cash Deposit VD
            if (sat.CashDeposited > 0)
            {
                var vd = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId =sat.CashDepositAccId>0? sat.CashDepositAccId.GetValueOrDefault(): sat.SettlementAccountId.GetValueOrDefault(0),
                    Amount = sat.CashDeposited,
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 4,
                    VoucherId = v.Id
                };
                v.VoucherDetails.Add(vd);
            }
            if (sat.CashPaid > 0)
            {
                var vd = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = generateCashPaidAdvance==0?(sat.CashPaidAccId > 0 ? sat.CashPaidAccId.GetValueOrDefault() : sat.SettlementAccountId.GetValueOrDefault(0)): DefaultTruckControlAccountId,
                    Amount = -sat.CashPaid,
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 4,
                    VoucherId = v.Id
                };
                if (vd.AccountId == 0)
                {
                    vd.AccountId = _repository.GetConfigValue<long>("DefaultSettledAccountId");
                }
                v.VoucherDetails.Add(vd);
                if (generateCashPaidAdvance > 0)
                {
                    if (sat.CashPaidAccId.GetValueOrDefault() <= 0)
                    {
                        throw new BusinessException("cashpaidaccountrequired", sat.TripSheetNo);
                    }
                    PrepareCashPaidAdvance(ref sat, sat.CashPaidAccId.GetValueOrDefault(), vd.AccountId, vRepo, generateCashPaidAdvance);
                }
            }
            #endregion

            #region Settlement voucher
            v.ViewId = sat.ViewId;
            v.OfficeId = sat.OfficeId;
            v.VoucherNo = sat.TripSheetNo;
            v.VoucherDate = sat.SettleDate.GetValueOrDefault(DateTime.Now);
            v.VoucherDateTime = sat.SettleDate.Value;
            v.VoucherAmount = v.VoucherDetails.Where(x=> x.Amount>0).Sum(x=> x.Amount);//s.fk_Voucher.VoucherDetails.Sum(x => x.Amount);
            v.VoucherTypeId = 18;

            sat.ObjectState = ObjectState.Modified;
            sat.fk_Voucher = v;
            sat.VoucherId = v.Id;
            vRepo.Attach(v);
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;

            #endregion

            #region Settlement Balance Voucher
            if(netbalanceadvances==1 && netbalance != 0 && sat.SettleDate !=null)
            {
                var baladv = new TripAdvanceLog
                {
                    AdvanceDate = sat.SettleDate.Value,
                    FuelAmount = 0,
                    FuelQty = 0,
                    CashAmount = Math.Abs(netbalance),
                    OfficeId = sat.OfficeId,
                    CreditAccountId = DefaultTruckControlAccountId,
                    FuelRate = 0,
                    DebitAccountId = (settlacctype==4||settlacctype==1)/*Maintain In Driver ledger*/?(sat.SettlementAccountId?? sat.Driver1Id): DefaultTruckControlAccountId,
                    DriverId = sat.Driver1Id,
                    FuelId = null,
                    TripLogId = null,
                    VehicleId = null,
                    IsBulkEntry = false
                };

                switch (sat.AdjustmentTypeId)
                {
                    case 1592: //both
                        #region Fuel
                        if (sat.FuelAmountDifference > 0)/*[Receive]*/
                        {
                            #region TripAdvance Fuel

                            var adv1592 = baladv.Clone();
                            adv1592.Remark = "Trip Settlement fuel Balance Carry Forwarded as driver was unable to pay cash back to company.";
                            adv1592.ReferenceNo = "TSFBL-" + sat.TripSheetNo;
                            adv1592.VoucherNo = "TSFBL-" + sat.TripSheetNo;
                            adv1592.CashAmount = 0;
                            adv1592.FuelQty = sat.FuelQtyDifference;
                            adv1592.FuelAmount = sat.FuelAmountDifference;
                            adv1592.AdvanceTypeId = 85;
                            adv1592.FuelRate = sat.NetBalanceFuelRate;
                            #endregion End TripAdvance Fuel

                            #region Voucher Fuel
                            var v1592Fuel = new Voucher()
                            {
                                Id = sat.SetlBalFuelVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 85,
                                OfficeId = sat.OfficeId,
                                VoucherAmount= Math.Abs(sat.FuelAmountDifference),
                                VoucherNo = adv1592.VoucherNo,
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                Account1Id = adv1592.DebitAccountId,
                                Amount1 = Math.Abs(sat.FuelAmountDifference),
                                Account2Id = adv1592.CreditAccountId,
                                Amount2 = -Math.Abs(sat.FuelAmountDifference),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added
                            };
                            adv1592.VoucherId = v1592Fuel.Id;
                            adv1592.fk_Voucher = v1592Fuel;
                            #endregion

                            #region VD1 Fuel
                            var v1592Fuel1 = new VoucherDetail()
                            {
                                AccountId = v1592Fuel.Account1Id.GetValueOrDefault(),
                                Amount = v1592Fuel.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                VoucherId = v1592Fuel.Id,
                                Voucher = v1592Fuel,
                                ObjectState = ObjectState.Added,
                            };
                            v1592Fuel.VoucherDetails.Add(v1592Fuel1);
                            #endregion

                            #region VDR
                            var v1592fvdr = new VoucherDetailReference()
                            {
                                Amount = v1592Fuel1.Amount,
                                ReferenceNo = sat.TripSheetNo,
                                VDRTypeId = 1013, //New Reference
                                VoucherDetailId = v1592Fuel1.Id,
                                fk_VoucherDetail = v1592Fuel1,
                                ObjectState = ObjectState.Added
                            };
                            v1592Fuel1.VoucherDetailReferences.Add(v1592fvdr);

                            adv1592.VDRId = v1592fvdr.Id;
                            adv1592.fk_VDR = v1592fvdr;
                            adv1592.ObjectState = ObjectState.Added;
                            advRepo.Insert(adv1592);
                            #endregion

                            #region VD2 Fuel
                            var v1592Fuel2 = new VoucherDetail()
                            {
                                VoucherId = v1592Fuel.Id,
                                Voucher = v1592Fuel,
                                AccountId = v1592Fuel.Account2Id.GetValueOrDefault(),
                                Amount = v1592Fuel.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                            };
                            v1592Fuel.VoucherDetails.Add(v1592Fuel2);
                            #endregion
                            sat.fk_SetlBalFuelVoucher = v1592Fuel;
                            sat.SetlBalFuelVoucherId = v1592Fuel.Id;
                            sat.ObjectState = ObjectState.Modified;
                            vRepo.Insert(v1592Fuel);
                        }
                        #endregion

                        #region Cash
                        if(sat.SettledAmount>0)/*[Receive]*/
                        {
                            #region TripAdvance Cash
                            baladv.Remark = "Trip Settlement Cash Balance Carry Forwarded as driver was unable to pay back to company.";
                            baladv.ReferenceNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.AdvanceTypeId = 17;
                            baladv.CashAmount = sat.SettledAmount;
                            #endregion

                            #region Voucher Cash
                            var v1592Cash = new Voucher()
                            {
                                Id = sat.SetlBalVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 17,
                                OfficeId = sat.OfficeId,
                                VoucherNo = baladv.VoucherNo,
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                VoucherAmount = Math.Abs(sat.SettledAmount),
                                Account1Id = baladv.DebitAccountId,
                                Amount1 = Math.Abs(sat.SettledAmount),
                                Account2Id = baladv.CreditAccountId,
                                Amount2 = -Math.Abs(sat.SettledAmount),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added,
                                AccountingRemark= baladv.Remark
                            };
                            baladv.VoucherId = v1592Cash.Id;
                            baladv.fk_Voucher = v1592Cash;
                            #endregion

                            #region VD1 Cash
                            var v1592Cash1 = new VoucherDetail()
                            {
                                VoucherId = v1592Cash.Id,
                                Voucher = v1592Cash,
                                AccountId = v1592Cash.Account1Id.GetValueOrDefault(),
                                Amount = v1592Cash.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark
                            };
                            v1592Cash1.ObjectState = ObjectState.Added;
                            v1592Cash.VoucherDetails.Add(v1592Cash1);
                            #endregion

                            #region VDR
                            var v1592cash = new VoucherDetailReference()
                            {
                                VoucherDetailId = v1592Cash1.Id,
                                fk_VoucherDetail = v1592Cash1,
                                Amount = Math.Abs(v1592Cash1.Amount),
                                ReferenceNo = sat.TripSheetNo,
                                VDRTypeId = 1013, //New Reference
                                ObjectState = ObjectState.Added
                            };
                            v1592Cash1.VoucherDetailReferences.Add(v1592cash);

                            baladv.VDRId = v1592cash.Id;
                            baladv.fk_VDR = v1592cash;
                            baladv.ObjectState = ObjectState.Added;
                            advRepo.Insert(baladv);
                            #endregion

                            #region VD2 Cash
                            var v1592Cash2 = new VoucherDetail()
                            {
                                VoucherId = v1592Cash.Id,
                                Voucher = v1592Cash,
                                AccountId = v1592Cash.Account2Id.GetValueOrDefault(),
                                Amount = v1592Cash.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                                Narration=baladv.Remark
                            };
                            v1592Cash.VoucherDetails.Add(v1592Cash2);
                            #endregion
                            sat.fk_SetlBalVoucher = v1592Cash;
                            sat.SetlBalVoucherId = v1592Cash.Id;
                            vRepo.Insert(v1592Cash);
                        }
                        #endregion
                        break;

                    case 1590:// Net As Cash Adv
                        if (netbalance > 0)
                        {
                            #region TripAdvance
                            baladv.Remark = "Trip Settlement Cash Balance Carry Forwarded as driver was unable to pay cash back to company.";
                            baladv.ReferenceNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.AdvanceTypeId = 17;
                            baladv.CashAmount = netbalance;
                            #endregion

                            #region Voucher
                            var v1590 = new Voucher()
                            {
                                Id = sat.SetlBalVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 17,
                                OfficeId = sat.OfficeId,
                                VoucherNo = baladv.VoucherNo,
                                VoucherAmount= Math.Abs(netbalance),
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                Account1Id = baladv.DebitAccountId,
                                Amount1 = Math.Abs(netbalance),
                                Account2Id = baladv.CreditAccountId,
                                Amount2 = -Math.Abs(netbalance),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added,
                                AccountingRemark= baladv.Remark
                            };
                            baladv.VoucherId = v1590.Id;
                            baladv.fk_Voucher = v1590;

                            #endregion

                            #region VD1 Cash
                            var v15901 = new VoucherDetail()
                            {
                                VoucherId = v1590.Id,
                                Voucher = v1590,
                                AccountId = v1590.Account1Id.GetValueOrDefault(),
                                Amount = v1590.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                ObjectState = ObjectState.Added,
                                Narration=baladv.Remark
                            };
                            v15901.ObjectState = ObjectState.Added;
                            v1590.VoucherDetails.Add(v15901);
                            #endregion

                            #region VDR

                            var v1591vdr = new VoucherDetailReference()
                            {
                                VoucherDetailId = v15901.Id,
                                fk_VoucherDetail = v15901,
                                Amount = Math.Abs(v15901.Amount),
                                ReferenceNo = sat.TripSheetNo,
                                VDRTypeId = 1013, //New Reference
                                ObjectState = ObjectState.Added
                            };
                            v15901.VoucherDetailReferences.Add(v1591vdr);

                            baladv.VDRId = v1591vdr.Id;
                            baladv.fk_VDR = v1591vdr;
                            baladv.ObjectState = ObjectState.Added;
                            advRepo.Insert(baladv);
                            #endregion

                            #region VD2 Cash

                            var v15902 = new VoucherDetail()
                            {
                                VoucherId = v1590.Id,
                                Voucher = v1590,
                                AccountId = v1590.Account2Id.GetValueOrDefault(),
                                Amount = v1590.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark
                            };
                            v1590.VoucherDetails.Add(v15902);
                            #endregion
                            sat.fk_SetlBalVoucher = v1590;
                            sat.SetlBalVoucherId = v1590.Id;
                            vRepo.Insert(v1590);
                        }
                        break;

                    case 1591://Net As Fuel Adv

                        if (netbalance > 0)
                        {
                            #region TripAdvance
                            baladv.Remark = "Trip Settlement fuel Balance Carry Forwarded as driver was unable to pay back to company.";
                            baladv.ReferenceNo = "TSFBL-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TSFBL-" + sat.TripSheetNo;
                            baladv.CashAmount = 0;
                            baladv.FuelQty = sat.FuelQtyDifference;
                            baladv.AdvanceTypeId = 85;
                            baladv.FuelAmount = netbalance;
                            baladv.FuelRate = sat.NetBalanceFuelRate;
                            #endregion

                            #region Voucher Fuel

                            var v1591 = new Voucher()
                            {
                                Id = sat.SetlBalFuelVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 85,
                                OfficeId = sat.OfficeId,
                                VoucherNo = "FUEL-" + baladv.VoucherNo,
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                VoucherAmount = Math.Abs(netbalance),
                                Account1Id = baladv.DebitAccountId,
                                Amount1 = Math.Abs(netbalance),
                                Account2Id = baladv.CreditAccountId,
                                Amount2 = -Math.Abs(netbalance),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added
                            };
                            baladv.VoucherId = v1591.Id;
                            baladv.fk_Voucher = v1591;

                            #endregion

                            #region VD1 Fuel

                            var v15911 = new VoucherDetail()
                            {
                                AccountId = v1591.Account1Id.GetValueOrDefault(),
                                Amount = v1591.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                VoucherId = v1591.Id,
                                Voucher = v1591,
                                ObjectState = ObjectState.Added,
                            };
                            v1591.VoucherDetails.Add(v15911);
                            #endregion

                            #region VDR

                            var v15911fvdr = new VoucherDetailReference()
                            {
                                Amount = v15911.Amount,
                                ReferenceNo = sat.TripSheetNo,
                                VDRTypeId = 1013, //New Reference
                                VoucherDetailId = v15911.Id,
                                fk_VoucherDetail = v15911,
                                ObjectState = ObjectState.Added
                            };
                            v15911.VoucherDetailReferences.Add(v15911fvdr);
                            baladv.VDRId = v15911fvdr.Id;
                            baladv.fk_VDR = v15911fvdr;
                            baladv.ObjectState = ObjectState.Added;
                            advRepo.Insert(baladv);
                            #endregion

                            #region VD2 Fuel
                            var v15912 = new VoucherDetail()
                            {
                                VoucherId = v1591.Id,
                                Voucher = v1591,
                                AccountId = v1591.Account2Id.GetValueOrDefault(),
                                Amount = v1591.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                            };
                            v1591.VoucherDetails.Add(v15912);
                            #endregion
                            sat.fk_SetlBalFuelVoucher = v1591;
                            sat.SetlBalFuelVoucherId = v1591.Id;
                            sat.ObjectState = ObjectState.Modified;
                            vRepo.Insert(v1591);
                        }
                        break;
                }

            }

            #endregion

            #region Driver Balance Pending
            var NetBalanceCarryAmt= _repository.GetConfigValue<int>("NetBalanceCarryForwordAmount");


            var satamt =NetBalanceCarryAmt==0? netbalance: sat.SettledAmount - sat.CashDeposited+sat.CashPaid;
            if (SettPayoffRule>1&&sat.NetBalancePending && satamt < 0/*If Net Balance is negative it mean we have to pay to driver*/)
            {

                #region Driver Pending Advance

                var drvrbalancepending = new TripAdvanceLog
                {
                    AdvanceDate = sat.SettleDate.Value,
                    FuelAmount = 0,
                    FuelQty = 0,
                    AdvanceTypeId = 86,
                    Remark = "Pending Balance Carry Forwarded as company was unable to pay back to Driver.",
                    ReferenceNo = "TBPND-" + sat.TripSheetNo,
                    VoucherNo = "TBPND-" + sat.TripSheetNo,
                    CashAmount = Math.Abs(satamt),
                    OfficeId = sat.OfficeId,
                    CreditAccountId = DefaultTruckControlAccountId,
                    FuelRate = 0,
                    DebitAccountId = SettPayoffRule == 2 ? vd5.AccountId : DefaultSettlementNetBalancePayoffAccount,
                    DriverId = sat.Driver1Id,
                    FuelId = null,
                    TripLogId = null,
                    VehicleId = null,
                    IsBulkEntry = false
                };
                //Settlement Balance Deposit

                #endregion Driver Pending Advance

                #region Voucher PayBalance

                var vchr = new Voucher()
                {
                    Id = sat.SetlBalFuelVoucherId.GetValueOrDefault(),
                    VoucherTypeId = 86,
                    OfficeId = sat.OfficeId,
                    VoucherNo = drvrbalancepending.VoucherNo,
                    VoucherDate = sat.SettleDate.GetValueOrDefault(),
                    VoucherDateTime = sat.SettleDate.Value,
                    Account1Id = drvrbalancepending.DebitAccountId,
                    Amount1 = Math.Abs(satamt),
                    Account2Id = drvrbalancepending.CreditAccountId,
                    Amount2 = -Math.Abs(satamt),
                    IsAudited = false,
                    IsAccepted = false,
                    IsAccountsVisiblity = false,
                    PageId = null,
                    ViewId = sat.ViewId,
                    ObjectState = ObjectState.Added,
                    AccountingRemark= drvrbalancepending.Remark
                };
                drvrbalancepending.VoucherId = vchr.Id;
                drvrbalancepending.fk_Voucher = vchr;
                drvrbalancepending.ObjectState = ObjectState.Added;
                advRepo.Insert(drvrbalancepending);

                #endregion

                #region VD1 Fuel dr

                var vchrvd = new VoucherDetail()
                {
                    AccountId = vchr.Account1Id.GetValueOrDefault(),
                    Amount = vchr.Amount1,
                    OfficeId = sat.OfficeId,
                    OrderId = 1,
                    VoucherId = vchr.Id,
                    Voucher = vchr,
                    ObjectState = ObjectState.Added,
                    Narration = drvrbalancepending.Remark
                };
                vchr.VoucherDetails.Add(vchrvd);
                #endregion

                #region VD2 Fuel cr
                var vchrvd2 = new VoucherDetail()
                {
                    VoucherId = vchr.Id,
                    Voucher = vchr,
                    AccountId = vchr.Account2Id.GetValueOrDefault(),
                    Amount = vchr.Amount2,
                    OfficeId = sat.OfficeId,
                    OrderId = 2,
                    ObjectState = ObjectState.Added,
                    Narration = drvrbalancepending.Remark
                };
                vchr.VoucherDetails.Add(vchrvd2);
                #endregion

                #region VDR

                var vchrvd1 = new VoucherDetailReference()
                {
                    Amount = vchrvd2.Amount,
                    ReferenceNo = sat.TripSheetNo,
                    VDRTypeId = 1013, //New Reference
                    VoucherDetailId = vchrvd2.Id,
                    fk_VoucherDetail = vchrvd2,
                    ObjectState = ObjectState.Added
                };
                drvrbalancepending.fk_VDR= vchrvd1;
                drvrbalancepending.VDRId = vchrvd1.Id;
                vchrvd2.VoucherDetailReferences.Add(vchrvd1);
                #endregion
                sat.fk_NetBalVoucher = vchr;
                sat.NetBalVoucherId = vchr.Id;
                sat.ObjectState = ObjectState.Modified;
                vRepo.Insert(vchr);
            }

            #endregion
        }
        public async Task CreateSettlementV3(VehicleTripSettlement sat, IUnitOfWorkAsync uow)
        {
            var settlementReceivedAdvTypeIds = new long?[] { 17};
            
            bool isNew = sat.Id == 0;
            var advRepo = _repository.GetRepository<TripAdvanceLog>();
            var ledgerRepo = _repository.GetRepository<Ledger>().Queryable();
            var vRepo = _repository.GetRepository<Voucher>();

            var teRepo = _repository.GetRepository<TripExpenseLog>();
            var expTypeRepo = uow.RepositoryAsync<ExpenseMaster>();
            var v = sat.fk_Voucher ?? (sat.VoucherId > 0 ? await vRepo.Queryable().FirstOrDefaultAsync(x => x.Id == sat.VoucherId) ?? new Voucher() : new Voucher());
            var fed = sat.vwFuelExpenses.Where(x => x.IsDeleted).Select(x => x.Id).ToList();
            fed.AddRange(sat.vwTripExpenses.Where(x => x.IsDeleted).Select(x => x.Id));
            if (fed.Any())
            {
                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripExpenseLog] WHERE Id in ({fed.JoinStrings(",")})");
                sat.vwTripExpenses?.RemoveAll(x => x.IsDeleted);
                sat.vwFuelExpenses?.RemoveAll(x => x.IsDeleted);
                var invalidfe = sat.vwFuelExpenses.Where(x => x.AdvanceId == 0 || x.TripLogId.GetValueOrDefault(0) == 0).Select(x => $"UsedQty:{x.UsedQty}[{x.Remark}]");
                if (sat.vwFuelExpenses.Any() && sat.vwFuelExpenses.Any(x => x.AdvanceId == 0 || x.TripLogId.GetValueOrDefault(0) == 0)) throw new BusinessException(ErrorCode.TS103, $"Few Fuel Stock Consumption are either not mapped to Trip or are not Mapped to Any Fuel Stock Entry.{Environment.NewLine}{(string.Join(",", invalidfe))}");
            }
            #region NetBalance Vd
            bool RefFlag = false;
            long DefaultTruckControlAccountId = 0;
            var netbalance = sat.FuelAmountDifference/*Value<0 Pay Value>0 Receive*/ + sat.SettledAmount /*Value<0 Pay Value>0 Receive*/- sat.CashDeposited/*Always Value>0*/+ sat.CashPaid;
            var SettPayoffRule = _repository.GetConfigValue<long>("SettlementNetBalancePayoffRule");/*0:Payoff,1:MaintainButNoAdvance, 2: MaintainInSettledAc, 3: MaintainInOtherThenSettlement*/
            var netbalanceadvances = _repository.GetConfigValue<long>("CreateSettlementNetBalanceAmount");
            var DefaultSettlementNetBalancePayoffAccount = _repository.GetConfigValue<long>("DefaultSettlementNetBalancePayoffAccount");
            if (SettPayoffRule == 0/*0:PayOff*/)
            {
                sat.NetBalancePending = false;
            }
            if (SettPayoffRule == 3 && DefaultSettlementNetBalancePayoffAccount == 0)
            {
                throw new BusinessException(ErrorCode.GLB103, "Missing Default Settlement NetBalance Payoff Account");
            }
            var vd5 = new VoucherDetail();
            //VoucherDetailReference vdr5 = null;
            var settlacctype = _repository.GetConfigValue<long>("SettledAccountType");
            var generateCashPaidAdvance = _repository.GetConfigValue<long>("GenerateCashPaidAdvance");
            var tladvmappingflag = _repository.GetClientConfigValue<long>("ShowAutoTripOnAdvance");
            switch (settlacctype)
            {
                case 0:
                    if (sat.SettlementAccountId != null)
                    {
                        //2:Auto Only Cash Account and Control Account are Allowed
                        var zero = await ledgerRepo.Where(x => x.Id == sat.SettlementAccountId).Select(x => new
                        {
                            x.Id,
                            x.ReferenceFlag
                        }).FirstOrDefaultAsync();
                        if (zero == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Wrong Settled Account Name Selected");
                        }
                        DefaultTruckControlAccountId = zero.Id;
                        RefFlag = zero.ReferenceFlag;
                    }
                    break;

                case 1:
                    //1:Driver//Credit or Debit could be only done in Driver Ledger
                    var one = await ledgerRepo.Where(x => x.Id == sat.Driver1Id).Select(x => new
                    {
                        x.Id,
                        x.ReferenceFlag
                    }).FirstOrDefaultAsync();
                    if (one == null)
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Only Driver can be selected in Settled Account Name");
                    }
                    DefaultTruckControlAccountId = one.Id;
                    RefFlag = one.ReferenceFlag;
                    break;
                case 3:
                    //1:Driver//Credit or Debit could be only done in DefaultControlAccount
                    DefaultTruckControlAccountId = _repository.GetConfigValue<long>("DefaultSettledAccountId");
                    if (DefaultTruckControlAccountId == 0)
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Default truck control account not configured");
                    }
                    var three = await ledgerRepo.Where(x => x.Id == DefaultTruckControlAccountId).Select(x => new
                    {
                        x.Id,
                        x.ReferenceFlag
                    }).FirstOrDefaultAsync();
                    if (three == null)
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Invalid Truck Control account configured ");
                    }
                    RefFlag = three.ReferenceFlag;
                    break;
            }

            if (DefaultTruckControlAccountId == 0)
            {
                DefaultTruckControlAccountId = _repository.GetConfigValue<long>("DefaultSettledAccountId");
            }
            if (netbalance != 0)
            {
                vd5 = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = sat.SettlementAccountId.GetValueOrDefault()>0? sat.SettlementAccountId.GetValueOrDefault() : DefaultTruckControlAccountId,//netbalance<0?/*If Pay[Negative]*/sat.SettlementAccountId.GetValueOrDefault() :/*If Receive[Possitive]*/ DefaultTruckControlAccountId,
                    Amount = netbalance,
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 5,
                    VoucherId = v.Id
                };
                v.VoucherDetails.Add(vd5);
                if(vd5.AccountId > 0 &&await _repository.GetRepository<Ledger>().Queryable().AnyAsync(x=>x.Id== vd5.AccountId && x.ReferenceFlag))
                {
                    var vdr5 = new VoucherDetailReference()
                    {
                        Amount = vd5.Amount,
                        ObjectState = ObjectState.Added,
                        VDRTypeId = 1013,   //new Reference
                        VoucherDetailId = vd5.Id,
                        fk_VoucherDetail = vd5,
                        ReferenceNo = $"{sat.TripSheetNo}-BAL",
                        AccountId = vd5.AccountId,
                        DueDate = sat.SettleDate ?? sat.EndDate??DateTime.Now
                    };
                    vd5.VoucherDetailReferences.Add(vdr5);
                }
            }
            #endregion

            #region Deleting Old Voucher

            if (!isNew)
            {
                if (sat.CashPaidAdvId.GetValueOrDefault() > 0 && await advRepo.Queryable().AnyAsync(x => x.Id == sat.CashPaidAdvId && x.RequestStatusId == 1597))
                {
                    throw new BusinessException(ErrorCode.TADV108, "The Balance TripAdvance for this Trip Settlement has been Disburshed.");
                }

                /*Delete All the VD of Settlement*/
                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVoucherVD] WHERE VoucherId={v.Id}");
                /**/
                var balvids = new List<long?>() { sat.SetlBalFuelVoucherId.GetValueOrDefault(), sat.SetlBalVoucherId.GetValueOrDefault(), sat.NetBalVoucherId.GetValueOrDefault() }.Where(x => x > 0).ToList();
                if (balvids.Any())
                {
                    if (await advRepo.Queryable().AnyAsync(x => x.VoucherId > 0 && x.SettledAdvances.Any() && balvids.Contains(x.VoucherId)))
                    {
                        throw new BusinessException(ErrorCode.TADV106, "Balance of this settlement has been settled in any other settlement or has been reversed");
                    }
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripAdvanceLog] WHERE VoucherId IN ({(balvids.JoinStrings(","))})");
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripSettlement] SET SetlBalFuelVoucherId=NULL, SetlBalVoucherId=NULL, NetBalVoucherId=NULL,CashPaidAdvId=NULL WHERE Id={sat.Id}");
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVouchers] WHERE Id IN ({balvids.JoinStrings(",")})");
                }
                if (sat.CashPaidAdvId.GetValueOrDefault() > 0)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripSettlement] SET CashPaidAdvId=NULL WHERE Id={sat.Id}");
                    var voucherid = advRepo.Queryable().Where(x => x.Id == sat.CashPaidAdvId).Select(x => x.VoucherId).FirstOrDefault();
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripAdvanceLog] WHERE Id=@p0", sat.CashPaidAdvId);
                    if (voucherid > 0)
                    {
                        await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVouchers] WHERE Id=@p0", voucherid);
                    }
                }
                sat.SetlBalFuelVoucherId = null;
                sat.SetlBalVoucherId = null;
                sat.NetBalVoucherId = null;
                sat.CashPaidAdvId = null;
            }

            #endregion

            #region Trip Expenses
            var existingexpids = sat.vwTripExpenses.Where(x => x.Id > 0 && !x.IsDeleted).Select(y => y.Id).Distinct().ToList();
            //if (!isNew)
            //{
            //    var todbedeleted= sat.vwTripExpenses.Where(x => x.Id > 0 && x.IsDeleted).Select(y => y.Id).Distinct().ToList();
            //    var exists = existingexpids.JoinStrings(",");
            //    string query = "";
            //    var both = (existingexpids.Any() || todbedeleted.Any());
            //    var p1 = existingexpids.Any();
            //    var p2 = todbedeleted.Any();
            //    var conddelete = $"{(both ? "AND(":"")}{(p1?$"Id not in ({ exists})" : "")} {(both?" OR ":"")} {(p2 ? $"Id in ({  todbedeleted.JoinStrings(",")})" : "")}{(both ? ")" : "")}";
            //    await uow.ExecSqlQueryAsync($"DELETE  FROM [dbo].[tTripExpenseLog] WHERE SettlementId={sat.Id} AND IsBudgeted<>1 {conddelete} AND NOT EXISTS(SELECT 1 FROM tTripAdvanceLog tl WHERE ISNULL([dbo].[tTripExpenseLog].TripAdvanceLogId,0)=tl.Id AND tl.AdvanceTypeId=3)");
            //}
            var expansetypeids = sat.vwTripExpenses?.Where(x => !x.IsDeleted).Select(x => x.TypeId).Distinct().ToList();
            var expaccounts = await expTypeRepo.Queryable().Where(x => expansetypeids.Contains(x.Id)).Select(x => new
            {
                x.LedgerId,
                x.Id,
                x.NatureId
            }).Distinct().ToListAsync();
            if (expaccounts.Any())
            {
                sat.vwTripExpenses?.Where(x => !x.IsDeleted).ToList().ForEach(x =>
                {
                    var acid = expaccounts.FirstOrDefault(y => y.Id == x.TypeId);
                    x.AccountId = acid?.LedgerId;
                    x.ExpNatureId = acid?.NatureId;
                });
            }

            var existingexps = existingexpids.Any() ? await teRepo.Queryable().Where(x => existingexpids.Contains(x.Id)).ToListAsync() : null;
            foreach (var item in sat.vwTripExpenses.Where(x => !x.IsDeleted))
            {
                /* Prepare TripExpenseLog for all received Expense*/
                var texp = (item.Id > 0 ? existingexps.FirstOrDefault(x => x.Id == item.Id) : null) ?? new TripExpenseLog();

                //if (item.Id > 0 && existingexps.Any(x => x.Id == item.Id))
                //{
                //    texp.Id = item.Id;
                //    teRepo.Update(texp);
                //    var entry = this._repository.UOW.Context.Entry(texp);
                //    entry.State = EntityState.Modified;
                //    entry.Property("RowVersion").OriginalValue = item.RowVersion;
                //    //texp.RowVersion = item.RowVersion;
                //    if (texp.RowVersion == null)
                //    {
                //        var db = existingexps.FirstOrDefault(x => x.Id == texp.Id)?.RowVersion; ;
                //        entry.Property("RowVersion").OriginalValue = db;
                //        //texp.RowVersion = db;

                //    }
                //}
                if (texp.Id <= 0)
                {
                    texp.ObjectState = ObjectState.Added;
                    teRepo.Insert(texp);
                }
                else
                {
                    texp.ObjectState = ObjectState.Modified;
                    teRepo.Update(texp);
                }
                texp.TripLogId = item.TripLogId;
                texp.SettlementId = sat.Id;
                if (!texp.IsBudgeted)
                {
                    texp.ClaimAmount = item.ClaimAmt;
                }
                texp.SettledAmount = item.SettledAmt;
                texp.Remarks = item.Remark;
                texp.ExpenseTypeId = item.TypeId;
                texp.TripAdvanceLogId = item.TripAdvanceLogId;
                texp.FuelRate = item.Rate;
                texp.FuelQty = item.FuelQty;
                texp.ViewId = sat.ViewId;
                texp.ObjectState = texp.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                sat.TripExpenses.Add(texp);
                if (sat.vwTripLogs.All(x => !x.IsDeleted && x.Id != texp.TripLogId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "One of trip expense has triplog which is not getting settled in this settlement");
                }
            }

            if (isNew)
            {
                sat.ObjectState = ObjectState.Added;
                _repository.Insert(sat);
                await uow.SaveChangesAsync();
            }
            else
            {
                sat.ObjectState = ObjectState.Modified;
                _repository.Update(sat);
                await uow.SaveChangesAsync();
            }
            #endregion
            #region Fuel Expanses Mapping
            #region Start new code for Fuel Stock
            var expenseMaster = await uow.RepositoryAsync<ExpenseMaster>().Queryable().FirstOrDefaultAsync(x => x.NatureId == 1479);

            if (expenseMaster == null)
            {
                throw new BusinessException(ErrorCode.TS103, $"Expense Master with nature Fuel is not defined.");
            }

            if (expenseMaster.LedgerId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.TS103, $"Expense Master is not mapped with Expense Ledger Hint.Expense Name:{expenseMaster.Name}");
            }

            var expadvIds =
                sat.vwFuelExpenses.Where(x => (x.ShortageQty + x.UsedQty) > 0)
                    .Select(x => x.AdvanceId)
                    .Distinct().ToList();
            var feAdvances = await advRepo.Queryable().Include(f => f.FuelExpanses.Select(y => y.fk_ExpenseType.fk_Ledger)).Where(x => expadvIds.Contains(x.Id)).ToListAsync();
            foreach (var fsac in feAdvances.GroupBy(x => x.DebitAccountId))//each debit account in Fuel Stock Advances
            {
                /*VD for Fuel Exp*/
                var vd = new VoucherDetail()
                {
                    OfficeId = sat.OfficeId,
                    AccountId = fsac.Key.GetValueOrDefault(),
                    OrderId = 7,
                    ObjectState = ObjectState.Added,
                    VoucherId = v.Id,
                    Voucher = v
                };
                v.VoucherDetails.Add(vd);
                var fe_expvd = new VoucherDetail()
                {
                    OfficeId = sat.OfficeId,
                    AccountId = expenseMaster.LedgerId.GetValueOrDefault(),//?? Expense Account Kaise Pata kare
                    OrderId = 9,
                    ObjectState = ObjectState.Added,
                    VoucherId = v.Id
                };

                ////var fe_shortagevd = new VoucherDetail()
                ////{
                ////    OfficeId = sat.OfficeId,
                ////    AccountId = sat.Driver1Id.GetValueOrDefault(),
                ////    OrderId = 10,
                ////    ObjectState = ObjectState.Added,
                ////    VoucherId = v.Id
                ////};

                /*
                 VD1 Control A/c Credit 3000
                 VDRS => All the Against Refs 2000 4 Against
                 VDR => All the Non Against Ref 1000
                VD2 ExpenseA/c 2500
                Vd3 Driver Shortage A/c 500

                 */
                foreach (var ad in fsac)//each advance in debit account group
                {
                    var expenses = sat.vwFuelExpenses.Where(x => x.AdvanceId == ad.Id);
                    var shortageamt = expenses.Where(x => !x.IsDeleted).Sum(x => x.ShortageAmt);
                    var totalexpamt = expenses.Where(x => !x.IsDeleted).Sum(x => x.UsedAmt + x.ShortageAmt);
                    vd.Amount += -totalexpamt;
                    fe_expvd.Amount += totalexpamt - shortageamt;
                    //if(shortageamt>0) fe_shortagevd.Amount += shortageamt;
                }
                //if (fe_shortagevd.Amount > 0) {
                //    fe_shortagevd.Voucher = v;
                //    v.VoucherDetails.Add(fe_shortagevd);                    
                //}
                if (fe_expvd.Amount > 0)
                {
                    fe_expvd.Voucher = v;
                    v.VoucherDetails.Add(fe_expvd);
                }
            }
            #endregion
            foreach (IGrouping<long, FuelExpense> expanses in sat.vwFuelExpenses.Where(x => (x.ShortageQty + x.UsedQty) > 0).GroupBy(x => x.AdvanceId))
            {
                var shortageamt = expanses.Where(x => !x.IsDeleted).Sum(x => x.ShortageAmt);
                var totalexpamt = expanses.Where(x => !x.IsDeleted).Sum(x => x.UsedAmt + x.ShortageAmt);

                var adv = sat.TripAdvances.Any(a => a.Id == expanses.Key) ? sat.TripAdvances.First(a => a.Id == expanses.Key) : feAdvances.FirstOrDefault(a => a.Id == expanses.Key);
                //Through Error if Provided AdvanceId is Wrong
                if (adv == null)
                {
                    throw new BusinessException(ErrorCode.TS103, $"Advance Reference Mentioned in one of Fuel Expense is wrong. Hint: AdvKey_{expanses.Key}");
                }
                ////If Fuel Expanses is empty for advance then retry to fatch them direct from DataBase
                //if (adv.FuelExpanses == null || !adv.FuelExpanses.Any())
                //{
                //    var exps = expRepo.Queryable().Include(z => z.fk_ExpenseType.fk_Ledger).Where(z => z.TripAdvanceLogId == adv.Id);
                //    if (exps.Any())
                //    {
                //        adv.FuelExpanses = new List<TripExpenseLog>();
                //        adv.FuelExpanses.AddRange(exps);
                //    }
                //}
                if (adv.FuelExpanses != null && adv.FuelExpanses.Any())
                {
                    //var tls = settlement.TripLogs.Where(x=>x.SettlementId.HasValue).Select(x => x.Id);
                    var tls = sat.TripLogs.Select(x => x.Id);
                    foreach (var x in adv.FuelExpanses.Where(x => !tls.Contains(x.TripLogId)))
                    {
                        x.ObjectState = ObjectState.Unchanged;
                    }
                }

                foreach (FuelExpense expanse in expanses)
                {
                    var exp = adv.FuelExpanses.FirstOrDefault(x => x.Id == expanse.Id);
                    if (exp == null)
                    {
                        exp = teRepo.Queryable().Include(z => z.fk_ExpenseType.fk_Ledger).FirstOrDefault(a => a.Id == expanse.Id && a.TripAdvanceLogId == adv.Id);
                        if (exp == null)
                        {
                            exp = new TripExpenseLog();
                        }
                    }
                    if (expanse.IsDeleted)
                    {
                        exp.SettlementId = null;
                        exp.fk_Settlement = null;
                        exp.fk_TripAdvanceLog = null;
                        exp.fk_TripAdvanceLog = null;
                        exp.fk_TripLog = null;
                        exp.fk_TripLog = null;
                        exp.ObjectState = ObjectState.Deleted;

                        adv.BalanceQty = adv.FuelExpanses.Where(a => a.ObjectState != ObjectState.Deleted).Sum(a => a.FuelQty + a.ShortFuelQty);
                        adv.ObjectState = ObjectState.Modified;

                        teRepo.Delete(exp);
                    }
                    else
                    {
                        decimal otherConsume = 0;
                        if (adv.FuelExpanses != null && adv.FuelExpanses.Any())
                        {
                            otherConsume = expanse.Id > 0 ? adv.FuelExpanses.Where(a => a.Id != expanse.Id).Sum(r => r.FuelQty + r.ShortFuelQty) : adv.FuelExpanses.Sum(r => r.FuelQty + r.ShortFuelQty);
                        }

                        if (adv.FuelQty <= otherConsume)
                            throw new BusinessException(ErrorCode.TS103, $"The Total Fuel Qty is already adjusted against advancelog id:{expanse.AdvanceId}");

                        if ((expanse.UsedQty + expanse.ShortageQty) > (adv.FuelQty - otherConsume))
                            throw new BusinessException(ErrorCode.TS103, $"The Consumed Fuel Qty {expanse.UsedQty + expanse.ShortageQty} Exceeded than Balance Qty {adv.FuelQty - otherConsume} against advancelog no:{adv.ReferenceNo}");

                        exp.FuelQty = expanse.UsedQty;
                        exp.TripAdvanceLogId = expanse.AdvanceId;
                        exp.fk_TripAdvanceLog = adv;
                        exp.Remarks = expanse.Remark;
                        exp.SettledAmount = expanse.UsedAmt;
                        exp.SettlementId = sat.Id;
                        exp.fk_Settlement = sat;
                        exp.ObjectState = exp.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        exp.ClaimAmount = 0;
                        exp.TripLogId = expanse.TripLogId.GetValueOrDefault(0);
                        exp.ShortFuelQty = expanse.ShortageQty;
                        exp.ShortFuelAmt = expanse.ShortageAmt;
                        adv.BalanceQty = adv.FuelQty - adv.FuelExpanses.Where(a => a.ObjectState != ObjectState.Deleted).Sum(a => a.FuelQty + a.ShortFuelQty);
                        adv.ObjectState = ObjectState.Modified;
                        //Through Error if Provided AdvanceId is not mapped with any TripLog in currant Settlement
                        if (sat.vwTripLogs.Where(x => !x.IsDeleted).All(e => e.Id != expanse.TripLogId.GetValueOrDefault(0)))
                        {
                            throw new BusinessException(ErrorCode.TS103, $"TripLogId {expanse.TripLogId} is wrong against advance id:{expanse.AdvanceId}");
                        }

                        if (exp.ExpenseTypeId <= 0)
                        {
                            exp.ExpenseTypeId = _repository.GetConfigValue<long>("DefaultFuelStockExpenseId");
                            if (exp.ExpenseTypeId <= 0)
                            {
                                throw new BusinessException(ErrorCode.GLB103, "Default Fuel Stock Consumption Expense name not Configured Hind:Key=>DefaultFuelStockExpenseId");
                            }
                        }

                        if (sat.TripExpenses.All(x => x.Id != exp.Id || exp.Id == 0))
                        {
                            sat.TripExpenses.Add(exp);
                            if (exp.Id > 0)
                            {
                                teRepo.Update(exp);
                            }
                            else
                            {
                                teRepo.Insert(exp);
                            }
                        }
                    }
                    adv.BalanceQty = adv.FuelQty - adv.FuelExpanses.Where(a => a.ObjectState != ObjectState.Deleted).Sum(a => a.FuelQty + a.ShortFuelQty);
                    adv.ObjectState = ObjectState.Modified;
                    advRepo.Update(adv);

                    //if (Math.Abs(totalexpamt) != 0)
                    //{
                    //    var fuelexpvd = new VoucherDetail
                    //    {
                    //        OfficeId = sat.OfficeId,
                    //        AccountId = adv.DebitAccountId.GetValueOrDefault(),
                    //        OrderId = 7,
                    //        Amount = -totalexpamt,
                    //        ObjectState = ObjectState.Added,
                    //        VoucherId = v.Id,
                    //        Voucher = v
                    //    };
                    //    v.VoucherDetails.Add(fuelexpvd);
                    //}
                }
            }
            #endregion
            #region Advance Reveresal
            await uow.ExecSqlQueryAsync($"UPDATE [tTripAdvanceLog] SET SettlementId=NULL {(tladvmappingflag > 0 ? "" : " ,TripLogId=NULL ")} WHERE SettlementId=@id AND AdvanceTypeId=94", new SqlParameter("id", sat.Id));
            var reverse = sat.vwTripAdvances?.Where(x => !x.IsDeleted && x.TypeId == 94).ToList();
            if (reverse.Any())
            {
                foreach (var r in reverse)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [tTripAdvanceLog] SET SettlementId={sat.Id},TripLogId={r.TripLogId} WHERE Id={r.Id} AND AdvanceTypeId=94");
                }

            }
            #endregion
            #region 1:Driver Cash Advances/Settled Balances Type Advances 1590: driver Cash Advance

            var cashadvids = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 1/*CashAdvance*/ || x.TypeId == 17/*Trip Settlement Balance*/ || x.TypeId == 16/*Driver Penalty*/|| x.TypeId == 91/*Driver Debit*/)).Select(x => x.Id).ToList() ?? new List<long>();
            var cashadvances = cashadvids.Any() ? await advRepo.Queryable().Include(x => x.SettledAdvances).Where(x => cashadvids.Contains(x.Id)).ToListAsync() : new List<TripAdvanceLog>();
            var vdr_repo = _repository.GetRepository<VoucherDetailReference>();

            if (cashadvids.Any())//Cash Advance and Cash Settlement Balance
            {
                var advtypes = cashadvances.Select(x => x.AdvanceTypeId).Distinct().ToArray();
                var advancetypes = await _repository.GetRepository<VoucherType>().Queryable().Where(x => advtypes.Contains(x.Id)).Select(x => new { x.Id, x.VoucherTypeName }).ToDictionaryAsync(x => x.Id, y => y.VoucherTypeName);
                foreach (var loggroup in cashadvances.GroupBy(x => new { x.DebitAccountId, x.AdvanceTypeId }))
                {
                    decimal multiplayfactor = 1;
                    var firstvdrId = loggroup.FirstOrDefault(z => z.VDRId > 0).VDRId;
                    if (firstvdrId != null && firstvdrId > 0)
                    {
                        var firstvdrInfo = await vdr_repo.Queryable().Where(x => x.Id == firstvdrId).Select(x => new { x.Amount }).FirstOrDefaultAsync();
                        if(firstvdrInfo!=null) multiplayfactor = firstvdrInfo.Amount>0?-1:1;
                    }
                    //var multiplayfactor = (loggroup.Key.AdvanceTypeId == 16||loggroup.Key.AdvanceTypeId== settlementPayAdvTypeId ? 1 : -1);/*In Accounts Driver Penalty and Driver Cash Deposit is negative in debit account so to get it settled we need to convert this vd in possitive because it the new ref is in negative*/
                    var settledamt = loggroup.SelectMany(x => (x.SettledAdvances ?? new List<TripAdvanceLog>()), (p, c) => new { c.CashAmount }).Sum(x => x.CashAmount);
                    var balanceamount = from log in loggroup
                                        join ad in sat.vwTripAdvances on log.Id equals ad.Id
                                        select new { Balance = settlementReceivedAdvTypeIds.Contains(log.AdvanceTypeId)? ad.SettAdvAmt : log.CashAmount };
                    var advtype = advancetypes[loggroup.Key.AdvanceTypeId ?? 0];
                    var vd = new VoucherDetail
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = loggroup.Key.DebitAccountId.GetValueOrDefault(),
                        OrderId = 1,
                        Amount = multiplayfactor * (balanceamount.Sum(x => x.Balance) - settledamt),
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id,
                        Voucher = v,
                        Particular = $"{advtype} got settled",
                        Narration = $"{advtype} got settled",
                    };
                    decimal onaccount = 0;
                    foreach (var log in loggroup)
                    {
                        var sadv = sat.vwTripAdvances.FirstOrDefault(x => x.Id == log.Id);
                        if (log.SettlementId > 0 && log.SettlementId != sat.Id)
                        {
                            throw new BusinessException(ErrorCode.TADV106, $"Reference No {log.ReferenceNo}");
                        }
                        log.SettlementId = settlementReceivedAdvTypeIds.Contains(log.AdvanceTypeId)  ? (long?)null : sat.Id;
                        log.TripLogId = settlementReceivedAdvTypeIds.Contains(log.AdvanceTypeId)? null : sadv?.TripLogId;
                        log.ObjectState = ObjectState.Modified;
                        if (log.VDRId > 0)
                        {
                            var vdrInfo = vdr_repo.Queryable().Where(x => x.Id == log.VDRId).Select(x => new { x.Amount, Consumed = x.AgainstReferences.DefaultIfEmpty().Sum(y => (decimal?)y.Amount) ?? 0, x.ReferenceNo }).FirstOrDefault();
                            //var vdrbalamt = (vdrInfo.Amount + vdrInfo.Consumed);/*Balance As per Accounts*/
                            var advbalamt = log.CashAmount - log.SettledAdvances.Sum(x => x.CashAmount);/*Balance as per fleet*/
                            var tobesettled = (settlementReceivedAdvTypeIds.Contains(log.AdvanceTypeId)  ? sadv.SettAdvAmt : log.CashAmount);
                            if ((vdrInfo.Amount > 0 && (vdrInfo.Amount + (multiplayfactor * tobesettled) + vdrInfo.Consumed) < 0) ||
                                    (vdrInfo.Amount < 0 && (vdrInfo.Amount + tobesettled + vdrInfo.Consumed) > 0))
                            {
                                throw new BusinessException(ErrorCode.VCH109, $"Reference no {vdrInfo.ReferenceNo} balance getting settled ({vdrInfo.Amount})+({(multiplayfactor * tobesettled)})={(vdrInfo.Amount + (multiplayfactor * tobesettled) + vdrInfo.Consumed)}, factor is {multiplayfactor}");
                            }
                            if (advbalamt < tobesettled)
                            {
                                throw new BusinessException(ErrorCode.GLB106, $"Insufficient balance amount available for adjustment of Advance Number {log.VoucherNo}");
                            }
                            var vdr = new VoucherDetailReference()
                            {
                                Amount = multiplayfactor * (settlementReceivedAdvTypeIds.Contains(log.AdvanceTypeId) ? sadv.SettAdvAmt : (log.CashAmount - log.SettledAdvances.Sum(x => x.CashAmount))),
                                ObjectState = ObjectState.Added,
                                RefId = log.VDRId,
                                VDRTypeId = 1014,   //Against Reference
                                VoucherDetailId = vd.Id,
                                fk_VoucherDetail = vd,
                                ReferenceNo = log.ReferenceNo,
                                AccountId = vd.AccountId,
                                DueDate = sat.SettleDate ?? log.AdvanceDate
                            };
                            vd.VoucherDetailReferences.Add(vdr);
                        }
                        else
                        {
                            onaccount += multiplayfactor * (settlementReceivedAdvTypeIds.Contains(log.AdvanceTypeId) ? sadv.SettAdvAmt : (log.CashAmount - log.SettledAdvances.Sum(x => x.CashAmount)));
                        }
                        sat.TripAdvances.Add(log);
                    }

                    if (onaccount > 0 && !vd.VoucherDetailReferences.Any())
                    {
                        var onaccountvdr = new VoucherDetailReference()
                        {
                            Amount = onaccount,
                            ObjectState = ObjectState.Added,
                            RefId = null,
                            VDRTypeId = 1448,   //On Account
                            VoucherDetailId = vd.Id,
                            fk_VoucherDetail = vd,
                            ReferenceNo = null,
                            AccountId = vd.AccountId,
                            DueDate = sat.SettleDate ?? DateTime.Now
                        };
                        vd.VoucherDetailReferences.Add(onaccountvdr);
                    }

                    if (vd.VoucherDetailReferences.Any() && vd.Amount != vd.VoucherDetailReferences.Sum(x => x.Amount))//I am facing error 
                    {

                        throw new BusinessException(ErrorCode.VCH106, $"[ErrorSource:VTSS]Few Cash Advance/SettledBalance are not having Against Ref Transaction. Hint: OrderId:{vd.OrderId},AccountId:{vd.AccountId}");
                    }
                    v.VoucherDetails.Add(vd);
                }
            }
            if (!isNew)
            {
                await uow.ExecSqlQueryAsync(
                    $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId=NULL  {(tladvmappingflag > 0 ? "" : " ,TripLogId=NULL ")} WHERE SettlementId={sat.Id} AND AdvanceTypeId in(1,17,16,91) {(cashadvids.Any() ? $"AND Id NOT IN({cashadvids.JoinStrings(",")})" : "")}");
            }
            #endregion

            #region 88:e-Toll Payment
            var etollexpmaster = await uow.RepositoryAsync<ExpenseMaster>().Queryable().FirstOrDefaultAsync(x => x.NatureId == 1617);

            if (etollexpmaster == null)
            {
                throw new BusinessException(ErrorCode.TS103, $"eToll Expense Master is not defined.");
            }

            if (etollexpmaster.LedgerId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.TS103, $"eToll Expense Master is not mapped with Expense Ledger Hint.Expense Name:{etollexpmaster.Name}");
            }

            var eTotaladvids = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 88)).ToList() ?? new List<Advance>();
            if (eTotaladvids.Any())
            {
                if (sat.vwTripAdvances.Any(x => !x.IsDeleted && x.TypeId == 88 && x.TripLogId.GetValueOrDefault() == 0))
                {
                    throw new BusinessException(ErrorCode.GLB106, "One of eToll is not mapped to any of TripLog included this settlement.");
                }

                var etollids = eTotaladvids.Select(y => y.Id).ToList();
                var etoll = (await advRepo.Queryable().Where(x => etollids.Contains(x.Id))
                    .SumAsync(x => (decimal?)x.CashAmount)) ?? 0;
                var etollexp = sat.vwTripExpenses.Where(x => !x.IsDeleted && x.ExpNatureId == 1617)
                    .Sum(x => (decimal?)x.SettledAmt) ?? 0;
                if (etollexp !=
                    etoll)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"eToll Expense Amount does not match with sum of eToll Payments included in this Settlement.\n eToll Expense Amount is Rs.{etollexp} and eToll Payment Total is Rs.{etoll}");
                }
                var etollidscomma = etollids.JoinStrings(",");
                await _repository.UOW.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET TripLogId=NULL,SettlementId=NULL WHERE SettlementId=@arg1 and AdvanceTypeId=88 and Id not in({etollidscomma})", new SqlParameter("arg1", sat.Id));


                foreach (var l in eTotaladvids.GroupBy(x => x.TripLogId))
                {
                    var ids = l.Select(x => x.Id).ToArray().JoinStrings(",");
                    await _repository.UOW.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET TripLogId=@arg1,SettlementId=@arg2 WHERE AdvanceTypeId=88 AND Id in({ids})", new SqlParameter("arg1", ((object)l.Key ?? DBNull.Value)), new SqlParameter("arg2", sat.Id));
                }
                var etolladv = advRepo.Queryable().Where(x => etollids.Contains(x.Id)).ToList();
                foreach (var fsac in etolladv.GroupBy(y => y.DebitAccountId))
                {
                    /*VD for eToll Control A/c (-)*/
                    var vd = new VoucherDetail()
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = fsac.Key.GetValueOrDefault(),
                        OrderId = 11,
                        Amount = -fsac.Sum(x => x.Amount),
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id,
                        Voucher = v
                    };
                    v.VoucherDetails.Add(vd);
                }
                var etoll_expvd = new VoucherDetail()
                {
                    OfficeId = sat.OfficeId,
                    AccountId = etollexpmaster.LedgerId.GetValueOrDefault(),//?? Expense Account Kaise Pata kare
                    OrderId = 12,
                    ObjectState = ObjectState.Added,
                    VoucherId = v.Id,
                    Voucher = v,
                    Amount = etoll
                };
                v.VoucherDetails.Add(etoll_expvd);
            }
            #endregion
            #region 6:Driver Settlement Net Balance Payoff 

            var cashnetbalances = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 86));
            var drvsetnetbalpayoff = cashnetbalances.Select(x => x.Id).ToList() ?? new List<long>();
            var query86 = advRepo.Queryable().Where(x => drvsetnetbalpayoff.Contains(x.Id) && (!x.SettledAdvances.Any() || (x.CashAmount - x.SettledAdvances.Sum(y => y.CashAmount + y.FuelAmount)) > 0));
            var settnetbalpayoff = drvsetnetbalpayoff.Any() ? await query86.ToListAsync() : new List<TripAdvanceLog>();
            var balances86 = drvsetnetbalpayoff.Any() ? await query86.Select(x => new { x.Id, x.CreditAccountId, Amount = (x.FuelQty > 0 ? x.FuelAmount : x.CashAmount), SettledAmt = x.SettledAdvances.Sum(y => (decimal?)(y.FuelQty > 0 ? y.FuelAmount : y.CashAmount)) ?? 0 }).ToListAsync() : null;
            if (drvsetnetbalpayoff.Count() != settnetbalpayoff.Count())
            {
                throw new BusinessException(ErrorCode.GLB106, "One or more Driver Net balance deposit was not found or was partially intiated using Driver Deposit Refund.");
            }
            if (drvsetnetbalpayoff.Any())
            {
                foreach (var loggroup in settnetbalpayoff.GroupBy(x => x.CreditAccountId))
                {
                    var firstQuery = balances86?.Where(x => x.CreditAccountId == loggroup.Key);
                    var vd = new VoucherDetail()
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = loggroup.Key.GetValueOrDefault(),
                        OrderId = 6,
                        Amount = /*loggroup.Sum(x => x.CashAmount)-*/ (firstQuery?.Sum(x => x.Amount - x.SettledAmt) ?? loggroup.Sum(x => x.CashAmount)),
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id,
                        Voucher = v
                    };
                    decimal onaccount = 0;
                    foreach (var log in loggroup)
                    {
                        var first = balances86?.FirstOrDefault(x => x.Id == log.Id);
                        var sadv = sat.vwTripAdvances.FirstOrDefault(x => x.Id == log.Id);
                        if (log.SettlementId > 0 && log.SettlementId != sat.Id)
                        {
                            throw new BusinessException(ErrorCode.TADV106, $"Reference No {log.ReferenceNo}");
                        }
                        log.SettlementId = sat.Id;
                        log.TripLogId = sadv?.TripLogId;
                        log.ObjectState = ObjectState.Modified;
                        if (log.VDRId > 0)
                        {
                            var vdr = new VoucherDetailReference()
                            {
                                Amount = (first?.Amount - first?.SettledAmt) ?? log.Amount,
                                ObjectState = ObjectState.Added,
                                RefId = log.VDRId,
                                VDRTypeId = 1014,   //Against Reference
                                VoucherDetailId = vd.Id,
                                fk_VoucherDetail = vd,
                                ReferenceNo = log.ReferenceNo,
                                AccountId = vd.AccountId,
                                DueDate = sat.SettleDate ?? log.AdvanceDate
                            };
                            vd.VoucherDetailReferences.Add(vdr);
                        }
                        else
                        {
                            onaccount += (first?.Amount - first?.SettledAmt) ?? log.Amount;
                        }
                        sat.TripAdvances.Add(log);
                    }
                    if (onaccount > 0 && vd.VoucherDetailReferences.Any())
                    {
                        var onaccountvdr = new VoucherDetailReference()
                        {
                            Amount = -onaccount,
                            ObjectState = ObjectState.Added,
                            RefId = null,
                            VDRTypeId = 1448,   //On Account
                            VoucherDetailId = vd.Id,
                            fk_VoucherDetail = vd,
                            ReferenceNo = null,
                            AccountId = vd.AccountId,
                            DueDate = sat.SettleDate ?? DateTime.Now
                        };
                        vd.VoucherDetailReferences.Add(onaccountvdr);
                    }
                    if (vd.VoucherDetailReferences.Any() && vd.Amount != vd.VoucherDetailReferences.Sum(x => x.Amount))
                    {
                        throw new BusinessException(ErrorCode.VCH106, "[ErrorSource:VTSS]Few NetBalance Cash Deposit Payoff's are not having Against Ref Transaction.");
                    }
                    v.VoucherDetails.Add(vd);
                }
            }
            if (!isNew)
            {
                await uow.ExecSqlQueryAsync(
                    $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId=NULL  {(tladvmappingflag > 0 ? "" : " ,TripLogId=NULL ")}  WHERE SettlementId={sat.Id} AND AdvanceTypeId=86 {(drvsetnetbalpayoff.Any() ? $"AND Id NOT IN({drvsetnetbalpayoff.JoinStrings(",")})" : "")}");
            }
            #endregion

            #region 2: Driver Fuel Advances: 1591
            var fueladv = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 2 || x.TypeId == 85)).Select(x => x.Id).ToList() ?? new List<long>();
            if (!isNew)//iF SETTLEMENT IS GETTING UPDATED THAN UNMAP ALL FUEL ADVANCES THOSE WERE PREVIOUSALY MAPPED BUT NOT NOW
            {
                await uow.ExecSqlQueryAsync(
                $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId=NULL  {(tladvmappingflag > 0 ? "" : " ,TripLogId=NULL ")}  WHERE SettlementId={sat.Id} AND AdvanceTypeId in(2,85) {(fueladv.Any() ? $"AND Id NOT IN({fueladv.JoinStrings(",")})" : "")}");
            }
            if (fueladv.Any())
            {
                var fueladvs = await advRepo.Queryable().Include(x => x.SettledAdvances).Where(x => fueladv.Contains(x.Id)).ToListAsync();

                var crfueladv = fueladvs.Where(x => x.FuelAmount - x.SettledAdvances.Sum(z => z.FuelAmount) > sat.vwTripExpenses.Where(y => y.TripAdvanceLogId == x.Id).Sum(y => y.SettledAmt)).ToList();
                var drfueladv = fueladvs.Where(x => x.FuelAmount - x.SettledAdvances.Sum(z => z.FuelAmount) < sat.vwTripExpenses.Where(y => y.TripAdvanceLogId == x.Id).Sum(y => y.SettledAmt)).ToList();
                if (crfueladv.Any())//If Fuel Paid was greater than fuel expense
                {
                    foreach (var fuelgroup in crfueladv.GroupBy(x => x.DebitAccountId))
                    {
                        var settledamt = fuelgroup.SelectMany(x => x.SettledAdvances ?? new List<TripAdvanceLog>(), (p, c) => new { c.FuelAmount }).Sum(x => x.FuelAmount);
                        var tobesettledexp = sat.vwTripExpenses.Where(x => fuelgroup.Select(y => (long?)y.Id).Contains(x.TripAdvanceLogId));
                        var amt1 = fuelgroup.Sum(x => x.FuelAmount) - settledamt;
                        var amt2 = tobesettledexp.Sum(x => x.SettledAmt);
                        var vd = new VoucherDetail()
                        {
                            OfficeId = sat.OfficeId,
                            AccountId = fuelgroup.Key.GetValueOrDefault(),
                            OrderId = 2,
                            Amount = -(amt1 - amt2), //-(fuelgroup.Sum(x => x.FuelAmount)-tobesettledexp.Sum(x=>x.SettledAmt)),
                            ObjectState = ObjectState.Added,
                            VoucherId = v.Id,
                            Voucher = v
                        };
                        foreach (var log in fuelgroup)
                        {
                            var sadv = tobesettledexp.FirstOrDefault(x => x.TripAdvanceLogId == log.Id);
                            var existingadv = sat.vwTripAdvances.FirstOrDefault(x => x.Id == log.Id);
                            if (log.SettlementId > 0 && log.SettlementId != sat.Id)
                            {
                                throw new BusinessException(ErrorCode.TADV106, $"Reference No {log.ReferenceNo}");
                            }
                            log.SettlementId = sat.Id;
                            if (existingadv?.TripLogId > 0)
                            {
                                log.TripLogId = existingadv.TripLogId;
                            }
                            log.ObjectState = ObjectState.Modified;
                            if (log.VDRId > 0)
                            {
                                var vdramt1 = log.FuelAmount;
                                var vdramt2 = sadv?.SettledAmt ?? 0;
                                var vdr = new VoucherDetailReference()
                                {
                                    Amount = -(vdramt1 - vdramt2), //-(log.FuelAmount- sadv?.SettledAmt??0),
                                    RefId = log.VDRId,
                                    VDRTypeId = 1014,//Against Reference
                                    VoucherDetailId = vd.Id,
                                    fk_VoucherDetail = vd,
                                    ReferenceNo = log.ReferenceNo,
                                    AccountId = vd.AccountId,
                                    DueDate = sat.SettleDate ?? log.AdvanceDate,
                                    ObjectState = ObjectState.Added
                                };
                                vd.VoucherDetailReferences.Add(vdr);
                            }
                            sat.TripAdvances.Add(log);
                        }
                        v.VoucherDetails.Add(vd);
                    }
                }

                if (drfueladv.Any())//If Fuel Paid was less than fuel expense
                {
                    Dictionary<long, decimal> amts = new Dictionary<long, decimal> { };

                    var fuelexpgroup = sat.vwTripExpenses.Where(x => !x.IsDeleted && drfueladv.Select(y => (long?)y.Id).Contains(x.TripAdvanceLogId)).GroupBy(x => x.AccountId);
                    foreach (var expgr in fuelexpgroup)
                    {
                        if (expgr.Key.GetValueOrDefault() == 0)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "One of Expense does not have Expense Account Mapped");
                        }
                        var advids = expgr.Select(x => x.TripAdvanceLogId.GetValueOrDefault()).Distinct();
                        decimal vdamount = 0;
                        foreach (var item in advids)
                        {
                            var thisgrpexp = expgr.Where(x => x.TripAdvanceLogId == item).Sum(x => x.SettledAmt);
                            if (!amts.ContainsKey(item))
                            {
                                var log = drfueladv.FirstOrDefault(x => x.Id == item);
                                if (log.SettlementId > 0 && log.SettlementId != sat.Id)
                                {
                                    throw new BusinessException(ErrorCode.TADV106, $"Reference No {log.ReferenceNo}");
                                }
                                log.SettlementId = sat.Id;
                                log.TripLogId = sat.vwTripAdvances.FirstOrDefault(x => x.Id == log.Id)?.TripLogId;
                                log.ObjectState = ObjectState.Modified;
                                sat.TripAdvances.Add(log);
                                var thisadvexpamt = sat.vwTripExpenses.Where(x => !x.IsDeleted && advids.Contains(x.TripAdvanceLogId.GetValueOrDefault())).Sum(x => x.SettledAmt);

                                var thisadvtotaldiff = Math.Abs(log.FuelAmount - thisadvexpamt);
                                amts[log.Id] = thisadvtotaldiff;
                            }
                            decimal amt = 0;
                            if (amts.ContainsKey(item) && amts[item] - thisgrpexp >= 0) amt = thisgrpexp;
                            else if (amts.ContainsKey(item) && amts[item] - thisgrpexp < 0) amt = amts[item];
                            if (amt == 0) continue;
                            if (amts.ContainsKey(item))
                            {
                                amts[item] -= amt;
                                vdamount += amt;
                            }

                        }
                        if (vdamount > 0)
                        {
                            var vd = new VoucherDetail()
                            {
                                OfficeId = sat.OfficeId,
                                AccountId = expgr.Key.GetValueOrDefault(),
                                OrderId = 2,
                                Amount = Math.Abs(vdamount),
                                ObjectState = ObjectState.Added,
                                VoucherId = v.Id,
                                Voucher = v
                            };
                            v.VoucherDetails.Add(vd);
                        }

                    }
                }
                foreach (var adv in fueladvs)
                {
                    var fadv = sat.vwTripAdvances?.FirstOrDefault(x => x.Id == adv.Id);
                    if (fadv?.TripLogId > 0)
                    {
                        adv.TripLogId = fadv.TripLogId;
                    }
                    adv.SettlementId = sat.Id;
                    adv.ObjectState = ObjectState.Modified;
                }
            }
            #endregion

            #region 3:Trip Expances Voucher Details
            var excludedtypes = new long[] { 2, 85 };
            if (sat.vwTripExpenses.Any(x => (x.TripAdvanceLogId.GetValueOrDefault() == 0 || sat.vwTripAdvances.Any(y => y.Id == x.TripAdvanceLogId && !excludedtypes.Contains(y.TypeId))) && x.ExpNatureId != 1617 && !x.IsDeleted && x.AccountId.GetValueOrDefault() == 0))
            {
                throw new BusinessException(ErrorCode.GLB106, "Account is not mapped on expance type");
            }

            var expgroup = sat.vwTripExpenses?.Where(x => (x.TripAdvanceLogId.GetValueOrDefault() == 0 || sat.vwTripAdvances.Any(y => y.Id == x.TripAdvanceLogId && !excludedtypes.Contains(y.TypeId))) && x.ExpNatureId != 1617 && !x.IsDeleted)?.GroupBy(x => x.AccountId).ToList();
            if (expgroup != null)
            {
                foreach (var exp in expgroup)
                {
                    var vd = new VoucherDetail()
                    {
                        OfficeId = sat.OfficeId,
                        AccountId = exp.Key.GetValueOrDefault(),
                        OrderId = 3,
                        Amount = exp.Sum(x => x.SettledAmt),
                        ObjectState = ObjectState.Added,
                        VoucherId = v.Id,
                        Voucher = v
                    };
                    v.VoucherDetails.Add(vd);
                }
            }
            #endregion

            #region Trip Log Mapping  : 1:VehicleMovementlog
            var tlids = sat.vwTripLogs?.Where(x => !x.IsDeleted).Select(x => x.Id).ToList();
            if (tlids != null && tlids.Any())
            {
                var invalids = sat.TripAdvances.Where(x => x.AdvanceTypeId != 17 && (x.TripLogId == null || !tlids.Contains((long)x.TripLogId))).ToList();
                if (invalids.Any())
                {
                    var invalidrefs = invalids.Select(x => x.ReferenceNo).JoinStrings(",");
                    throw new BusinessException(ErrorCode.GLB106, $"{invalidrefs} Attached Trip Cash Advance is either not mapped to any TripLog or is Mapped to TripLog that is not in this Settlement.");
                }
                if (!isNew)
                {
                    await uow.ExecSqlQueryAsync(
                        $"UPDATE [dbo].[tVehicleMovementLog] SET SettlementId=NULL WHERE SettlementId={sat.Id};");
                }
                foreach (var tl in sat.vwTripLogs?.Where(x => !x.IsDeleted).ToList())
                {
                    await uow.ExecSqlQueryAsync(
                 $"UPDATE [dbo].[tVehicleMovementLog] SET SettlementId={sat.Id},KmRunAdd={tl.AddKM} WHERE Id={tl.Id}");
                }

            }

            #endregion

            #region Cash Deposit VD
            if (sat.CashDeposited > 0)
            {
                var vd = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = sat.CashDepositAccId > 0 ? sat.CashDepositAccId.GetValueOrDefault() : sat.SettlementAccountId.GetValueOrDefault(0),
                    Amount = sat.CashDeposited,
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 4,
                    VoucherId = v.Id
                };
                v.VoucherDetails.Add(vd);
            }
            if (sat.CashPaid > 0)
            {
                if (sat.CashPaidAccId.GetValueOrDefault() <= 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Cash Paid Account is Required when Cash Paid Amount is greater than Zero.");
                }
                var vd = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = generateCashPaidAdvance == 0 ? (sat.CashPaidAccId > 0 ? sat.CashPaidAccId.GetValueOrDefault() : sat.SettlementAccountId.GetValueOrDefault(0)) : DefaultTruckControlAccountId,
                    Amount = -sat.CashPaid,
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 4,
                    VoucherId = v.Id
                };
                if (vd.AccountId == 0)
                {
                    vd.AccountId = _repository.GetConfigValue<long>("DefaultSettledAccountId");
                }
                v.VoucherDetails.Add(vd);
                if (generateCashPaidAdvance > 0)
                {
                    if (sat.CashPaidAccId.GetValueOrDefault() <= 0)
                    {
                        throw new BusinessException("cashpaidaccountrequired", sat.TripSheetNo);
                    }
                    PrepareCashPaidAdvance(ref sat, sat.CashPaidAccId.GetValueOrDefault(), vd.AccountId, vRepo, generateCashPaidAdvance);
                }
            }
            #endregion

            #region Settlement voucher
            v.ViewId = sat.ViewId;
            v.OfficeId = sat.OfficeId;
            v.VoucherNo = sat.TripSheetNo;
            v.VoucherDate = sat.SettleDate.GetValueOrDefault(DateTime.Now);
            v.VoucherDateTime = sat.SettleDate.Value;
            v.VoucherAmount = v.VoucherDetails.Where(x => x.Amount > 0).Sum(x => x.Amount);//s.fk_Voucher.VoucherDetails.Sum(x => x.Amount);
            v.VoucherTypeId = 18;

            sat.ObjectState = ObjectState.Modified;
            sat.fk_Voucher = v;
            sat.VoucherId = v.Id;
            vRepo.Attach(v);
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;

            #endregion

            #region Settlement Balance Voucher
            if (netbalanceadvances == 1 && netbalance != 0 && sat.SettleDate != null)
            {
                var baladv = new TripAdvanceLog
                {
                    AdvanceDate = sat.SettleDate.Value,
                    FuelAmount = 0,
                    FuelQty = 0,
                    CashAmount = Math.Abs(netbalance),
                    OfficeId = sat.OfficeId,
                    CreditAccountId = DefaultTruckControlAccountId,
                    FuelRate = 0,
                    DebitAccountId = (settlacctype == 4 || settlacctype == 1)/*Maintain In Driver ledger*/? (sat.SettlementAccountId ?? sat.Driver1Id) : DefaultTruckControlAccountId,
                    DriverId = sat.Driver1Id,
                    FuelId = null,
                    TripLogId = null,
                    VehicleId = null,
                    IsBulkEntry = false
                };

                switch (sat.AdjustmentTypeId)
                {
                    case 1592: //both
                        #region Fuel
                        if (sat.FuelAmountDifference > 0)/*[Receive]*/
                        {
                            #region TripAdvance Fuel

                            var adv1592 = baladv.Clone();
                            adv1592.Remark = "Trip Settlement fuel Balance Carry Forwarded as driver was unable to pay cash back to company.";
                            adv1592.ReferenceNo = "TSFBL-" + sat.TripSheetNo;
                            adv1592.VoucherNo = "TSFBL-" + sat.TripSheetNo;
                            adv1592.CashAmount = 0;
                            adv1592.FuelQty = sat.FuelQtyDifference;
                            adv1592.FuelAmount = sat.FuelAmountDifference;
                            adv1592.AdvanceTypeId = 85;
                            adv1592.FuelRate = sat.NetBalanceFuelRate;
                            #endregion End TripAdvance Fuel

                            #region Voucher Fuel
                            var v1592Fuel = new Voucher()
                            {
                                Id = sat.SetlBalFuelVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 85,
                                OfficeId = sat.OfficeId,
                                VoucherAmount = Math.Abs(sat.FuelAmountDifference),
                                VoucherNo = adv1592.VoucherNo,
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                Account1Id = adv1592.DebitAccountId,
                                Amount1 = Math.Abs(sat.FuelAmountDifference),
                                Account2Id = adv1592.CreditAccountId,
                                Amount2 = -Math.Abs(sat.FuelAmountDifference),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added
                            };
                            adv1592.VoucherId = v1592Fuel.Id;
                            adv1592.fk_Voucher = v1592Fuel;
                            #endregion

                            #region VD1 Fuel
                            var v1592Fuel1 = new VoucherDetail()
                            {
                                AccountId = v1592Fuel.Account1Id.GetValueOrDefault(),
                                Amount = v1592Fuel.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                VoucherId = v1592Fuel.Id,
                                Voucher = v1592Fuel,
                                ObjectState = ObjectState.Added,
                            };
                            v1592Fuel.VoucherDetails.Add(v1592Fuel1);
                            #endregion

                            #region VDR
                            var v1592fvdr = new VoucherDetailReference()
                            {
                                Amount = v1592Fuel1.Amount,
                                ReferenceNo = sat.TripSheetNo,
                                VDRTypeId = 1013, //New Reference
                                VoucherDetailId = v1592Fuel1.Id,
                                fk_VoucherDetail = v1592Fuel1,
                                ObjectState = ObjectState.Added
                            };
                            v1592Fuel1.VoucherDetailReferences.Add(v1592fvdr);

                            adv1592.VDRId = v1592fvdr.Id;
                            adv1592.fk_VDR = v1592fvdr;
                            adv1592.ObjectState = ObjectState.Added;
                            advRepo.Insert(adv1592);
                            #endregion

                            #region VD2 Fuel
                            var v1592Fuel2 = new VoucherDetail()
                            {
                                VoucherId = v1592Fuel.Id,
                                Voucher = v1592Fuel,
                                AccountId = v1592Fuel.Account2Id.GetValueOrDefault(),
                                Amount = v1592Fuel.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                            };
                            v1592Fuel.VoucherDetails.Add(v1592Fuel2);
                            #endregion
                            sat.fk_SetlBalFuelVoucher = v1592Fuel;
                            sat.SetlBalFuelVoucherId = v1592Fuel.Id;
                            sat.ObjectState = ObjectState.Modified;
                            vRepo.Insert(v1592Fuel);
                        }
                        #endregion

                        #region Cash
                        if (sat.SettledAmount > 0)/*[Receive]*/
                        {
                            #region TripAdvance Cash
                            baladv.Remark = "Trip Settlement Cash Balance Carry Forwarded as driver was unable to pay back to company.";
                            baladv.ReferenceNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.AdvanceTypeId = 17;
                            baladv.CashAmount = sat.SettledAmount;
                            #endregion

                            #region Voucher Cash
                            var v1592Cash = new Voucher()
                            {
                                Id = sat.SetlBalVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 17,
                                OfficeId = sat.OfficeId,
                                VoucherNo = baladv.VoucherNo,
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                VoucherAmount = Math.Abs(sat.SettledAmount),
                                Account1Id = baladv.DebitAccountId,
                                Amount1 = Math.Abs(sat.SettledAmount),
                                Account2Id = baladv.CreditAccountId,
                                Amount2 = -Math.Abs(sat.SettledAmount),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added,
                                AccountingRemark = baladv.Remark
                            };
                            baladv.VoucherId = v1592Cash.Id;
                            baladv.fk_Voucher = v1592Cash;
                            #endregion

                            #region VD1 Cash
                            var v1592Cash1 = new VoucherDetail()
                            {
                                VoucherId = v1592Cash.Id,
                                Voucher = v1592Cash,
                                AccountId = v1592Cash.Account1Id.GetValueOrDefault(),
                                Amount = v1592Cash.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark
                            };
                            v1592Cash1.ObjectState = ObjectState.Added;
                            v1592Cash.VoucherDetails.Add(v1592Cash1);
                            #endregion

                            #region VDR
                            var v1592cash = new VoucherDetailReference()
                            {
                                VoucherDetailId = v1592Cash1.Id,
                                fk_VoucherDetail = v1592Cash1,
                                Amount = Math.Abs(v1592Cash1.Amount),
                                ReferenceNo = sat.TripSheetNo,
                                VDRTypeId = 1013, //New Reference
                                ObjectState = ObjectState.Added
                            };
                            v1592Cash1.VoucherDetailReferences.Add(v1592cash);

                            baladv.VDRId = v1592cash.Id;
                            baladv.fk_VDR = v1592cash;
                            baladv.ObjectState = ObjectState.Added;
                            advRepo.Insert(baladv);
                            #endregion

                            #region VD2 Cash
                            var v1592Cash2 = new VoucherDetail()
                            {
                                VoucherId = v1592Cash.Id,
                                Voucher = v1592Cash,
                                AccountId = v1592Cash.Account2Id.GetValueOrDefault(),
                                Amount = v1592Cash.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark
                            };
                            v1592Cash.VoucherDetails.Add(v1592Cash2);
                            #endregion
                            sat.fk_SetlBalVoucher = v1592Cash;
                            sat.SetlBalVoucherId = v1592Cash.Id;
                            vRepo.Insert(v1592Cash);
                        }
                        #endregion
                        break;

                    case 1590:// Net As Cash Adv
                        if (netbalance > 0)
                        {
                            #region TripAdvance
                            baladv.Remark = "Trip Settlement Cash Balance Carry Forwarded as driver was unable to pay cash back to company.";
                            baladv.ReferenceNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.AdvanceTypeId = 17;
                            baladv.CashAmount = netbalance;
                            #endregion

                            #region Voucher
                            var v1590 = new Voucher()
                            {
                                Id = sat.SetlBalVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 17,
                                OfficeId = sat.OfficeId,
                                VoucherNo = baladv.VoucherNo,
                                VoucherAmount = Math.Abs(netbalance),
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                Account1Id = baladv.DebitAccountId,
                                Amount1 = Math.Abs(netbalance),
                                Account2Id = baladv.CreditAccountId,
                                Amount2 = -Math.Abs(netbalance),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added,
                                AccountingRemark = baladv.Remark
                            };
                            baladv.VoucherId = v1590.Id;
                            baladv.fk_Voucher = v1590;

                            #endregion

                            #region VD1 Cash
                            var v15901 = new VoucherDetail()
                            {
                                VoucherId = v1590.Id,
                                Voucher = v1590,
                                AccountId = v1590.Account1Id.GetValueOrDefault(),
                                Amount = v1590.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark
                            };
                            v15901.ObjectState = ObjectState.Added;
                            v1590.VoucherDetails.Add(v15901);
                            #endregion

                            #region VDR

                            var v1591vdr = new VoucherDetailReference()
                            {
                                VoucherDetailId = v15901.Id,
                                fk_VoucherDetail = v15901,
                                Amount = Math.Abs(v15901.Amount),
                                ReferenceNo = sat.TripSheetNo,
                                VDRTypeId = 1013, //New Reference
                                ObjectState = ObjectState.Added
                            };
                            v15901.VoucherDetailReferences.Add(v1591vdr);

                            baladv.VDRId = v1591vdr.Id;
                            baladv.fk_VDR = v1591vdr;
                            baladv.ObjectState = ObjectState.Added;
                            advRepo.Insert(baladv);
                            #endregion

                            #region VD2 Cash

                            var v15902 = new VoucherDetail()
                            {
                                VoucherId = v1590.Id,
                                Voucher = v1590,
                                AccountId = v1590.Account2Id.GetValueOrDefault(),
                                Amount = v1590.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark
                            };
                            v1590.VoucherDetails.Add(v15902);
                            #endregion
                            sat.fk_SetlBalVoucher = v1590;
                            sat.SetlBalVoucherId = v1590.Id;
                            vRepo.Insert(v1590);
                        }
                        break;

                    case 1591://Net As Fuel Adv

                        if (netbalance > 0)
                        {
                            #region TripAdvance
                            baladv.Remark = "Trip Settlement fuel Balance Carry Forwarded as driver was unable to pay back to company.";
                            baladv.ReferenceNo = "TSFBL-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TSFBL-" + sat.TripSheetNo;
                            baladv.CashAmount = 0;
                            baladv.FuelQty = sat.FuelQtyDifference;
                            baladv.AdvanceTypeId = 85;
                            baladv.FuelAmount = netbalance;
                            baladv.FuelRate = sat.NetBalanceFuelRate;
                            #endregion

                            #region Voucher Fuel

                            var v1591 = new Voucher()
                            {
                                Id = sat.SetlBalFuelVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 85,
                                OfficeId = sat.OfficeId,
                                VoucherNo = "FUEL-" + baladv.VoucherNo,
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                VoucherAmount = Math.Abs(netbalance),
                                Account1Id = baladv.DebitAccountId,
                                Amount1 = Math.Abs(netbalance),
                                Account2Id = baladv.CreditAccountId,
                                Amount2 = -Math.Abs(netbalance),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added
                            };
                            baladv.VoucherId = v1591.Id;
                            baladv.fk_Voucher = v1591;

                            #endregion

                            #region VD1 Fuel

                            var v15911 = new VoucherDetail()
                            {
                                AccountId = v1591.Account1Id.GetValueOrDefault(),
                                Amount = v1591.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                VoucherId = v1591.Id,
                                Voucher = v1591,
                                ObjectState = ObjectState.Added,
                            };
                            v1591.VoucherDetails.Add(v15911);
                            #endregion

                            #region VDR

                            var v15911fvdr = new VoucherDetailReference()
                            {
                                Amount = v15911.Amount,
                                ReferenceNo = sat.TripSheetNo,
                                VDRTypeId = 1013, //New Reference
                                VoucherDetailId = v15911.Id,
                                fk_VoucherDetail = v15911,
                                ObjectState = ObjectState.Added
                            };
                            v15911.VoucherDetailReferences.Add(v15911fvdr);
                            baladv.VDRId = v15911fvdr.Id;
                            baladv.fk_VDR = v15911fvdr;
                            baladv.ObjectState = ObjectState.Added;
                            advRepo.Insert(baladv);
                            #endregion

                            #region VD2 Fuel
                            var v15912 = new VoucherDetail()
                            {
                                VoucherId = v1591.Id,
                                Voucher = v1591,
                                AccountId = v1591.Account2Id.GetValueOrDefault(),
                                Amount = v1591.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                            };
                            v1591.VoucherDetails.Add(v15912);
                            #endregion
                            sat.fk_SetlBalFuelVoucher = v1591;
                            sat.SetlBalFuelVoucherId = v1591.Id;
                            sat.ObjectState = ObjectState.Modified;
                            vRepo.Insert(v1591);
                        }
                        break;
                }

            }

            #endregion

            #region Driver Balance Pending
            var NetBalanceCarryAmt = _repository.GetConfigValue<int>("NetBalanceCarryForwordAmount");

            var satamt = NetBalanceCarryAmt == 0 ? netbalance : (sat.SettledAmount - sat.CashDeposited + sat.CashPaid);
            if (sat.Id == 0 || sat.CreatedDOE >= new DateTime(2022, 04, 01)) sat.NetBalancePending = true;
            if (SettPayoffRule > 1 && sat.NetBalancePending && satamt < 0/*If Net Balance is negative it mean we have to pay to driver*/)
            {
                #region Driver Pending Advance

                var drvrbalancepending = new TripAdvanceLog
                {
                    AdvanceDate = sat.SettleDate.Value,
                    FuelAmount = 0,
                    FuelQty = 0,
                    AdvanceTypeId = 86,
                    Remark = "Pending Balance Carry Forwarded as company was unable to pay back to Driver.",
                    ReferenceNo = "TBPND-" + sat.TripSheetNo,
                    VoucherNo = "TBPND-" + sat.TripSheetNo,
                    CashAmount = Math.Abs(satamt),
                    OfficeId = sat.OfficeId,
                    CreditAccountId = DefaultTruckControlAccountId,
                    FuelRate = 0,
                    DebitAccountId = SettPayoffRule == 2 ? vd5.AccountId : DefaultSettlementNetBalancePayoffAccount,
                    DriverId = sat.Driver1Id,
                    FuelId = null,
                    TripLogId = null,
                    VehicleId = null,
                    IsBulkEntry = false
                };
                //Settlement Balance Deposit

                #endregion Driver Pending Advance

                #region Voucher PayBalance

                var vchr = new Voucher()
                {
                    Id = sat.SetlBalVoucherId.GetValueOrDefault(),
                    VoucherTypeId = 86,
                    OfficeId = sat.OfficeId,
                    VoucherNo = drvrbalancepending.VoucherNo,
                    VoucherDate = sat.SettleDate.GetValueOrDefault(),
                    VoucherDateTime = sat.SettleDate.Value,
                    Account1Id = drvrbalancepending.DebitAccountId,
                    Amount1 = Math.Abs(satamt),
                    Account2Id = drvrbalancepending.CreditAccountId,
                    Amount2 = -Math.Abs(satamt),
                    IsAudited = false,
                    IsAccepted = false,
                    IsAccountsVisiblity = false,
                    PageId = null,
                    ViewId = sat.ViewId,
                    ObjectState = ObjectState.Added,
                    AccountingRemark = drvrbalancepending.Remark
                };
                drvrbalancepending.VoucherId = vchr.Id;
                drvrbalancepending.fk_Voucher = vchr;
                drvrbalancepending.ObjectState = ObjectState.Added;
                advRepo.Insert(drvrbalancepending);

                #endregion

                #region VD1 Fuel dr

                var vchrvd = new VoucherDetail()
                {
                    AccountId = vchr.Account1Id.GetValueOrDefault(),
                    Amount = vchr.Amount1,
                    OfficeId = sat.OfficeId,
                    OrderId = 1,
                    VoucherId = vchr.Id,
                    Voucher = vchr,
                    ObjectState = ObjectState.Added,
                    Narration = drvrbalancepending.Remark
                };
                vchr.VoucherDetails.Add(vchrvd);
                #endregion

                #region VD2 Fuel cr
                var vchrvd2 = new VoucherDetail()
                {
                    VoucherId = vchr.Id,
                    Voucher = vchr,
                    AccountId = vchr.Account2Id.GetValueOrDefault(),
                    Amount = vchr.Amount2,
                    OfficeId = sat.OfficeId,
                    OrderId = 2,
                    ObjectState = ObjectState.Added,
                    Narration = drvrbalancepending.Remark
                };
                vchr.VoucherDetails.Add(vchrvd2);
                #endregion

                #region VDR

                var vchrvd1 = new VoucherDetailReference()
                {
                    Amount = vchrvd2.Amount,
                    ReferenceNo = sat.TripSheetNo,
                    VDRTypeId = 1013, //New Reference
                    VoucherDetailId = vchrvd2.Id,
                    fk_VoucherDetail = vchrvd2,
                    ObjectState = ObjectState.Added
                };
                drvrbalancepending.fk_VDR = vchrvd1;
                drvrbalancepending.VDRId = vchrvd1.Id;
                vchrvd2.VoucherDetailReferences.Add(vchrvd1);
                #endregion
                sat.fk_NetBalVoucher = vchr;
                sat.NetBalVoucherId = vchr.Id;
                sat.ObjectState = ObjectState.Modified;
                vRepo.Insert(vchr);
            }

            #endregion
        }

        /*Multi Currency*/
        public async Task CreateSettlementV4(VehicleTripSettlement sat, IUnitOfWorkAsync uow)
        {
            #region Step0:- Initilizing using variables
            var settlementReceivedAdvTypeIds = new long?[] { 17 };
            bool isNew = sat.Id == 0;
            int ConstCurTypeId = Helper.ConstCurTypeId.GetValueOrDefault();
            sat.NetBalancePending = false;
            var advRepo = _repository.GetRepository<TripAdvanceLog>();
            long DefaultRoundOffAccountId = _repository.GetConfigValue<long>("DefaultRoundOffAccountId");
            long DefaultTruckControlAccountId = _repository.GetConfigValue<long>("DefaultSettledAccountId");
            long SettlementBalanceControlAccountId = _repository.GetConfigValue<long>("SettlementBalanceControlAccountId");

            //var netbalance_1 = sat.FuelAmountDifference/*Value<0 Pay Value>0 Receive*/ + sat.SettledAmount /*Value<0 Pay Value>0 Receive*/- sat.CashDeposited/*Always Value>0*/+ sat.CashPaid;

            if (DefaultRoundOffAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.ROUNDOFF100);
            }
            if (DefaultTruckControlAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.GLB100, "Defualt Truck Control A/c is need to be configured");
            }
            #endregion
            #region Step2:- Initializing used Repos
            var teRepo = _repository.GetRepository<TripExpenseLog>();
            var expTypeRepo = uow.RepositoryAsync<ExpenseMaster>();
            #endregion

            #region Step4:- Deleting Old Voucher
            if (!isNew)
            {
                /*Delete All the VD of Settlement*/
                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVoucherVD] WHERE VoucherId={sat.VoucherId}");
                
                #region Removing Balances if removed from TS
                sat.vwTripAdvances?.RemoveAll(x => x.IsDeleted && (x.TypeId == 17 || x.TypeId == 85 || x.TypeId == 86));
                #endregion

                var balvids = new List<long?>() { sat.SetlBalFuelVoucherId.GetValueOrDefault(), sat.SetlBalVoucherId.GetValueOrDefault(), sat.NetBalVoucherId.GetValueOrDefault() }.Where(x => x > 0).ToList();
                if (balvids.Any())
                {
                    if (await advRepo.Queryable().AnyAsync(x => x.VoucherId > 0 && x.SettledAdvances.Any() && balvids.Contains(x.VoucherId)))
                    {
                        throw new BusinessException(ErrorCode.TADV106, "Balance of this settlement has been settled in any other settlement or has been reversed");
                    }
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripAdvanceLog] WHERE VoucherId IN ({(balvids.JoinStrings(","))})");
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripSettlement] SET SetlBalFuelVoucherId=NULL, SetlBalVoucherId=NULL, NetBalVoucherId=NULL,CashPaidAdvId=NULL WHERE Id={sat.Id}");
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVouchers] WHERE Id IN ({balvids.JoinStrings(",")})");
                }

                if (sat.CashPaidAdvId.GetValueOrDefault() > 0)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripSettlement] SET CashPaidAdvId=NULL WHERE Id={sat.Id}");
                    var voucherid = advRepo.Queryable().Where(x => x.Id == sat.CashPaidAdvId).Select(x => x.VoucherId).FirstOrDefault();
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripAdvanceLog] WHERE Id=@p0", sat.CashPaidAdvId);
                    if (voucherid > 0)
                    {
                        await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVouchers] WHERE Id=@p0", voucherid);
                    }
                }
                sat.SetlBalFuelVoucherId = null;
                sat.SetlBalVoucherId = null;
                sat.NetBalVoucherId = null;
                sat.CashPaidAdvId = null;
            }

            #endregion

            #region Step3:- Deleting Removed Trip Expenses & Fuel Expense from settlement
            /*
            if (!sat.TripLogs.Any() && sat.Id>0)
            {
                sat.vwFuelExpenses.ForEach(x => { x.IsDeleted = true; });
                sat.vwTripExpenses.ForEach(x => { x.IsDeleted = true; });
            }
            */
            var fed = sat.vwFuelExpenses.Where(x => x.IsDeleted).Select(x => x.Id).ToList();
            fed.AddRange(sat.vwTripExpenses.Where(x => x.IsDeleted).Select(x => x.Id));
            

            if (fed.Any())
            {
                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripExpenseLog] WHERE Id in ({fed.JoinStrings(",")})");
                sat.vwTripExpenses?.RemoveAll(x => x.IsDeleted);
                sat.vwFuelExpenses?.RemoveAll(x => x.IsDeleted);
                var invalidfe = sat.vwFuelExpenses.Where(x => x.AdvanceId == 0 || x.TripLogId.GetValueOrDefault(0) == 0).Select(x => $"UsedQty:{x.UsedQty}[{x.Remark}]");
                if (sat.vwFuelExpenses.Any() && sat.vwFuelExpenses.Any(x => x.AdvanceId == 0 || x.TripLogId.GetValueOrDefault(0) == 0)) throw new BusinessException(ErrorCode.TS103, $"Few Fuel Stock Consumption are either not mapped to Trip or are not Mapped to Any Fuel Stock Entry.{Environment.NewLine}{(string.Join(",", invalidfe))}");
            }
            #endregion

            #region Step5:- Trip Expenses
            var existingexpids = sat.vwTripExpenses.Where(x => x.Id > 0 && !x.IsDeleted).Select(y => y.Id).Distinct().ToList();

            var existingexps = existingexpids.Any() ? await teRepo.Queryable().Where(x => existingexpids.Contains(x.Id)).ToListAsync() : null;
            foreach (var item in sat.vwTripExpenses.Where(x => !x.IsDeleted))
            {
                /* Prepare TripExpenseLog for all received Expense*/
                var _ld = uow.RepositoryAsync<ExpenseMaster>().Queryable().Where(x=>x.Id==item.TypeId).FirstOrDefault();
                item.AccountId = _ld.LedgerId;

                var texp = (item.Id > 0 ? existingexps.FirstOrDefault(x => x.Id == item.Id) : null) ?? new TripExpenseLog();
                if (texp.Id <= 0)
                {
                    texp.ObjectState = ObjectState.Added;
                    teRepo.Insert(texp);
                }
                else
                {
                    texp.ObjectState = ObjectState.Modified;
                    teRepo.Update(texp);
                }

                texp.TripLogId = item.TripLogId;
                texp.SettlementId = sat.Id;
                if (!texp.IsBudgeted)
                {
                    texp.ClaimAmount = item.ClaimAmt;
                }
                texp.SettledAmount = item.SettledAmt;
                texp.Remarks = item.Remark;
                texp.ExpenseTypeId = item.TypeId;
                texp.TripAdvanceLogId = item.TripAdvanceLogId;
                texp.FuelRate = item.Rate;
                texp.FuelQty = item.FuelQty;
                texp.ViewId = sat.ViewId;
                texp.ObjectState = texp.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                sat.TripExpenses.Add(texp);
                if (sat.vwTripLogs.All(x => !x.IsDeleted && x.Id != texp.TripLogId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "One of trip expense has triplog which is not getting settled in this settlement");
                }
            }
            sat.SettleDate = sat.SettleDate;
            if (isNew)
            {
                sat.ObjectState = ObjectState.Added;
                _repository.Insert(sat);
                await uow.SaveChangesAsync();
            }
            else
            {
                sat.ObjectState = ObjectState.Modified;
                _repository.Update(sat);
                await uow.SaveChangesAsync();
            }
            #endregion

            #region Step6:- Trip Log Mapping  : 1:VehicleMovementlog
            var tlids = sat.vwTripLogs?.Where(x => !x.IsDeleted).Select(x => x.Id).ToList();
            if (tlids != null && tlids.Any())
            {
                var invalids = sat.TripAdvances.Where(x => x.AdvanceTypeId != 17 && (x.TripLogId == null || !tlids.Contains((long)x.TripLogId))).ToList();
                if (invalids.Any())
                {
                    var invalidrefs = invalids.Select(x => x.ReferenceNo).JoinStrings(",");
                    throw new BusinessException(ErrorCode.GLB106, $"{invalidrefs} Attached Trip Cash Advance is either not mapped to any TripLog or is Mapped to TripLog that is not in this Settlement.");
                }
                if (!isNew)
                {
                    await uow.ExecSqlQueryAsync(
                        $"UPDATE [dbo].[tVehicleMovementLog] SET SettlementId=NULL WHERE SettlementId={sat.Id};");
                }
                foreach (var tl in sat.vwTripLogs?.Where(x => !x.IsDeleted).ToList())
                {
                    await uow.ExecSqlQueryAsync(
                 $"UPDATE [dbo].[tVehicleMovementLog] SET SettlementId={sat.Id},KmRunAdd={tl.AddKM} WHERE Id={tl.Id}");
                }

            }

            #endregion

            #region Step7:- Advance Reveresal
            await uow.ExecSqlQueryAsync($"UPDATE [tTripAdvanceLog] SET SettlementId=NULL WHERE SettlementId=@id AND AdvanceTypeId=94", new SqlParameter("id", sat.Id));
            var reverse = sat.vwTripAdvances?.Where(x => !x.IsDeleted && x.TypeId == 94).ToList();
            if (reverse.Any())
            {
                foreach (var r in reverse)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [tTripAdvanceLog] SET SettlementId={sat.Id},TripLogId={r.TripLogId} WHERE Id={r.Id} AND AdvanceTypeId=94");
                }

            }
            #endregion

            #region Step8:- Driver Advance
            var cashadvids = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 1 || x.TypeId == 16 || x.TypeId == 91)).Select(x => x.Id).ToList() ?? new List<long>();
            if (!isNew)
            {
                await uow.ExecSqlQueryAsync(
                $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId=NULL WHERE SettlementId={sat.Id} {(cashadvids.Any() ? $" AND Id NOT IN({cashadvids.JoinStrings(",")})" : "")}");
            }
            if (cashadvids.Any())
            {
                await uow.ExecSqlQueryAsync(
                $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId={sat.Id} WHERE Id IN({cashadvids.JoinStrings(",")})");
            }
            #endregion

            #region Step:- Driver Fuel Advances
            var fueladv = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 2 || x.TypeId == 85)).Select(x => x.Id).ToList() ?? new List<long>();
            if (!isNew)
            {
                await uow.ExecSqlQueryAsync(
                $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId=NULL  WHERE SettlementId={sat.Id} AND AdvanceTypeId in(2,85) {(fueladv.Any() ? $" AND Id NOT IN({fueladv.JoinStrings(",")})" : "")}");
            }
            if (fueladv.Any())
            {
                await uow.ExecSqlQueryAsync(
                $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId={sat.Id} WHERE Id IN({fueladv.JoinStrings(",")})");
            }
            #endregion

            #region 88:e-Toll Payment
            var etollexpmaster = await uow.RepositoryAsync<ExpenseMaster>().Queryable().FirstOrDefaultAsync(x => x.NatureId == 1617);

            if (etollexpmaster == null)
            {
                throw new BusinessException(ErrorCode.TS103, $"eToll Expense Master is not defined.");
            }

            if (etollexpmaster.LedgerId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.TS103, $"eToll Expense Master is not mapped with Expense Ledger Hint.Expense Name:{etollexpmaster.Name}");
            }

            var eTotaladvids = sat.vwTripAdvances?.Where(x => !x.IsDeleted && (x.TypeId == 88)).ToList() ?? new List<Advance>();
            if (eTotaladvids.Any())
            {
                if (sat.vwTripAdvances.Any(x => !x.IsDeleted && x.TypeId == 88 && x.TripLogId.GetValueOrDefault() == 0))
                {
                    throw new BusinessException(ErrorCode.GLB106, "One of eToll is not mapped to any of TripLog included this settlement.");
                }
                var etollids = eTotaladvids.Select(y => y.Id).ToList();

                var etollidscomma = etollids.JoinStrings(",");
                await _repository.UOW.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET TripLogId=NULL,SettlementId=NULL WHERE SettlementId=@arg1 and AdvanceTypeId=88 and Id not in({etollidscomma})", new SqlParameter("arg1", sat.Id));

                foreach (var l in eTotaladvids.GroupBy(x => x.TripLogId))
                {
                    var ids = l.Select(x => x.Id).ToArray().JoinStrings(",");
                    await _repository.UOW.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET TripLogId=@arg1,SettlementId=@arg2 WHERE AdvanceTypeId=88 AND Id in({ids})", new SqlParameter("arg1", ((object)l.Key ?? DBNull.Value)), new SqlParameter("arg2", sat.Id));
                }
            }
            #endregion

            #region Settlement voucher
            var vRepo = _repository.GetRepository<Voucher>();
            var v = sat.fk_Voucher ?? (sat.VoucherId > 0 ?
            await vRepo.Queryable().FirstOrDefaultAsync(x => x.Id == sat.VoucherId) ?? new Voucher() : new Voucher());

            sat.ConstCurTypeId = ConstCurTypeId;
            sat.CurRate = sat.CurTypeId == ConstCurTypeId ? 1 : sat.CurRate;

            v.ViewId = sat.ViewId;
            v.OfficeId = sat.OfficeId;
            v.VoucherNo = sat.TripSheetNo;
            v.VoucherDate = sat.SettleDate.GetValueOrDefault(DateTime.Now);
            v.VoucherDateTime = sat.SettleDate.Value;
            v.VoucherAmount = v.VoucherDetails.Where(x => x.Amount > 0).Sum(x => x.Amount);//s.fk_Voucher.VoucherDetails.Sum(x => x.Amount);
            v.VoucherTypeId = 18;
            v.IsCCRequired = true;
            v.CurTypeId = sat.ConstCurTypeId;
            v.ConstCurTypeId = v.CurTypeId;
            v.CurRate = 1;

            sat.ObjectState = ObjectState.Modified;
            sat.fk_Voucher = v;
            sat.VoucherId = v.Id;
            vRepo.Attach(v);
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;

            //if (sat.Id <= 0)
            //{
            //    sat.ObjectState = ObjectState.Added;
            //    _repository.Insert(sat);
            //    await uow.SaveChangesAsync();
            //}
            //else
            //{
            //    sat.ObjectState = ObjectState.Modified;
            //    _repository.Update(sat);
            //    await uow.SaveChangesAsync();
            //}
            #region VD When Driver Deposit Cash
            if (sat.CashDeposited > 0)
            {
                var vd = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = sat.CashDepositAccId > 0 ? sat.CashDepositAccId.GetValueOrDefault() : DefaultTruckControlAccountId,
                    Amount = Math.Round(sat.CashDeposited,2),
                    Amount_MNC = Math.Round(sat.CashDeposited,2),
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 4,
                    VoucherId = v.Id,
                    CurTypeId = v.CurTypeId,
                    CurRate = v.CurRate,
                    ConstCurTypeId = v.ConstCurTypeId,
                    IsCCRequired = false,
                };
                v.VoucherDetails.Add(vd);
            }
            if (sat.CashPaid > 0)
            {
                var generateCashPaidAdvance = _repository.GetConfigValue<long>("GenerateCashPaidAdvance");
                if (sat.CashPaidAccId.GetValueOrDefault() <= 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Cash Paid Account is Required when Cash Paid Amount is greater than Zero.");
                }
                var vd = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = sat.CashPaidAccId > 0 ? sat.CashPaidAccId.GetValueOrDefault() : DefaultTruckControlAccountId,
                    Amount = -sat.CashPaid,
                    Amount_MNC = -sat.CashPaid,
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 4,
                    VoucherId = v.Id,
                    CurTypeId = v.CurTypeId,
                    CurRate = v.CurRate,
                    ConstCurTypeId = v.ConstCurTypeId,
                    IsCCRequired = false,
                };
                if (vd.AccountId == 0)
                {
                    vd.AccountId = _repository.GetConfigValue<long>("DefaultSettledAccountId");
                }
                v.VoucherDetails.Add(vd);
                if (vd.AccountId <= 0)
                {
                    throw new BusinessException("Cash Paid Account Not Configured", sat.TripSheetNo);
                }
                PrepareCashPaidAdvance(ref sat, sat.CashPaidAccId.GetValueOrDefault(), sat.Driver1Id.GetValueOrDefault(), vRepo, generateCashPaidAdvance);
            }
            #endregion

            #region Credit VDs
            foreach (var tal in sat.vwTripAdvances.Where(x=>!x.IsDeleted).GroupBy(x => new { x.DebitAcId, x.TypeId }))
            {
                var vd = new VoucherDetail
                {
                    OfficeId = sat.OfficeId,
                    AccountId = tal.Key.DebitAcId.GetValueOrDefault(),
                    OrderId = 1,
                    Amount = -tal.Sum(y => y.SettAdvAmt),
                    Amount_MNC = -tal.Sum(y => y.SettAdvAmt),
                    ObjectState = ObjectState.Added,
                    VoucherId = v.Id,
                    Voucher = v,
                    CurTypeId = v.CurTypeId,
                    CurRate = v.CurRate,
                    ConstCurTypeId = v.ConstCurTypeId,
                    IsCCRequired = false,
                };
                v.VoucherDetails.Add(vd);

                foreach (var log in tal.Where(x => x.VDRId.GetValueOrDefault() > 0))
                {
                    var vdr = new VoucherDetailReference()
                    {
                        ObjectState = ObjectState.Added,
                        Amount = -log.DocSettAdvAmt,
                        IsCCRequired = false,
                        RefId = log.VDRId,
                        VDRTypeId = 1014,
                        VoucherDetailId = vd.Id,
                        fk_VoucherDetail = vd,
                        ReferenceNo = log.RefNo,
                        AccountId = vd.AccountId,
                        Amount_MNC = -log.SettAdvAmt,
                        DueDate = sat.SettleDate.Value,
                        CurTypeId = ConstCurTypeId,
                        CurRate = v.CurRate
                    };
                    vd.VoucherDetailReferences.Add(vdr);
                }
                /*if any advance come without VDR so total of such amount will be go in onaccount otherwise VDAmount will not match with VDR Total*/
                decimal onaccountamount = tal.Where(x => x.VDRId.GetValueOrDefault(0) <= 0).Sum(y => y.SettAdvAmt);
                if (onaccountamount > 0)
                {
                    var vdr = new VoucherDetailReference()
                    {
                        IsCCRequired = false,
                        ObjectState = ObjectState.Added,
                        Amount = -onaccountamount,
                        Amount_MNC = -onaccountamount,
                        VDRTypeId = 1448,
                        VoucherDetailId = vd.Id,
                        fk_VoucherDetail = vd,
                        ReferenceNo = sat.TripSheetNo,
                        AccountId = vd.AccountId,
                        DueDate = sat.SettleDate.Value,
                        CurTypeId = v.CurTypeId,
                        CurRate = v.CurRate,
                        ConstCurTypeId = v.ConstCurTypeId
                    };
                    vd.VoucherDetailReferences.Add(vdr);
                }
            }
            #endregion
            #region Debit VDs
            foreach (var tal in sat.vwTripExpenses.Where(x=>!x.IsDeleted).GroupBy(k => k.AccountId))
            {
                var vd = new VoucherDetail
                {
                    OfficeId = sat.OfficeId,
                    AccountId = tal.Key.GetValueOrDefault(),
                    OrderId = 2,
                    Amount = Math.Round(tal.Sum(y => y.SettledAmt),2),
                    Amount_MNC = Math.Round(tal.Sum(y => y.SettledAmt), 2),
                    ObjectState = ObjectState.Added,
                    VoucherId = v.Id,
                    Voucher = v,
                    CurTypeId = v.CurTypeId,
                    CurRate = v.CurRate,
                    ConstCurTypeId = v.ConstCurTypeId,
                    IsCCRequired = false,
                };
                v.VoucherDetails.Add(vd);
            }
            #endregion

            #region Net VDs & Trip Amount carry forward
            var netbalance = sat.FuelAmountDifference/*Value<0 Pay Value>0 Receive*/ + sat.SettledAmount /*Value<0 Pay Value>0 Receive*/- sat.CashDeposited/*Always Value>0*/+ sat.CashPaid;
            if (netbalance != 0)
            {
                var vd = new VoucherDetail
                {
                    OfficeId = sat.OfficeId,
                    AccountId = SettlementBalanceControlAccountId,
                    OrderId = 10,
                    Amount = netbalance,
                    Amount_MNC = netbalance,
                    ObjectState = ObjectState.Added,
                    VoucherId = v.Id,
                    Voucher = v,
                    CurTypeId = v.CurTypeId,
                    CurRate = v.CurRate,
                    ConstCurTypeId = v.ConstCurTypeId,
                    IsCCRequired = false
                };
                v.VoucherDetails.Add(vd);               

                var baladv = new TripAdvanceLog
                {
                    AdvanceDate = sat.SettleDate.Value,
                    FuelAmount = 0,
                    FuelQty = 0,
                    CashAmount = 0,
                    OfficeId = sat.OfficeId,
                    FuelRate = 0,

                    DriverId = sat.Driver1Id,
                    FuelId = null,
                    TripLogId = null,
                    VehicleId = null,
                    IsBulkEntry = false,
                    CurTypeId = sat.CurTypeId,
                    CurRate = sat.CurTypeId == sat.ConstCurTypeId ? 1 : sat.CurRate,
                    ConstCurTypeId = sat.ConstCurTypeId,
                    CreditAccountId = SettlementBalanceControlAccountId,
                    DebitAccountId = SettlementBalanceControlAccountId
                };
                sat.AdjustmentTypeId = sat.AdjustmentTypeId <= 0 ? 1590 : sat.AdjustmentTypeId;

                #region Roundoffvalue

                #endregion

                /*Payable / Receivable Entries */
                switch (sat.AdjustmentTypeId)
                {
                    case 1592: //both separetely
                        #region Fuel Receive
                        if (sat.FuelAmountDifference > 0)/*[Receive]*/
                        {
                            #region TripAdvance Fuel
                            //var frate = sat.FuelQtyDifference < 0 ? 0 : sat.FuelAmountDifference / sat.FuelQtyDifference;

                            var adv1592 = baladv.Clone();
                            adv1592.Remark = "Trip Settlement fuel Balance Carry Forwarded as driver was unable to pay cash back to company.";
                            adv1592.ReferenceNo = "TSFBL-" + sat.TripSheetNo;
                            adv1592.VoucherNo = "TSFBL-" + sat.TripSheetNo;
                            adv1592.CashAmount = 0;
                            adv1592.FuelQty = sat.FuelQtyDifference;
                            adv1592.FuelAmount = sat.CurTypeId == sat.ConstCurTypeId ? sat.FuelAmountDifference : Math.Round((sat.FuelAmountDifference / sat.CurRate),2);
                            adv1592.AdvanceTypeId = 85;
                            adv1592.FuelRate = Math.Round(adv1592.FuelQty > 0 ? adv1592.FuelAmount / adv1592.FuelQty : 0, 4);
                            
                            adv1592.VehicleId = sat.VehicleId;
                            #endregion End TripAdvance Fuel

                            #region Voucher Fuel
                            var v1592Fuel = new Voucher()
                            {
                                Id = sat.SetlBalFuelVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 85,
                                OfficeId = sat.OfficeId,
                                VoucherAmount = Math.Abs(adv1592.FuelAmount),
                                VoucherNo = adv1592.VoucherNo,
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                Account1Id = adv1592.DebitAccountId,
                                Amount1 = Math.Abs(adv1592.FuelAmount),
                                Account2Id = adv1592.CreditAccountId,
                                Amount2 = -Math.Abs(adv1592.FuelAmount),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added,
                                CurTypeId = adv1592.CurTypeId,
                                CurRate = adv1592.CurRate,
                                ConstCurTypeId = adv1592.ConstCurTypeId
                            };
                            adv1592.VoucherId = v1592Fuel.Id;
                            adv1592.fk_Voucher = v1592Fuel;
                            #endregion

                            #region VD1 Fuel
                            var v1592Fuel1 = new VoucherDetail()
                            {
                                AccountId = v1592Fuel.Account1Id.GetValueOrDefault(),
                                Amount = v1592Fuel.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                VoucherId = v1592Fuel.Id,
                                Voucher = v1592Fuel,
                                ObjectState = ObjectState.Added,
                                CurTypeId = v1592Fuel.CurTypeId,
                                CurRate = v1592Fuel.CurRate,
                                ConstCurTypeId = v1592Fuel.ConstCurTypeId
                            };
                            v1592Fuel.VoucherDetails.Add(v1592Fuel1);
                            #endregion

                            #region VDR
                            var v1592fvdr = new VoucherDetailReference()
                            {
                                Amount = v1592Fuel1.Amount,
                                ReferenceNo = v1592Fuel.VoucherNo,
                                VDRTypeId = 1013, //New Reference
                                VoucherDetailId = v1592Fuel1.Id,
                                fk_VoucherDetail = v1592Fuel1,
                                ObjectState = ObjectState.Added,
                                CurTypeId = v1592Fuel.CurTypeId,
                                CurRate = v1592Fuel.CurRate,
                                ConstCurTypeId = v1592Fuel.ConstCurTypeId
                            };
                            v1592Fuel1.VoucherDetailReferences.Add(v1592fvdr);

                            adv1592.VDRId = v1592fvdr.Id;
                            adv1592.fk_VDR = v1592fvdr;
                            adv1592.ObjectState = ObjectState.Added;
                            advRepo.Insert(adv1592);
                            #endregion

                            #region VD2 Fuel
                            var v1592Fuel2 = new VoucherDetail()
                            {
                                VoucherId = v1592Fuel.Id,
                                Voucher = v1592Fuel,
                                AccountId = v1592Fuel.Account2Id.GetValueOrDefault(),
                                Amount = v1592Fuel.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                                CurTypeId = v1592Fuel.CurTypeId,
                                CurRate = v1592Fuel.CurRate,
                                ConstCurTypeId = v1592Fuel.ConstCurTypeId
                            };
                            v1592Fuel.VoucherDetails.Add(v1592Fuel2);
                            #endregion
                            /*Round Off*/
                            var fgn = v1592Fuel.VoucherDetails.Sum(x => x.Amount_MNC);
                            if (fgn != 0)
                            {
                                #region VD2 Fuel
                                var v1592Fuel3 = new VoucherDetail()
                                {
                                    VoucherId = v1592Fuel.Id,
                                    Voucher = v1592Fuel,
                                    AccountId = DefaultRoundOffAccountId,
                                    Amount = -fgn,
                                    OfficeId = sat.OfficeId,
                                    OrderId = 12,
                                    ObjectState = ObjectState.Added,
                                    CurTypeId = v.ConstCurTypeId,
                                    CurRate = 1,
                                    ConstCurTypeId = v.ConstCurTypeId
                                };
                                v1592Fuel.VoucherDetails.Add(v1592Fuel3);

                                #endregion
                            }
                            sat.fk_SetlBalFuelVoucher = v1592Fuel;
                            sat.SetlBalFuelVoucherId = v1592Fuel.Id;
                            sat.ObjectState = ObjectState.Modified;
                            vRepo.Insert(v1592Fuel);
                        }

                        #endregion

                        #region Cash Receive
                        if (sat.SettledAmount > 0)/*[Receive]*/
                        {
                            #region TripAdvance Cash
                            baladv.Remark = "Trip Settlement Cash Balance Carry Forwarded as driver was unable to pay back to company.";
                            baladv.ReferenceNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.AdvanceTypeId = 17;

                            baladv.CashAmount = sat.CurTypeId == sat.ConstCurTypeId ? sat.SettledAmount : Math.Round(sat.SettledAmount / sat.CurRate,2);

                            baladv.CreditAccountId = sat.SettledAmount > 0 ? baladv.CreditAccountId : sat.Driver1Id;
                            baladv.DebitAccountId = sat.SettledAmount > 0 ? sat.Driver1Id : baladv.DebitAccountId;
                            #endregion

                            #region Voucher Cash
                            var v1592Cash = new Voucher()
                            {
                                Id = sat.SetlBalVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 17,
                                OfficeId = sat.OfficeId,
                                VoucherNo = baladv.VoucherNo,
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                VoucherAmount = Math.Abs(baladv.CashAmount),
                                Account1Id = baladv.DebitAccountId,
                                Amount1 = Math.Abs(baladv.CashAmount),
                                Account2Id = baladv.CreditAccountId,
                                Amount2 = -Math.Abs(baladv.CashAmount),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added,
                                AccountingRemark = baladv.Remark,
                                CurTypeId = baladv.CurTypeId,
                                CurRate = baladv.CurRate,
                                ConstCurTypeId = baladv.ConstCurTypeId
                            };
                            baladv.VoucherId = v1592Cash.Id;
                            baladv.fk_Voucher = v1592Cash;
                            #endregion

                            #region VD1 Cash
                            var v1592Cash1 = new VoucherDetail()
                            {
                                VoucherId = v1592Cash.Id,
                                Voucher = v1592Cash,
                                AccountId = v1592Cash.Account1Id.GetValueOrDefault(),
                                Amount = v1592Cash.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark,
                                CurTypeId = v1592Cash.CurTypeId,
                                CurRate = v1592Cash.CurRate,
                                ConstCurTypeId = v1592Cash.ConstCurTypeId
                            };
                            v1592Cash1.ObjectState = ObjectState.Added;
                            v1592Cash.VoucherDetails.Add(v1592Cash1);
                            #endregion

                            #region VDR
                            var v1592cash = new VoucherDetailReference()
                            {
                                VoucherDetailId = v1592Cash1.Id,
                                fk_VoucherDetail = v1592Cash1,
                                Amount = Math.Abs(v1592Cash1.Amount),
                                ReferenceNo = v1592Cash.VoucherNo,
                                VDRTypeId = 1013, //New Reference
                                ObjectState = ObjectState.Added,
                                CurTypeId = v1592Cash.CurTypeId,
                                CurRate = v1592Cash.CurRate,
                                ConstCurTypeId = v1592Cash.ConstCurTypeId
                            };
                            v1592Cash1.VoucherDetailReferences.Add(v1592cash);

                            baladv.VDRId = v1592cash.Id;
                            baladv.fk_VDR = v1592cash;
                            baladv.ObjectState = ObjectState.Added;
                            advRepo.Insert(baladv);
                            #endregion

                            #region VD2 Cash
                            var v1592Cash2 = new VoucherDetail()
                            {
                                VoucherId = v1592Cash.Id,
                                Voucher = v1592Cash,
                                AccountId = v1592Cash.Account2Id.GetValueOrDefault(),
                                Amount = v1592Cash.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark,
                                CurTypeId = v1592Cash.CurTypeId,
                                CurRate = v1592Cash.CurRate,
                                ConstCurTypeId = v1592Cash.ConstCurTypeId
                            };
                            v1592Cash.VoucherDetails.Add(v1592Cash2);
                            #endregion

                            /*RoundOff vd*/
                            var fgn = v1592Cash.VoucherDetails.Sum(x => x.Amount_MNC);
                            if (fgn != 0)
                            {
                                #region VD2 Fuel
                                var v1592Fuel3 = new VoucherDetail()
                                {
                                    VoucherId = v1592Cash.Id,
                                    Voucher = v1592Cash,
                                    AccountId = DefaultRoundOffAccountId,
                                    Amount = -fgn,
                                    OfficeId = sat.OfficeId,
                                    OrderId = 12,
                                    ObjectState = ObjectState.Added,
                                    CurTypeId = v.ConstCurTypeId,
                                    CurRate = 1,
                                    ConstCurTypeId = v.ConstCurTypeId
                                };
                                v1592Cash.VoucherDetails.Add(v1592Fuel3);

                                #endregion
                            }
                            sat.fk_SetlBalVoucher = v1592Cash;
                            sat.SetlBalVoucherId = v1592Cash.Id;
                            vRepo.Insert(v1592Cash);
                        }

                        if (sat.SettledAmount < 0)/*[Pay]*/
                        {
                            #region TripAdvance Cash

                            baladv.AdvanceTypeId = 86;
                            baladv.Remark = "Pending Balance Carry Forwarded as company was unable to pay back to Driver.";
                            baladv.ReferenceNo = "TBPND-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TBPND-" + sat.TripSheetNo;

                            baladv.CashAmount = sat.CurTypeId == sat.ConstCurTypeId ? sat.SettledAmount : Math.Round(sat.SettledAmount / sat.CurRate, 2);

                            baladv.CreditAccountId = sat.Driver1Id;
                            baladv.DebitAccountId = baladv.DebitAccountId;
                            #endregion

                            #region Voucher Cash
                            var v1592Cash = new Voucher()
                            {
                                Id = sat.SetlBalVoucherId.GetValueOrDefault(),
                                VoucherTypeId = 86,
                                OfficeId = sat.OfficeId,
                                VoucherNo = baladv.VoucherNo,
                                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                                VoucherDateTime = sat.SettleDate.Value,
                                VoucherAmount = Math.Abs(baladv.CashAmount),
                                Account1Id = baladv.DebitAccountId,
                                Amount1 = Math.Abs(baladv.CashAmount),
                                Account2Id = baladv.CreditAccountId,
                                Amount2 = -Math.Abs(baladv.CashAmount),
                                IsAudited = false,
                                IsAccepted = false,
                                IsAccountsVisiblity = false,
                                PageId = null,
                                ViewId = sat.ViewId,
                                ObjectState = ObjectState.Added,
                                AccountingRemark = baladv.Remark,
                                CurTypeId = baladv.CurTypeId,
                                CurRate = baladv.CurRate,
                                ConstCurTypeId = baladv.ConstCurTypeId
                            };
                            baladv.VoucherId = v1592Cash.Id;
                            baladv.fk_Voucher = v1592Cash;
                            #endregion

                            #region VD1 Cash
                            var v1592Cash1 = new VoucherDetail()
                            {
                                VoucherId = v1592Cash.Id,
                                Voucher = v1592Cash,
                                AccountId = v1592Cash.Account1Id.GetValueOrDefault(),
                                Amount = v1592Cash.Amount1,
                                OfficeId = sat.OfficeId,
                                OrderId = 1,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark,
                                CurTypeId = v1592Cash.CurTypeId,
                                CurRate = v1592Cash.CurRate,
                                ConstCurTypeId = v1592Cash.ConstCurTypeId
                            };
                            v1592Cash1.ObjectState = ObjectState.Added;
                            v1592Cash.VoucherDetails.Add(v1592Cash1);
                            #endregion
                            baladv.ObjectState = ObjectState.Added;
                         
                            #region VD2 Cash
                            var v1592Cash2 = new VoucherDetail()
                            {
                                VoucherId = v1592Cash.Id,
                                Voucher = v1592Cash,
                                AccountId = v1592Cash.Account2Id.GetValueOrDefault(),
                                Amount = v1592Cash.Amount2,
                                OfficeId = sat.OfficeId,
                                OrderId = 2,
                                ObjectState = ObjectState.Added,
                                Narration = baladv.Remark,
                                CurTypeId = v1592Cash.CurTypeId,
                                CurRate = v1592Cash.CurRate,
                                ConstCurTypeId = v1592Cash.ConstCurTypeId
                            };
                            v1592Cash2.ObjectState = ObjectState.Added;
                            v1592Cash.VoucherDetails.Add(v1592Cash2);
                            #endregion
                            #region VDR
                            var v1592Cash2vdr = new VoucherDetailReference()
                            {
                                VoucherDetailId = v1592Cash2.Id,
                                fk_VoucherDetail = v1592Cash2,
                                Amount = Math.Abs(v1592Cash2.Amount),
                                ReferenceNo = v1592Cash.VoucherNo,
                                VDRTypeId = 1013, //New Reference
                                ObjectState = ObjectState.Added,
                                CurTypeId = v1592Cash.CurTypeId,
                                CurRate = v1592Cash.CurRate,
                                ConstCurTypeId = v1592Cash.ConstCurTypeId
                            };
                            v1592Cash2.VoucherDetailReferences.Add(v1592Cash2vdr);

                            baladv.VDRId = v1592Cash2vdr.Id;
                            baladv.fk_VDR = v1592Cash2vdr;
                            baladv.ObjectState = ObjectState.Added;

                            advRepo.Insert(baladv);
                            #endregion

                            /*RoundOff vd*/
                            var fgn = v1592Cash.VoucherDetails.Sum(x => x.Amount_MNC);
                            if (fgn != 0)
                            {
                                #region VD2 Fuel
                                var v1592Fuel3 = new VoucherDetail()
                                {
                                    VoucherId = v1592Cash.Id,
                                    Voucher = v1592Cash,
                                    AccountId = DefaultRoundOffAccountId,
                                    Amount = -fgn,
                                    OfficeId = sat.OfficeId,
                                    OrderId = 12,
                                    ObjectState = ObjectState.Added,
                                    CurTypeId = v.ConstCurTypeId,
                                    CurRate = 1,
                                    ConstCurTypeId = v.ConstCurTypeId
                                };
                                v1592Cash.VoucherDetails.Add(v1592Fuel3);

                                #endregion
                            }
                            sat.fk_SetlBalVoucher = v1592Cash;
                            sat.SetlBalVoucherId = v1592Cash.Id;
                            vRepo.Insert(v1592Cash);
                        }
                        #endregion
                        break;

                    case 1590:// Net As Cash Adv
                        if (netbalance > 0)
                        {
                            #region TripAdvance
                            baladv.Remark = "Trip Settlement Cash Balance Carry Forwarded as driver was unable to pay cash back to company.";
                            baladv.ReferenceNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TSCBL-" + sat.TripSheetNo;
                            baladv.AdvanceTypeId = 17;
                        }
                        else
                        {
                            baladv.AdvanceTypeId = 86;
                            baladv.Remark = "Pending Balance Carry Forwarded as company was unable to pay back to Driver.";
                            baladv.ReferenceNo = "TBPND-" + sat.TripSheetNo;
                            baladv.VoucherNo = "TBPND-" + sat.TripSheetNo;
                        }
                        baladv.CashAmount = sat.CurTypeId == sat.ConstCurTypeId ? netbalance : Math.Round(netbalance / sat.CurRate,2);
                        baladv.CreditAccountId = netbalance > 0 ? baladv.CreditAccountId : sat.Driver1Id;
                        baladv.DebitAccountId = netbalance > 0 ? sat.Driver1Id : baladv.DebitAccountId;
                        #endregion

                        #region Voucher
                        var v1590 = new Voucher()
                        {
                            Id = sat.SetlBalVoucherId.GetValueOrDefault(),
                            VoucherTypeId = baladv.AdvanceTypeId.GetValueOrDefault(),
                            OfficeId = sat.OfficeId,
                            VoucherNo = baladv.VoucherNo,
                            VoucherAmount = Math.Abs(baladv.CashAmount),
                            VoucherDate = sat.SettleDate.GetValueOrDefault(),
                            VoucherDateTime = sat.SettleDate.Value,
                            Account1Id = baladv.DebitAccountId,
                            Amount1 = Math.Abs(baladv.CashAmount),
                            Account2Id = baladv.CreditAccountId,
                            Amount2 = -Math.Abs(baladv.CashAmount),
                            IsAudited = false,
                            IsAccepted = false,
                            IsAccountsVisiblity = false,
                            PageId = null,
                            ViewId = sat.ViewId,
                            ObjectState = ObjectState.Added,
                            AccountingRemark = baladv.Remark,
                            CurTypeId = sat.CurTypeId,
                            CurRate = sat.CurRate,
                            ConstCurTypeId = sat.ConstCurTypeId
                        };
                        baladv.VoucherId = v1590.Id;
                        baladv.fk_Voucher = v1590;

                        #endregion

                        #region VD1 Cash
                        var v15901 = new VoucherDetail()
                        {
                            VoucherId = v1590.Id,
                            Voucher = v1590,
                            AccountId = v1590.Account1Id.GetValueOrDefault(),
                            Amount = v1590.Amount1,
                            OfficeId = sat.OfficeId,
                            OrderId = 1,
                            ObjectState = ObjectState.Added,
                            Narration = baladv.Remark,
                            CurTypeId = v1590.CurTypeId,
                            CurRate = v1590.CurRate,
                            ConstCurTypeId = v1590.ConstCurTypeId
                        };
                        v15901.ObjectState = ObjectState.Added;
                        v1590.VoucherDetails.Add(v15901);
                        #endregion


                        #region VDR
                        if (baladv.AdvanceTypeId != 86)
                        {
                            var v1591vdr = new VoucherDetailReference()
                            {
                                VoucherDetailId = v15901.Id,
                                fk_VoucherDetail = v15901,
                                Amount = Math.Abs(v15901.Amount),
                                ReferenceNo = v1590.VoucherNo,
                                VDRTypeId = 1013, //New Reference
                                ObjectState = ObjectState.Added,
                                CurTypeId = v1590.CurTypeId,
                                CurRate = v1590.CurRate,
                                ConstCurTypeId = v1590.ConstCurTypeId
                            };
                            v15901.VoucherDetailReferences.Add(v1591vdr);

                            baladv.VDRId = v1591vdr.Id;
                            baladv.fk_VDR = v1591vdr;
                            baladv.ObjectState = ObjectState.Added;
                            advRepo.Insert(baladv);
                        }
                        #endregion

                        #region VD2 Cash

                        var v15902 = new VoucherDetail()
                        {
                            VoucherId = v1590.Id,
                            Voucher = v1590,
                            AccountId = v1590.Account2Id.GetValueOrDefault(),
                            Amount = v1590.Amount2,
                            OfficeId = sat.OfficeId,
                            OrderId = 2,
                            ObjectState = ObjectState.Added,
                            Narration = baladv.Remark,
                            CurTypeId = v1590.CurTypeId,
                            CurRate = v1590.CurRate,
                            ConstCurTypeId = v1590.ConstCurTypeId
                        };
                        v1590.VoucherDetails.Add(v15902);
                        #region VDR
                        if (baladv.AdvanceTypeId == 86)
                        {
                            var v1592vdr = new VoucherDetailReference()
                            {
                                VoucherDetailId = v15902.Id,
                                fk_VoucherDetail = v15902,
                                Amount = Math.Abs(v15902.Amount),
                                ReferenceNo = v1590.VoucherNo,
                                VDRTypeId = 1013, //New Reference
                                ObjectState = ObjectState.Added,
                                CurTypeId = v1590.CurTypeId,
                                CurRate = v1590.CurRate,
                                ConstCurTypeId = v1590.ConstCurTypeId
                            };
                            v15901.VoucherDetailReferences.Add(v1592vdr);

                            baladv.VDRId = v1592vdr.Id;
                            baladv.fk_VDR = v1592vdr;
                            baladv.ObjectState = ObjectState.Added;
                            advRepo.Insert(baladv);
                        }
                        #endregion
                        /*Round off vd*/
                        var fgn1 = v1590.VoucherDetails.Sum(x=>x.Amount_MNC);
                        if (fgn1 != 0)
                        {
                            #region VD3 
                            var v1590vd3 = new VoucherDetail()
                            {
                                VoucherId = v1590.Id,
                                Voucher = v1590,
                                AccountId = DefaultRoundOffAccountId,
                                Amount = -fgn1,
                                OfficeId = sat.OfficeId,
                                OrderId = 12,
                                ObjectState = ObjectState.Added,
                                CurTypeId = v.ConstCurTypeId,
                                CurRate = 1,
                                ConstCurTypeId = v.ConstCurTypeId
                            };
                            v1590.VoucherDetails.Add(v1590vd3);
                            #endregion
                        }
                        #endregion
                        sat.fk_SetlBalVoucher = v1590;
                        sat.SetlBalVoucherId = v1590.Id;
                        vRepo.Insert(v1590);

                        break;
                    case 1591:// Forfeit
                        vd.AccountId = _repository.GetConfigValue<long>("DefaultForfeitSettledAccountId");
                        break;
                }
            }

            /*Round Off VD*/
            var rvdamt = v.VoucherDetails.Sum(x => x.Amount_MNC);
            if (rvdamt != 0)
            {
                #region VD2 Fuel
                var rdvd = new VoucherDetail()
                {
                    VoucherId = v.Id,
                    Voucher = v,
                    AccountId = DefaultRoundOffAccountId,
                    Amount = -rvdamt,
                    OfficeId = sat.OfficeId,
                    OrderId = 12,
                    ObjectState = ObjectState.Added,
                    CurTypeId = v.ConstCurTypeId,
                    CurRate = 1,
                    ConstCurTypeId = v.ConstCurTypeId
                };
                v.VoucherDetails.Add(rdvd);

                #endregion
            }
            #endregion
            #endregion
        }

        private void PrepareCashPaidAdvance(ref VehicleTripSettlement sat,long creditaccountId,long debitAccountId,IRepository<Voucher> repo,long advanceStatus)
        {

            #region Driver Pending Advance
            if (sat.CashPaid == 0)
            {
                return;
            }
            var adv = new TripAdvanceLog
            {
                AdvanceDate = sat.SettleDate.Value,
                FuelAmount = 0,
                FuelQty = 0,
                AdvanceTypeId = 101/*Settlement Balance[CashPaid]*/,
                Remark = "Cash Paid to paid to Driver as a Settlement Balance.",
                ReferenceNo = "CPSBL-" + sat.TripSheetNo,
                VoucherNo = "CPSBL-" + sat.TripSheetNo,
                CashAmount = Math.Abs(sat.CashPaid),
                OfficeId = sat.OfficeId,
                CreditAccountId = creditaccountId,
                FuelRate = 0,
                DebitAccountId = debitAccountId,
                DriverId = sat.Driver1Id,
                FuelId = null,
                TripLogId = null,
                VehicleId = sat.VehicleId,
                IsBulkEntry = false,
                RequestStatusId = advanceStatus/*1597:DiractPay,1596:AsRequest*/,
                CurTypeId = sat.CurTypeId,
                CurRate = 1,
                ConstCurTypeId = sat.ConstCurTypeId
            };
            if (adv.RequestStatusId == 1596)
            {
                adv.RequestAmount = adv.CashAmount;
                adv.RequestDate = adv.AdvanceDate;
                adv.CashAmount = 0;
            }
            //Settlement Balance Deposit
            if (adv.RequestStatusId.GetValueOrDefault(1597) == 1597)
            {
                adv.RequestStatusId = null;
                #endregion Driver Pending Advance

            #region Voucher PayBalance

            var vchr = new Voucher()
            {
                Id = adv.VoucherId.GetValueOrDefault(),
                VoucherTypeId = adv.AdvanceTypeId.GetValueOrDefault(),
                OfficeId = sat.OfficeId,
                VoucherNo = adv.VoucherNo,
                VoucherDate = sat.SettleDate.GetValueOrDefault(),
                VoucherDateTime = sat.SettleDate.Value,
                Account1Id = adv.DebitAccountId,
                Amount1 = Math.Abs(adv.CashAmount),
                Account2Id = adv.CreditAccountId,
                Amount2 = -Math.Abs(adv.CashAmount),
                IsAudited = false,
                IsAccepted = false,
                IsAccountsVisiblity = false,
                PageId = null,
                ViewId = sat.ViewId,
                ObjectState = ObjectState.Added,
                AccountingRemark= adv.Remark
            };
            adv.VoucherId = vchr.Id;
            adv.fk_Voucher = vchr;
                

            #endregion

            #region VD1  dr

            var vchrvd = new VoucherDetail()
            {
                AccountId = vchr.Account1Id.GetValueOrDefault(),
                Amount = vchr.Amount1,
                OfficeId = sat.OfficeId,
                OrderId = 1,
                VoucherId = vchr.Id,
                Voucher = vchr,
                ObjectState = ObjectState.Added,
                Narration = adv.Remark
            };
            vchr.VoucherDetails.Add(vchrvd);
            #endregion

            #region VD2 cr
            var vchrvd2 = new VoucherDetail()
            {
                VoucherId = vchr.Id,
                Voucher = vchr,
                AccountId = vchr.Account2Id.GetValueOrDefault(),
                Amount = vchr.Amount2,
                OfficeId = sat.OfficeId,
                OrderId = 2,
                ObjectState = ObjectState.Added,
                Narration = adv.Remark
            };
            vchr.VoucherDetails.Add(vchrvd2);
            #endregion

            #region VDR

            var vchrvd1 = new VoucherDetailReference()
            {
                Amount = vchrvd2.Amount,
                ReferenceNo = sat.TripSheetNo,
                VDRTypeId = 1013, //New Reference
                VoucherDetailId = vchrvd2.Id,
                fk_VoucherDetail = vchrvd2,
                ObjectState = ObjectState.Added
            };
            adv.fk_VDR = vchrvd1;
            adv.VDRId = vchrvd1.Id;
            vchrvd2.VoucherDetailReferences.Add(vchrvd1);
            #endregion               
            repo.Insert(vchr);
            }
            adv.ObjectState = ObjectState.Added;
            repo.GetRepository<TripAdvanceLog>().Insert(adv);
            sat.fk_CashPaidAdv = adv;
            sat.CashPaidAdvId = adv.Id;
            sat.ObjectState = ObjectState.Modified;
        }
        public async Task HireSettlementV1(VehicleTripSettlement sat, IUnitOfWorkAsync uow)
        {
            bool isNew = sat.Id == 0;
            var advRepo = _repository.GetRepository<TripAdvanceLog>();
            var ledgerRepo = _repository.GetRepository<Ledger>().Queryable();
            var vRepo = _repository.GetRepository<Voucher>();
            var larRepo = uow.RepositoryAsync<HMArrivalLog>();
            if (sat.HVPId.GetValueOrDefault(0) <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Hire Vehicle Party Account is Required");
            }
            var v = sat.fk_Voucher ?? (sat.VoucherId > 0 ? await vRepo.Queryable().FirstOrDefaultAsync(x => x.Id == sat.VoucherId) ?? new Voucher() : new Voucher());
            if (v == null)
            {
                v = new Voucher();
            }
            if (v.VoucherDetails == null)
            {
                v.VoucherDetails = new List<VoucherDetail>();
            }
            var tladvmappingflag = _repository.GetClientConfigValue<long>("ShowAutoHSOnAdvance",1);            
            var unsettledBalance = (sat.SettledAmount/*TotalHSFreight+TotalLARAmount*/- sat.CashPaid/*Balance paid On Settlement*/ -sat.FuelAdvanceAmt/*Fuel Paid Advances*/-sat.TripAdvanceAmt/*Cash paid Advances*/-sat.TDSAmount/*SettlementTDS*/-sat.PenaltyAmount/*HSTDS+LARTDS*/) + sat.CashDeposited/*Cash Deposited On HS Settlement*/;
            var settledhsamount = (sat.SettledAmount/*TotalHSFreight+TotalLARAmount*/- sat.FuelAdvanceAmt/*Fuel Paid Advances*/- sat.TripAdvanceAmt/*Cash paid Advances*/-  sat.PenaltyAmount/*HSTDS+LARTDS*/) + sat.CashDeposited/*Cash Deposited On HS Settlement*/;
            var baladv = sat.VoucherId>0? await advRepo.Queryable().FirstOrDefaultAsync(x => x.AdvanceTypeId == 98 && x.VoucherId == sat.VoucherId):new TripAdvanceLog { 
            AdvanceTypeId=98,
            ViewId=sat.ViewId,
            };
            #region Deleting Old Voucher

            if (!isNew)
            {
                /*Delete All the VD of Settlement*/
                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVoucherVD] WHERE VoucherId=@p0",v.Id);
                /**/
                var balvids = new List<long?>() { sat.SetlBalFuelVoucherId.GetValueOrDefault(), sat.SetlBalVoucherId.GetValueOrDefault(), sat.NetBalVoucherId.GetValueOrDefault() }.Where(x => x > 0).ToList();
                if (balvids.Any())
                {
                    if (await advRepo.Queryable().AnyAsync(x => x.VoucherId > 0 && x.SettledAdvances.Any() && balvids.Contains(x.VoucherId)))
                    {
                        throw new BusinessException(ErrorCode.TADV106, "Balance of this settlement has been settled in any other settlement or has been reversed");
                    }
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripAdvanceLog] WHERE VoucherId IN ({balvids.JoinStrings(",")})");
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripSettlement] SET SetlBalFuelVoucherId=NULL, SetlBalVoucherId=NULL, NetBalVoucherId=NULL WHERE Id=@p0", sat.Id);
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVouchers] WHERE Id IN ({balvids.JoinStrings(",")})");

                    sat.SetlBalFuelVoucherId = null;
                    sat.SetlBalVoucherId = null;
                    sat.NetBalVoucherId = null;
                }
            }

            #endregion
            if (sat.Id > 0)
            {
                sat.ObjectState = ObjectState.Modified;
                _repository.Attach(sat);
                _repository.Update(sat);
            }
            else
            {
                sat.ObjectState = ObjectState.Added;
                _repository.Attach(sat);
                _repository.Insert(sat);
            }
            await uow.SaveChangesAsync();
            sat.ObjectState = ObjectState.Modified;
            #region 1:Driver Cash Advances/Settled Balances Type Advances 1590: driver Cash Advance

            var cashadvids = sat.vwTripAdvances?.Where(x => !x.IsDeleted).Select(x => x.Id).ToList() ?? new List<long>();
            if (!isNew)
            {
                await uow.ExecSqlQueryAsync(
                    $"UPDATE [dbo].[tTripAdvanceLog] SET SettlementId=NULL  {(tladvmappingflag > 0 ? "" : " ,TripLogId=NULL ")} WHERE SettlementId={sat.Id} {(cashadvids.Any() ? $"AND Id NOT IN({cashadvids.JoinStrings(",")})" : "")}");
            }
            var cashadvances = cashadvids.Any() ? await advRepo.Queryable().Include(x => x.SettledAdvances).Where(x => cashadvids.Contains(x.Id)).ToListAsync() : new List<TripAdvanceLog>();
            cashadvances?.ForEach(x =>
            {
                x.SettlementId = sat.Id;
                x.fk_Settlement = sat;
                x.ObjectState = ObjectState.Modified;
            });
            
            #endregion

            #region Trip Log Mapping  : 1:VehicleMovementlog
            var tlids = sat.vwTripLogs?.Where(x => !x.IsDeleted).Select(x => x.Id).ToList();
            if (tlids != null && tlids.Any())
            {
                var invalids = sat.TripAdvances.Where(x => x.TripLogId == null || !tlids.Contains((long)x.TripLogId)).ToList();
                if (invalids.Any())
                {
                    var invalidrefs = invalids.Select(x => x.ReferenceNo).JoinStrings(",");
                    throw new BusinessException(ErrorCode.GLB106, $"{invalidrefs} Attached Trip Cash Advance is either not mapped to any TripLog or is Mapped to TripLog that is not in this Settlement.");
                }
                if (!isNew)
                {
                    await uow.ExecSqlQueryAsync(
                        $"UPDATE [dbo].[tVehicleMovementLog] SET SettlementId=NULL WHERE SettlementId=@p0;", sat.Id);
                }
                foreach (var tl in sat.vwTripLogs?.Where(x => !x.IsDeleted).ToList())
                {
                    await uow.ExecSqlQueryAsync(
                 $"UPDATE [dbo].[tVehicleMovementLog] SET SettlementId=@p0,KmRunAdd=@p1 WHERE Id=@p2", sat.Id, tl.AddKM, tl.Id);
                }

            }

            #endregion
            #region LAR Details
            var lars_vw_ids = sat.vwTripExpenses.Where(x=>!x.IsDeleted).Select(x => x.Id).ToArray();
            if (!isNew)
            {
                await uow.ExecSqlQueryAsync(
                    $"UPDATE [dbo].[tHMArrivalLog] SET SettlementId=NULL WHERE SettlementId={sat.Id} {(lars_vw_ids.Any() ? $"AND Id NOT IN({lars_vw_ids.JoinStrings(",")})" : "")}");
            }
            decimal laramt = 0;
            if (lars_vw_ids!=null && lars_vw_ids.Any())
            {
                await uow.ExecSqlQueryAsync(
                        $"UPDATE [dbo].[tHMArrivalLog] SET SettlementId={sat.Id} WHERE Id IN({lars_vw_ids.JoinStrings(",")})");
                var lar_details = larRepo.Queryable().Where(x=>lars_vw_ids.Contains(x.Id)).Select(x => new {x.Amount,x.TDSAmount,x.NetPayable }).ToList();
                var vdlar = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = sat.HVPId.GetValueOrDefault(0),
                    Amount = laramt=lar_details.Sum(x=>x.NetPayable),
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 6,
                    Narration="LAR Balance Payment"
                };
                settledhsamount = settledhsamount - lar_details.Sum(x => x.Amount)+ lar_details.Sum(x => x.TDSAmount);
                v.VoucherDetails.Add(vdlar);
            }
            #endregion
            #region VDS
            var vdbill = new VoucherDetail()
            {
                Voucher = v,
                AccountId = sat.HVPId.GetValueOrDefault(0),
                //Amount = sat.SettledAmount-laramt,
                Amount= settledhsamount,
                ObjectState = ObjectState.Added,
                OfficeId = sat.OfficeId,
                OrderId = 1,
                Narration="HireSlip Balance Payment"
            };
            v.VoucherDetails.Add(vdbill);
            if (sat.TDSAmount > 0)
            {
                if (sat.TDSAccountId.GetValueOrDefault(0) <= 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, "TDS Account is Required");
                }
                var vd = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = sat.TDSAccountId.GetValueOrDefault(0),
                    Amount = -Math.Abs(sat.TDSAmount),
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 2,
                    Constant1Id=1910
                };
                v.VoucherDetails.Add(vd);
            }
            if (sat.CashDeposited > 0)
            {
                if (sat.SettlementAccountId.GetValueOrDefault(0) <= 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Cash or Bank Account is Required");
                }
                var vd = new VoucherDetail()
                {
                    Voucher = v,
                    AccountId = sat.SettlementAccountId.GetValueOrDefault(0),
                    Amount = Math.Abs(sat.CashDeposited),
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 3,
                    VoucherId = v.Id
                };
                v.VoucherDetails.Add(vd);
            }
            if (sat.CashPaid > 0)
            {
                if (sat.SettlementAccountId.GetValueOrDefault(0) <= 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Cash or Bank Account is Required");
                }
                var vd = new VoucherDetail()
                {
                    VoucherId = v.Id,
                    Voucher = v,
                    AccountId = sat.SettlementAccountId.GetValueOrDefault(0),
                    Amount = -Math.Abs(sat.CashPaid),
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 4
                };
                v.VoucherDetails.Add(vd);
            }
            if (unsettledBalance != 0)
            {
                var vd = new VoucherDetail()
                {
                    VoucherId = v.Id,
                    Voucher = v,
                    AccountId = sat.HVPId.GetValueOrDefault(0),
                    Amount = -unsettledBalance,
                    ObjectState = ObjectState.Added,
                    OfficeId = sat.OfficeId,
                    OrderId = 5
                };
                v.VoucherDetails.Add(vd);
                if (baladv != null)
                {
                    baladv.DebitAccountId = baladv.CreditAccountId = sat.HVPId;
                    baladv.CashAmount = -unsettledBalance;
                    baladv.AdvanceDate = sat.SettleDate.Value;
                    baladv.AdvanceTypeId = 98;
                    baladv.ObjectState = baladv.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    baladv.OfficeId = sat.OfficeId;
                    baladv.ReferenceNo = baladv.VoucherNo = sat.TripSheetNo;
                    baladv.VoucherId = sat.VoucherId;
                    baladv.fk_Voucher = v;
                    baladv.Remark = $"Unsettled Amount carry forwarded for Settlement Number {sat.TripSheetNo} dated:{sat.SettleDate}";
                    if (baladv.Id == 0)
                    {
                        advRepo.Insert(baladv);
                    }
                    else
                    {
                        advRepo.Update(baladv);
                    }
                }              
                
            }
            else if(baladv!=null&&baladv.Id>0)
            {
                baladv.ObjectState = ObjectState.Deleted;
                advRepo.Delete(baladv);
            }
            #endregion

            #region Settlement voucher
            v.ViewId = sat.ViewId;
            v.OfficeId = sat.OfficeId;
            v.VoucherNo = sat.TripSheetNo;
            v.VoucherDate = sat.SettleDate.Value.Date;
            v.VoucherDateTime = sat.SettleDate.Value;
            v.VoucherAmount = v.VoucherDetails.Where(x => x.Amount > 0).Sum(x => x.Amount);//s.fk_Voucher.VoucherDetails.Sum(x => x.Amount);
            v.VoucherTypeId = 98;

            sat.ObjectState = ObjectState.Modified;
            sat.fk_Voucher = v;
            sat.VoucherId = v.Id;
            vRepo.Attach(v);
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            #endregion
            

        }
    }
}
