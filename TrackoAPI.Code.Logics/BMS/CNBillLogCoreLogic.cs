using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Extensions;
using Repository.Pattern.DataContext;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global.DTS;

namespace TrackoAPI.Code.Logics.BMS
{
    public class CNBillLogCoreLogic : BaseLogic<CNBillLog>
    {
        //protected static CNBillLogCoreLogic _Instance;
        //public static CNBillLogCoreLogic Instance => _Instance ?? (_Instance = new CNBillLogCoreLogic());

        //protected IDataContextAsync _db;
        //public override IBaseLogic Bind(IDataContextAsync db)
        //{
        //    _db = db;
        //    return this;
        //}

        //public override void Execute(DbEntityEntry entry)
        //{
        //    Execute(entry, false);
        //}

        /// <summary>
        /// Executes the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="isPostLogicCall">if set to <c>true</c> [is post logic call].</param>
        public override void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            var cnbillog = entry.Entity as CNBillLog;

            if (cnbillog != null)
            {
                if (cnbillog.fk_Bill == null)
                {
                    cnbillog.fk_Bill = this._db.Set<CNBill>().Find(cnbillog.BillId);
                }
                var natureid = cnbillog.fk_Bill.BillNatureId;

                if ((cnbillog.ObjectState == ObjectState.Modified|| cnbillog.ObjectState == ObjectState.Added) && _db.Set<CNBillNature>().Any(x=>x.CNBillTypeId==1363&&x.Id==natureid))
                {
                    var prpaidamt =
                        _db.Set<CNBillLog>()
                            .Where(x => x.CNId == cnbillog.CNId && x.fk_Bill.fk_BillNature.CNBillTypeId == 1363 && x.Id != cnbillog.Id)
                            .Sum(x => (decimal?)x.CNFreight) ?? 0;
                    var cnamount =
                        _db.Set<CNMaster>()
                            .Where(x => x.Id == cnbillog.CNId)
                            .Select(x =>
                            new {
                                x.CNSubTotalII,
                                x.CNNo
                            }).FirstOrDefault()??new{ CNSubTotalII =(decimal)0, CNNo =""};
                    if (cnamount.CNSubTotalII < (prpaidamt + cnbillog.CNFreight))
                    {
                        throw new BusinessException(ErrorCode.GLB106, $"CN Billed freight Previous Billed={prpaidamt}+(In This Bill={cnbillog.CNFreight})=Total Billed={(prpaidamt + cnbillog.CNFreight)}  can not exceed CN booked Fright {cnamount.CNSubTotalII} for CN No {cnamount.CNNo}");
                    }
                }
                if (cnbillog.ObjectState == ObjectState.Modified)
                {
                    cnbillog.BalanceAmount = cnbillog.TotalBillAmount;
                    var paidamt =
                        _db.Set<CNBillPaymentLog>()
                            .Where(x => x.BillLogId == cnbillog.Id)
                            .Sum(x => (decimal?)x.Amount) ?? 0;
                    if (paidamt > 0)
                    {
                        cnbillog.BalanceAmount = cnbillog.TotalBillAmount - paidamt;
                    }
                }
                else if (cnbillog.ObjectState == ObjectState.Added)
                {
                    cnbillog.BalanceAmount = cnbillog.TotalBillAmount;
                }
                switch (cnbillog.ObjectState)
                {
                    case ObjectState.Added:
                    case ObjectState.Modified:
                        //1566
                        AddStatusMap(entry);
                        break;
                    case ObjectState.Deleted:
                        RemoveStatusMap(cnbillog);
                        break;
                }
            }
            
            
        }

        private void SalesLogMap(CNBillLog log)
        {
            var salesConfig = _db.GetApiConfig<int>("GenerateSalesVoucher");
            if (salesConfig == 1)
            {
               
            }
        }
        private void RemoveStatusMap(CNBillLog entity)
        {
            if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var statusid = _db.GetDTSStatusIdByDateId(1566);
            
            if (statusid == 0) return;

            var cndts = repo.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.CNId == entity.CNId && x.StatusId == statusid);
            if (cndts != null)
            {
                cndts.ObjectState = ObjectState.Deleted;
                new CNDTSStatusCoreLogic().Bind(_db).Execute(_db.Entry(cndts));
            }

        }
        private void AddStatusMap(DbEntityEntry entry)
        {
            var entity = entry.Entity as CNBillLog;
            if (entity==null||_db.GetApiConfig<int>("IsCNTrackEnabled") == 0||entity.CNId.GetValueOrDefault()<=0) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var statusid = _db.GetDTSStatusIdByDateId(1566);
            if(statusid==0)return;
            var cndts = repo.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.CNId == entity.CNId && x.StatusId == statusid) ?? new CNDTSStatusLog();
            if (cndts.NextLogId.GetValueOrDefault() <= 0)
            {
                if ((entity.fk_Bill?.Id).GetValueOrDefault(0) == 0)
                {
                    entity.fk_Bill = _db.Set<CNBill>().FirstOrDefault(x => x.Id==entity.BillId);
                }
                if(entity.fk_Bill==null)return;
                cndts.CNId = entity.CNId.GetValueOrDefault();
                cndts.IsAuto = true;
                cndts.StartDate = entity.fk_Bill.BillDate;
                cndts.OfficeId1 = entity.fk_Bill.BillOfficeId;
                cndts.ObjectState = cndts.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                cndts.Qty = 0;
                cndts.StatusId = statusid;
                cndts.Remark = $"Bill No:{entity.fk_Bill.BillNo}, Dated :{entity.fk_Bill.BillDate:D}";
                if (cndts.PreviousLogId.GetValueOrDefault() <= 0)
                {
                    var previousLog = repo.Local.OrderByDescending(x => x.StartDate.Date).ThenByDescending(x => x.Id).FirstOrDefault(
                                          x =>
                                              x.CNId == entity.CNId &&
                                              x.StartDate.Date<= cndts.StartDate.Date&&x.StatusId!=cndts.StatusId) ??//&& ( x.Id == 0||x.Id != entity.Id ) && (x.Id == 0||x.Id != nextlogid) 
                                      repo
                                          .OrderByDescending(x => DbFunctions.TruncateTime(x.StartDate))
                                          .ThenByDescending(x => x.Id)
                                          //.Include(x => x.fk_NextLog)
                                          .FirstOrDefault(
                                              x =>
                                                  x.CNId == entity.CNId &&
                                                  DbFunctions.TruncateTime(x.StartDate) <= DbFunctions.TruncateTime(cndts.StartDate) && x.Id != entity.Id && x.StatusId != cndts.StatusId);
                    cndts.PreviousLogId = previousLog?.Id;
                    cndts.fk_PreviousLog = previousLog;
                    if (previousLog != null)
                    {
                        previousLog.NextLogId = cndts.Id;
                        previousLog.fk_NextLog = cndts;
                        previousLog.NextLogId = cndts.Id;
                        previousLog.EndDate = cndts.StartDate;
                        previousLog.ConsumedMinutes =
                            previousLog.EndDate.GetValueOrDefault(DateTime.Now).Subtract(previousLog.StartDate).Minutes;
                        previousLog.ObjectState = previousLog.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        repo.AddOrUpdate(previousLog);
                    }
                }
                
                repo.AddOrUpdate(cndts);

                var dts = (CNDTSStatusCoreLogic)new CNDTSStatusCoreLogic().Bind(_db);
                dts.CreateNextStatusAuto(cndts);
                //SaveAfterPostLogic = true;
            }

        }
        //public override bool SaveAfterPostLogic { get; set; }
        //public override DbSet<CNBillLog> DbSet => _db.Set<CNBillLog>();
    }
}