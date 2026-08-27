using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using EntityFramework.Extensions;
using Repository.Pattern.DataContext;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;

namespace TrackoAPI.Code.Logics.FMS
{
    public class TripAdvanceCoreLogic : IBaseLogic
    {
        //private static TripAdvanceCoreLogic _instance;
        //public static TripAdvanceCoreLogic Instance => _instance ?? (_instance = new TripAdvanceCoreLogic());
        public bool SaveAfterPostLogic { get; private set; }
        private IDataContextAsync _db;
        private DbSet<TripExpenseLog> _telRepo;
        private int FuelAutomationFlag;

        public IBaseLogic Bind(IDataContextAsync db)
        {
            _telRepo = db.Set<TripExpenseLog>();
            _db = db;
            FuelAutomationFlag = _db.GetApiConfig<int>("RunFuelAutomationProcess");
            return this;
        }

        public void Execute(DbEntityEntry entry)
        {
           Execute(entry, false);
            SaveAfterPostLogic = false;
        }

        public void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = false;
            if (!isPostLogicCall)
            {
                VerifyAdvanceTripLogVehicleInegraty(entry);
                PreLogic(entry);
            }
            else
            {
                PostLogic(entry);
            }
        }
        private void VerifyAdvanceTripLogVehicleInegraty(DbEntityEntry entry)
        {
            try
            {
                if (!(entry.Entity is TripAdvanceLog entity)) return;
                if (new long?[] { 85/*Trip Settlement Balance[Fuel]*/, 17/*CashSettledBalance*/, 86/*Settlement Balance[Deposit]*/ }.Contains(entity.AdvanceTypeId)) return;
                if (entity.TripLogId > 0 && (entity.ObjectState == ObjectState.Added || entity.ObjectState == ObjectState.Modified))
                {
                    var tlvehicle = _db.Set<VehicleMovementLog>().Where(x => x.Id == entity.TripLogId).Select(x => new { x.VehicleId, x.HireVehicleId }).FirstOrDefault();
                    var includedTypes = new List<long?>() { 1, 2, 3, 15, 16, 13, 11, 16, 88 };
                    if ((entity.VehicleId != tlvehicle.VehicleId || entity.HireVehicleId != tlvehicle.HireVehicleId) && includedTypes.Contains(entity.AdvanceTypeId))
                    {
                        throw new BusinessException(ErrorCode.TADV107, $@"Advance Reference No :{entity.ReferenceNo}");
                    }

                }
            }
            catch(BusinessException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                //e.ToExceptionless().AddObject(entry.Entity).Submit();
            }
            
        }
        public void PostLogic(DbEntityEntry entry)
        {
            if (!(entry.Entity is TripAdvanceLog entity)) return;
            
            if (entity.HireVehicleId > 0|| FuelAutomationFlag<=0) return;            
            RunFuelAutomation(entity);
        }

        private void PreLogic(DbEntityEntry entry)
        {
            if(!(entry.Entity is TripAdvanceLog entity))return;
            if(entity.HireVehicleId>0)return;
            //if (entity.DriverId.GetValueOrDefault() <= 0)
            //{
            //    throw new BusinessException(ErrorCode.GLB106,$@"Driver is Required for Advance No {entity.ReferenceNo}");
            //}
            
            var settlement = _db.ChangeTracker.Entries<VehicleTripSettlement>().FirstOrDefault();
            var issettled = (entity.SettlementId > 0 || settlement!=null);
            
            switch (entity.ObjectState)
            {
                case ObjectState.Added:
                    NewExpense(entity, issettled);
                    AdvanceRevereseLogic(entity);
                    if (entity.AdvanceTypeId == 3)
                    {
                        entity.BalanceQty = entity.FuelQty > 0 ? entity.FuelQty : entity.RequestQty;
                    }
                    break;
                case ObjectState.Modified:
                    if (issettled&& entity.AdvanceTypeId != 3) return;
                    if (entity.AdvanceTypeId != 3)
                    {
                        var existingExp = _telRepo.FirstOrDefault(x => x.TripAdvanceLogId == entity.Id);
                        if (existingExp != null)
                        {
                            if (existingExp.ObjectState != ObjectState.Deleted)
                            {
                                if (entity.TripLogId.GetValueOrDefault() == 0)
                                {
                                    existingExp.ObjectState = ObjectState.Deleted;
                                    _telRepo.Remove(existingExp);
                                    break;
                                }
                                else
                                {
                                    existingExp.ClaimAmount = entity.Amount;
                                    existingExp.BudgetedQty = 0;
                                    if (entity.ExpenseId > 0)
                                    {
                                        existingExp.ExpenseTypeId = entity.ExpenseId.GetValueOrDefault();
                                    }
                                    existingExp.FuelQty = entity.FuelQty;
                                    existingExp.FuelRate = entity.FuelRate;
                                    existingExp.SettledAmount = entity.Amount;
                                    existingExp.TripLogId = entity.TripLogId.GetValueOrDefault();
                                    existingExp.ViewId = entity.ViewId;
                                    existingExp.ObjectState = ObjectState.Modified;
                                    _telRepo.AddOrUpdate(existingExp);
                                }
                            }
                        }
                        else
                        {
                            NewExpense(entity, false);
                        }
                        AdvanceRevereseLogic(entity);
                    }
                     //var ento = entry.OriginalValues.ToObject();
                    // var entc = entry.CurrentValues.ToObject();
                    //var jsonSetting= new JsonSerializerSettings()
                    // {
                    //     ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    // };
                    // var origional = JsonConvert.SerializeObject(ento, jsonSetting);
                    // var current = JsonConvert.SerializeObject(entc, jsonSetting);
                    // var jdp = new JsonDiffPatch();
                    // var left = JToken.Parse(origional);
                    // var right = JToken.Parse(current);

                    // var patch = jdp.Diff(left, right)?.ToString(Formatting.Indented);
                    if (entity.AdvanceTypeId == 3)
                    {
                        if (entity.RequestStatusId.GetValueOrDefault(1597) == 1597) {

                            var existingFuels = _telRepo.Where(x => x.TripAdvanceLogId == entity.Id).Select(x=>new { Qty = (decimal?)(x.ShortFuelQty + x.FuelQty),x.Id }).ToList();
                            var explist = this._db.Set<TripExpenseLog>().Local.Where(x=> entity.Id== x.TripAdvanceLogId);
                            foreach (var ad in explist)
                            {
                                if (existingFuels.All(x => x.Id != ad.Id))
                                {
                                    existingFuels.Add(new { Qty = (decimal?)ad.FuelQty + ad.ShortFuelQty, Id = ad.Id });
                                }
                            }
                            if (!existingFuels.Any())
                            {
                                entity.BalanceQty = entity.FuelQty > 0 ? entity.FuelQty : entity.RequestQty;
                            }
                            else
                            {
                                entity.BalanceQty = entity.FuelQty-existingFuels.Sum(x => x.Qty.GetValueOrDefault(0));
                            }
                            
                        }
                        else
                        {
                            entity.BalanceQty = entity.FuelQty > 0 ? entity.FuelQty : entity.RequestQty;
                        }
                       
                    }
                    break;
                case ObjectState.Deleted:
                    if (entity.AdvanceTypeId != 3)
                    {
                        var existingExpD = _telRepo.FirstOrDefault(x => x.TripAdvanceLogId == entity.Id);
                        if (existingExpD != null)
                        {
                            existingExpD.ObjectState = ObjectState.Deleted;
                            _telRepo.Remove(existingExpD);
                        }
                    }
                    else
                    {
                        if (_telRepo.Any(x => x.TripAdvanceLogId == entity.Id))
                        {
                            throw new BusinessException(ErrorCode.GLB106, $"The Advance with Ref number {entity.ReferenceNo} has been consumed. First freeup it from consuption by deleted Expense entries");
                        }
                    }
                    if (entity.AdvanceTypeId != 94 && _db.Set<TripAdvanceLog>().Any(x => x.SettledRefId == entity.Id && x.AdvanceTypeId == 94))
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Advance has been reveresed so cannot be modfied");
                    }
                    break;
            }

            
        }
        private void AdvanceRevereseLogic(TripAdvanceLog adv)
        {
            
            if (adv.AdvanceTypeId == 94 && adv.SettledRefId > 0)
            {
                var repo = _db.Set<TripAdvanceLog>();
                var advsettled = repo.Where(x => x.Id == adv.SettledRefId).Select(x => new
                {
                    Settled=x.SettledAdvances.Where(y=>y.Id!=adv.Id).Select(z=>new
                    {
                        z.CashAmount,
                        z.FuelQty,
                        z.FuelAmount
                    }),
                    x.FuelQty,
                    x.FuelAmount,
                    x.CashAmount,
                    x.SettlementId,
                    x.AdvanceTypeId
                }).FirstOrDefault();
                if (advsettled == null) throw new BusinessException(ErrorCode.GLB106, "Provided Advance for Reversal not found");
                if (advsettled.SettlementId.GetValueOrDefault()>0) throw new BusinessException(ErrorCode.TADV106, "Provided Advance for Reversal has been settled in Trip Settlement");
                
                if (advsettled.CashAmount > 0 && adv.CashAmount == 0|| advsettled.CashAmount < adv.CashAmount + advsettled.Settled.Sum(x => x.CashAmount))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Provided Advance for Reversal has invalid Amount Rs.{adv.CashAmount} maxmum amount can be settled is {advsettled.CashAmount- advsettled.Settled.Sum(x => x.CashAmount)}");
                }
                if (advsettled.FuelQty > 0 && adv.FuelQty == 0 || advsettled.FuelQty < adv.FuelQty + advsettled.Settled.Sum(x => x.FuelQty))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Provided Advance for Reversal has invalid Fuel Qty Rs.{adv.CashAmount} maxmum  Fuel Qty can be settled is {advsettled.FuelQty - advsettled.Settled.Sum(x => x.FuelQty)}");
                }
                var exp= _telRepo.FirstOrDefault(x => x.TripAdvanceLogId == adv.SettledRefId);
                if (exp != null)
                {
                    exp.SettledAmount = advsettled.CashAmount - advsettled.Settled.Sum(x => x.CashAmount) + adv.CashAmount;
                    exp.ClaimAmount = exp.SettledAmount;
                    exp.FuelQty= advsettled.FuelQty - advsettled.Settled.Sum(x => x.FuelQty) + adv.FuelQty;
                    exp.ObjectState = ObjectState.Modified;
                    _telRepo.AddOrUpdate(exp);
                }
            }            
        }
        private void RunFuelAutomation(TripAdvanceLog adv)
        {
            
            if(FuelAutomationFlag == 0)return;
            
            if (adv.VehicleId > 0 && adv.RequestStatusId.GetValueOrDefault(1597) == 1597&&adv.FuelQty>0 && adv.AdvanceTypeId == 3 &&
                adv.ObjectState!=ObjectState.Deleted)
            {
                _db.ExecuteProcedureAsync("[dbo].[Proc_TRANS_FuelAutomationHandle]",
                    new[] { new SqlParameter("TriplogId", adv.TripLogId??0), new SqlParameter("VehicleId", adv.VehicleId)});
            }
        }
        private void NewExpense(TripAdvanceLog entity,bool issettled)
        {
            
            if (FuelAutomationFlag == 1) return;
            if (!(entity.TripLogId > 0)) return;
            if (issettled) return;
            if ((entity.CashAmount + entity.FuelAmount) == 0) return;
            var exp=new TripExpenseLog()
            {
                ClaimAmount = entity.Amount,
                BudgetedQty = 0,
                ExpenseTypeId = entity.ExpenseId.GetValueOrDefault(),
                FuelQty = entity.FuelQty,
                FuelRate = entity.FuelRate,
                ObjectState = ObjectState.Added,
                SettledAmount = entity.Amount,
                TripLogId = entity.TripLogId.GetValueOrDefault(),
                ViewId = entity.ViewId
            };
            if (exp.ExpenseTypeId == 0&&exp.FuelQty>0)
            {
                var expType = _db.Set<ExpenseMaster>().Where(x => x.NatureId == 1479)
                    .Select(x => new {x.Id}).FromCacheFirstOrDefault();
                exp.ExpenseTypeId = expType?.Id ?? 0;
            }

            if (exp.ExpenseTypeId <= 0) return;
            exp.fk_TripAdvanceLog = entity;
            exp.TripAdvanceLogId = entity.Id;
            _telRepo.Add(exp);
            entity.FuelExpanses.Add(exp);
        }

        
    }
}
