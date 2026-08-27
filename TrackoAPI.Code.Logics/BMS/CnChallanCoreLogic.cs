using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Caching;
using Repository.Pattern.DataContext;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global.DTS;
using EntityFramework.Extensions;

using TrackoAPI.Code.Logics.BMS;

namespace TrackoAPI.Code.Logics
{
    public class CnChallanCoreLogic: IBaseLogic
    {
        //protected static CnChallanCoreLogic _Instance;
        //public static CnChallanCoreLogic Instance => _Instance ?? (_Instance = new CnChallanCoreLogic());

        protected IDataContextAsync _db;
        public DbSet<CnChallan> CnChallans;
        
        public IBaseLogic Bind(IDataContextAsync db)
        {
            _db = db;
            CnChallans = _db.Set<CnChallan>();
            
            return this;
        }
        
        public void Execute(DbEntityEntry entry)
        {
            Execute(entry,false);
        }

        /// <summary>
        /// Executes the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="isPostLogicCall">if set to <c>true</c> [is post logic call].</param>
        public void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = false;
            RunPostLogic = isPostLogicCall;
            if (!isPostLogicCall)
            {
                //PreLogic(entry);
                PreLogicNew(entry);
            }
            if (isPostLogicCall)
            {
                if(!(entry.Entity is CnChallan chcn))return;
                try
                {
                    var createStockMMForDirectDelivery = (_db.GetApiClientConfig("CreateStockMMLogForDD", 1) == 1 && chcn.DeliveryTypeId == 1472/*Direct Delivery*/) || chcn.DeliveryTypeId != 1472;
                    if (createStockMMForDirectDelivery)
                    {
                        _db.Database.ExecuteSqlCommand($"EXEC [dbo].[Proc_TRANS_CreateTLCHMMLog]{chcn.Id},1");
                    }
                }
                catch (Exception ex)
                {
                    throw new BusinessException(ErrorCode.GLB106, ex.GetBaseException().Message);
                }

                if (_db.GetApiConfig<int>("IsCNTrackEnabled") != 1) return;
                chcn.CnStockLogs?.ForEach(item => { 
                    //AddStatusMap(item);
                    try
                    {
                        _db.Database.ExecuteSqlCommand($"EXEC [dbo].[Proc_TRANS_1555_CreateDTSForStockLog]{item.Id}");
                    }
                    catch (SqlException ex)
                    {
                        throw new BusinessException(ErrorCode.GLB106, ex.GetBaseException().Message);
                    }
                });

            }
        }

        public bool RunPostLogic { get; private set; } = false;
        public bool SaveAfterPostLogic { get; private set; } = false;

        public void PreLogicNew(DbEntityEntry entry)
        {
            if (!(entry.Entity is CnChallan chcn)) return;
            var cnmmstockRepo = _db.Set<CNStockMMLog>();
            var stockRepo = _db.Set<CNStockLog>();
            CnChallan originalEntity = chcn;
            switch (chcn.ObjectState)
            {
                case ObjectState.Added:
                case ObjectState.Modified:
                    
                    if (chcn.Id != 0)
                    {
                        try
                        {
                            originalEntity= (CnChallan)entry.OriginalValues.ToObject();
                        }
                        catch
                        {
                            //Ignore
                        }
                    }                    
                    long formid = chcn.ViewId??0;
                    if (chcn.fk_Challan == null && chcn.ChallanId > 0)
                    {
                        chcn.fk_Challan = _db.Set<ChallanMaster>().Find(chcn.ChallanId);

                    }
                    if (chcn.fk_Challan != null)
                    {
                        if (chcn.fk_Challan.TriplogId.GetValueOrDefault(0) == 0 &&
                            chcn.TriplogId.GetValueOrDefault(0) > 0)
                        {
                           chcn.fk_Challan.TriplogId = chcn.TriplogId;
                        }
                        else if (chcn.fk_Challan.TriplogId.GetValueOrDefault(0) > 0&& chcn.TriplogId.GetValueOrDefault(0)==0)
                        {
                            chcn.TriplogId= chcn.fk_Challan.TriplogId;
                        }
                        else if (chcn.fk_Challan.TriplogId.GetValueOrDefault(0) > 0 &&
                                 chcn.TriplogId.GetValueOrDefault(0) > 0 &&
                                 chcn.fk_Challan.TriplogId.GetValueOrDefault(0) != chcn.TriplogId.GetValueOrDefault(0))
                        {
                            ////TODO:Which TripLogId should be considred as correct?
                        }
                        
                    }
                    if (chcn.fk_Triplog == null && chcn.TriplogId > 0)
                    {
                        chcn.fk_Triplog = _db.Set<VehicleMovementLog>().Find(chcn.TriplogId);
                    }

                    if (chcn.ViewId.GetValueOrDefault() == 0)
                    {
                        if (chcn.fk_Triplog != null)
                        {
                            long.TryParse(chcn.fk_Triplog.FormId, out formid);
                            chcn.ViewId = formid;
                        }
                        if (chcn.fk_Challan != null)
                        {
                            chcn.ViewId = formid = chcn.fk_Challan.ViewId;
                        }

                    }
                    if (chcn.DeliveryTypeId.GetValueOrDefault() == 0 || chcn.LogTypeId.GetValueOrDefault() <= 0)
                    {
                        chcn.LogTypeId = (chcn.TriplogId.GetValueOrDefault() == 0 && (chcn.fk_Triplog?.ObjectState != ObjectState.Added)) ? 1454/*Loading Awaited*/ : (chcn.fk_Triplog?.TripTypeId == 1453/*Local Delivery*/|| chcn.DeliveryTypeId == 1472/*Direct Delivery*/ ? 1451 : 1423);
                    }
                    else
                    {
                        chcn.LogTypeId = chcn.DeliveryTypeId.GetValueOrDefault() == 1545 ? 1423 : 1451;
                    }

                    if (chcn.RefStockId.GetValueOrDefault() == 0) throw new BusinessException(ErrorCode.GLB106, $"One of attached CN is not valid for stock movement.\nVerify each attached cn are in stock of selected office. Hint:CNId:{chcn.CNId}");

                    //Try to fetch challan if challanid has value and officeid or routeid don't have value
                    if (chcn.fk_Triplog == null && chcn.fk_Challan == null && chcn.ChallanId.GetValueOrDefault()>0 && chcn.TriplogId.GetValueOrDefault() ==0 && (chcn.OfficeId == null || chcn.RouteId == null))
                    {
                        chcn.fk_Challan = _db.Set<ChallanMaster>().Find(chcn.ChallanId);
                        if (chcn.fk_Challan == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "CE:Invalid Challan");
                        }
                        chcn.OfficeId = chcn.fk_Challan.OfficeID;
                        chcn.RouteId = chcn.fk_Challan.RouteId;
                    }
                    if (chcn.fk_Triplog == null && chcn.fk_Challan == null && (chcn.ChallanId.HasValue || chcn.TriplogId.HasValue) && (chcn.OfficeId == null || chcn.RouteId == null))
                    {
                        chcn.fk_Triplog = _db.Set<VehicleMovementLog>().Find(chcn.TriplogId);
                        if (chcn.fk_Triplog == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "CE:Invalid TripLog");
                        }
                        chcn.OfficeId = chcn.fk_Triplog.OfficeId;
                        chcn.RouteId = chcn.fk_Triplog.RouteId;
                    }

                    if (chcn.OfficeId == null || chcn.RouteId == null)
                    {
                        chcn.OfficeId = chcn.fk_Challan?.OfficeID ?? chcn.fk_Triplog?.OfficeId;
                        chcn.RouteId = chcn.fk_Challan?.RouteId ?? chcn.fk_Triplog?.RouteId;
                    }


                    chcn.ShipmentDate = chcn.fk_Challan?.ChallanDate ?? chcn.fk_Triplog.LoadingDate.GetValueOrDefault(chcn.fk_Triplog.TripStartDate);

                    if (chcn.fk_Triplog != null /*&& chcn.fk_Triplog.Id > 0 Removed by Mukesh .what if TripLog and CnCHallan are saved in parrallel*/ && (!chcn.ArrivalDate.HasValue &&chcn.fk_Triplog.UnloadingDate.HasValue)/* && chcn.fk_Triplog.FormId != "1503"*/)
                    {
                        chcn.ArrivalDate = chcn.fk_Triplog.UnloadingDate;
                        chcn.ArrivalViewId = formid;
                        
                    }
                    if (chcn.ArrivalDate!=null&&(chcn.ArrivalQty <= 0 || chcn.ArrivalQty != chcn.Qty))
                    {
                        chcn.ArrivalQty = chcn.Qty;
                    }
                    #region Stock Movement
                    //if (chcn.ObjectState == ObjectState.Modified && arrivalDate.CurrentValue == null && arrivalDate.OriginalValue != null && chcn?.fk_Triplog?.UnloadingDate != null)
                    //{
                    //    throw new BusinessException(ErrorCode.GLB106,
                    //        "Trip has ended so you cannot undo arrival of consignment.");
                    //}
                    CNStockLog stockIn = null;
                    CNStockLog stockout = null;
                    CNStockLog transitentry = null;
                    CNStockLog arrivalStock = null;
                    //List<CNStockMMLog> refMMStocks = null;//mm1
                    #region PrePare Stock Out & Transit Stock and Out For Delivery stock


                    #region Prepare Stock Out and Out for delivery
                    if (chcn.tempCNStockMMLogs != null && chcn.tempCNStockMMLogs.Any())
                    {
                        var refmmids = chcn.tempCNStockMMLogs.Select(x => x.RefStockId.GetValueOrDefault()).Distinct().ToList();
                        var refMmStocks =
                            cnmmstockRepo.Where(x => x.CNId == chcn.CNId &&
                                            x.InQty > (x.Outwards.Sum(y => (decimal?)y.OutQty) ?? 0) && refmmids.Contains(x.Id) && (x.LogTypeId == 1422 || x.LogTypeId == 1455)).Select(x => x.Id)
                                //.Include(x => x.fk_StockLog)
                                .ToList();
                        var invaliditems = chcn.tempCNStockMMLogs.Where(x => x.Id == 0).Where(log =>
                            refMmStocks == null || refMmStocks.All(x => x != log.RefStockId)).ToList();
                        if (invaliditems.Any())
                        {
                            throw new BusinessException(ErrorCode.GLB106, $"{invaliditems.Select(x => x.PartName ?? "" + "[Qty:" + x.OutQty + x.InQty + "]").JoinStrings("\n")} are out of stock");
                        }
                    }
                    //if ((chcn.CnStockLogs == null || !chcn.CnStockLogs.Any()) && chcn.Id > 0 &&
                    //    stockRepo.Any(x => x.ChallanCNId == chcn.Id))
                    //{
                    //    stockout =
                    //        stockRepo.Include(x => x.Outwards/*.Select(y => y.StockMMLogs)*/)
                    //            .Include(x => x.RefStock)
                    //            //.Include(x => x.StockMMLogs)
                    //            .FirstOrDefault(
                    //                x =>
                    //                    x.ChallanCNId == chcn.Id && x.CNId == chcn.CNId &&
                    //                    x.RefStockId == chcn.RefStockId);
                       
                    //}
                    
                    if (((chcn.CnStockLogs == null || !chcn.CnStockLogs.Any()) && stockRepo.Any(x => x.ChallanCNId == chcn.Id)))
                    {
                        //If CN Challan is Existing try to fetch the Out StockLog from Database if it's not in db assign new
                        if (chcn.Id > 0)
                        {
                            stockout = stockRepo.Include(x => x.Outwards/*.Select(y => y.StockMMLogs)*/).Include(x => x.RefStock)/*.Include(x => x.StockMMLogs)*/
                                .FirstOrDefault(
                                x => x.ChallanCNId == chcn.Id && x.CNId == chcn.CNId && (x.LogTypeId == 1423 || x.LogTypeId == 1454 || x.LogTypeId == 1451 || x.LogTypeId == 1425 || x.LogTypeId == 1455));
                        }
                        
                    }
                    if(chcn.CnStockLogs!=null&& chcn.CnStockLogs.Any())
                    {
                        stockout = chcn.CnStockLogs.FirstOrDefault(x => x.RefStockId == chcn.RefStockId);
                    }
                    if (stockout != null)
                    {
                        if (stockout.RefStock != null)
                        {
                            stockIn = stockout.RefStock;
                            if (stockout.RefStock.InQty < stockout.OutQty)
                            {
                                stockout.OutQty = stockout.RefStock.InQty;
                            }

                        }
                        stockRepo.Where(x=>x.RefStockId== stockout.Id).Load();
                        if(stockout.Outwards!=null&& stockout.Outwards.Any()&&stockout.LogTypeId!=1423)
                        {
                            stockout.Outwards.ForEach(x => x.ObjectState = ObjectState.Deleted);
                            var pipe = new CNStockLogCoreLogic().Bind(_db);
                            stockout.Outwards.ForEach(x =>
                            {
                                pipe.Execute(_db.Entry(x));
                            });
                        }
                        if (stockout.Outwards != null && stockout.Outwards.Any(x => (x.LogTypeId == 1424/*transit*/|| x.LogTypeId == 1422/*Stocked IN*/)&&x.ObjectState!=ObjectState.Deleted))
                        {
                            transitentry = stockout.Outwards.FirstOrDefault(x => x.LogTypeId == 1424 || x.LogTypeId == 1422);
                            if (transitentry != null)
                            {
                                if (transitentry.OutQty != stockout.OutQty)
                                {
                                    transitentry.OutQty = stockout.OutQty;
                                }
                                if (transitentry.LogTypeId == 1422/*Transit*/ && transitentry.InQty != transitentry.OutQty)
                                {
                                    transitentry.InQty = transitentry.OutQty;
                                }
                            }
                        }
                        if (stockout.OutQty != chcn.Qty)
                        {
                            stockout.OutQty= chcn.Qty;
                        }
                    }
                    else
                    {
                        stockout = new CNStockLog();
                    }
                    stockout.CNId = chcn.CNId;
                    stockout.InQty = 0;
                    stockout.LogDate = chcn.ShipmentDate.Value;
                    stockout.LogTypeId = chcn.LogTypeId.GetValueOrDefault();
                    stockout.ObjectState = stockout.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    stockout.OfficeId = chcn.OfficeId.GetValueOrDefault();
                    stockout.OutQty = chcn.Qty;
                    stockout.ExessQty = chcn.Excess;
                    stockout.DamagedQty = chcn.Damaged;
                    stockout.ShortageQty = chcn.Short;
                    stockout.ChallanCNId = chcn.Id;
                    stockout.fk_ChallanCN = chcn;
                    stockout.TriplogId = chcn.TriplogId;
                    stockout.RefStockId = chcn.RefStockId;
                    stockRepo.AddOrUpdate(stockout);
                    var otheroutqty = stockRepo.Where(x => x.RefStockId == chcn.RefStockId && x.Id != stockout.Id).Sum(y => (decimal?)y.OutQty).GetValueOrDefault();
                    if (chcn.CnStockLogs != null && chcn.CnStockLogs.Any() && stockout == null)
                    {
                        stockout = chcn.CnStockLogs.FirstOrDefault();
                    }
                    if (stockout != null && stockout.Outwards != null && stockout.Outwards.Any(x=>x.ObjectState!=ObjectState.Deleted)&& transitentry==null)
                    {
                        transitentry = stockout.Outwards.FirstOrDefault(x => x.LogTypeId == 1424);
                    }

                    if (stockout?.RefStock != null&& stockIn==null)
                    {
                        stockIn = stockout.RefStock;
                    }
                    if(stockIn==null)
                    {

                        stockIn = stockout.RefStock =
                            stockRepo.FirstOrDefault(x => x.CNId == chcn.CNId && x.Id == chcn.RefStockId);
                        if (stockIn != null)
                        {
                            
                            //new
                            //{
                            //    OutQty=x.Outwards.Where(z=>z.Id!= stockout.Id).Sum(y=>(decimal?)y.OutQty)
                            //    //,OutMMQty=x.Outwards.SelectMany(z=>z.StockMMLogs).Sum(y=> (decimal?)y.OutQty),
                            //    //MMQty = x.StockMMLogs.Sum(y => (decimal?)y.InQty)
                            //});
                            

                        }
                       

                    }
                    if (stockIn == null)
                    {
                        var baseitem = stockRepo.Select(x => new { CNNo = x.fk_CNMaster.CNNo, x.CNId, x.Id })
                            .FirstOrDefault(x => x.CNId == chcn.CNId || x.Id == chcn.RefStockId);
                        throw new BusinessException(ErrorCode.GLB106, $"CN InStock Reference Not Found or CN No {baseitem.CNNo} is Out of Stock.StockRefId({chcn.RefStockId})");
                    }
                    if(stockIn!=null)
                    {
                        if (stockIn.InQty < (otheroutqty + stockout.OutQty))
                        {
                            var baseitem = stockRepo.Select(x => new { CNNo = x.fk_CNMaster.CNNo, x.CNId, x.Id })
                                .FirstOrDefault(x => x.CNId == chcn.CNId || x.Id == chcn.RefStockId);
                            throw new BusinessException(ErrorCode.GLB106, $"CN No {baseitem.CNNo} is Out of Stock. \nHint StockQty({stockIn.InQty}) < OtherOutQty({otheroutqty})+CurrentOutQty({stockout.OutQty}) StockRefId({stockIn.Id}),StockOutId{stockout.Id}");
                        }
                    }
                    if (stockout.RefStockId.GetValueOrDefault()<=0)
                    {
                        stockout.RefStockId = stockIn.Id;
                    }
                    if (stockout.TriplogId.GetValueOrDefault() <= 0 && (chcn.TriplogId.GetValueOrDefault() > 0 || chcn.fk_Triplog?.ObjectState == ObjectState.Added))
                    {
                        stockout.TriplogId = chcn.TriplogId;
                        stockout.fk_Triplog = chcn.fk_Triplog;
                    }
                    if (stockout != null && chcn.LogTypeId > 0)
                    {
                        if (stockout.LogTypeId != chcn.LogTypeId && chcn.LogTypeId == 1451 /*Out for Delivery*/ && stockout.LogTypeId == 1423 /*Stock Out*/&& transitentry != null)
                        {
                            transitentry.ObjectState = ObjectState.Deleted;
                        }
                        stockout.LogTypeId = chcn.LogTypeId.GetValueOrDefault();
                        stockout.ObjectState = ObjectState.Modified;
                    }
                    #endregion

                    #region Prepare Transit Stock
                    if (chcn.TriplogId > 0 && stockout != null && stockout.ObjectState != ObjectState.Unchanged)
                    {
                        if (stockout == null && chcn.Id == 0)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Unable to Trigger Stock Out for one of CN");
                        }
                        //Create or update Transit Entry only when ChallanCN is mapped to TripLog and Out Stock Log Type is 1423 i.e. Stock Out
                        if (chcn.TriplogId > 0||chcn.fk_Triplog?.ObjectState==ObjectState.Added)
                        {
                            if (chcn.fk_Triplog == null && chcn.TriplogId > 0)
                            {
                                chcn.fk_Triplog = _db.Set<VehicleMovementLog>().Find(chcn.TriplogId);
                            }

                            if (stockout.LogTypeId == 1423)
                            {
                                if (transitentry == null)
                                {
                                    transitentry = (stockout.Id == 0
                                                       ? new CNStockLog()
                                                       : stockRepo /*.Include(x => x.StockMMLogs)*/.FirstOrDefault(x => x.ChallanCNId == chcn.Id && x.CNId == chcn.CNId && x.RefStockId == stockout.Id)) ?? new CNStockLog();
                                }

                                //Check if user has not done arrival
                                //if (transitentry.Id > 0 && chcn.Id > 0 &&
                                //    ((chcn.ArrivalViewId != formid && transitentry.LogTypeId != 1424 &&
                                //      chcn.ArrivalDate != null && originalArrivalDate != null) ||
                                //     stockRepo.Any(x => x.RefStockId == transitentry.Id)))
                                //{
                                //    //Incase you want to delete challan or deattach cn from challan delete all entries that are child of Stock in.
                                //    throw new BusinessException(ErrorCode.GLB106,
                                //        "CE:Cannot Modify Arrived/Delivered TripLog");
                                //}

                                transitentry.CNId = chcn.CNId;
                                transitentry.LogDate = chcn.ArrivalDate ?? chcn.fk_Triplog.TripStartDate;
                                transitentry.LogTypeId = chcn.ArrivalDate == null ? 1424 : 1422;
                                transitentry.ObjectState = transitentry.Id > 0
                                    ? ObjectState.Modified
                                    : ObjectState.Added;
                                transitentry.OutQty = chcn.Qty;
                                transitentry.InQty = chcn.ArrivalQty;
                                transitentry.ShortageQty = chcn.Short;
                                transitentry.ExessQty = chcn.Excess;
                                transitentry.DamagedQty = chcn.Damaged;
                                transitentry.ChallanCNId = chcn.Id;
                                transitentry.fk_ChallanCN = chcn;
                                transitentry.RefStockId = stockout.Id;
                                transitentry.RefStock = stockout;
                                transitentry.TriplogId = stockout.TriplogId;
                                transitentry.fk_Triplog = stockout.fk_Triplog;
                                var targetOfficeId =
                                    _db.Set<RouteMaster>()
                                        .Where(x => x.Id == chcn.RouteId)
                                        .Select(x => new {x.fk_ToPlace.ControllingOfficeId})
                                        .FromCacheFirstOrDefault();
                                transitentry.OfficeId = targetOfficeId?.ControllingOfficeId.GetValueOrDefault() != 0
                                    ? targetOfficeId.ControllingOfficeId.Value
                                    : stockout.OfficeId;
                                stockRepo.AddOrUpdate(transitentry);
                            }
                            else if (stockout.LogTypeId == 1451)
                            {
                                stockout.CNId = chcn.CNId;
                                stockout.LogDate = chcn.ShipmentDate??stockout.LogDate;
                                stockout.ObjectState = stockout.Id > 0
                                    ? ObjectState.Modified
                                    : ObjectState.Added;
                                stockout.OutQty = chcn.Qty;
                                stockout.InQty = 0;
                                stockout.ShortageQty = chcn.Short;
                                stockout.ExessQty = chcn.Excess;
                                stockout.DamagedQty = chcn.Damaged;
                                stockout.ChallanCNId = chcn.Id;
                                stockout.fk_ChallanCN = chcn;
                                stockout.RefStockId = stockIn?.Id;
                                stockout.RefStock = stockIn;
                                stockout.TriplogId = chcn.TriplogId;
                                if (stockout.OfficeId <= 0)
                                {
                                    var targetOfficeId =
                                        _db.Set<RouteMaster>()
                                            .Where(x => x.Id == chcn.RouteId)
                                            .Select(x => new { x.fk_ToPlace.ControllingOfficeId })
                                            .FromCacheFirstOrDefault();
                                    stockout.OfficeId = targetOfficeId?.ControllingOfficeId.GetValueOrDefault() != 0
                                        ? targetOfficeId.ControllingOfficeId.Value
                                        : stockout.OfficeId;
                                }
                            }
                        }

                    }
                    #endregion
                    #endregion
                    #region Prepare Arrival/Delivery Of Stock
                    if (chcn.TriplogId.GetValueOrDefault() > 0 || chcn.fk_Triplog?.ObjectState == ObjectState.Added)
                    {
                        long stockType = 0;
                        if (stockout == null || stockout.LogTypeId <= 0)
                        {
                            stockType =
                                stockRepo.Where(x => x.ChallanCNId == chcn.Id && (x.LogTypeId == 1423 || x.LogTypeId == 1451))
                                    .Select(x => x.LogTypeId)
                                    .FirstOrDefault();
                        }
                        else
                        {
                            stockType = stockout.LogTypeId;
                        }
                        if (stockType == 1423) //StockOut
                        {
                            if (transitentry == null || transitentry.LogTypeId <= 0)
                            {
                                arrivalStock =
                                    stockRepo/*.Include(x => x.StockMMLogs)*/.FirstOrDefault(
                                        x => x.ChallanCNId == chcn.Id && x.LogTypeId == 1424);
                            }
                            else
                            {
                                arrivalStock = transitentry;
                            }
                        }
                        else//OutFor Delivery
                        {
                            if (stockout == null || stockout.LogTypeId <= 0)
                            {
                                arrivalStock =
                                    stockRepo/*.Include(x => x.StockMMLogs)*/.FirstOrDefault(
                                        x => x.ChallanCNId == chcn.Id && x.LogTypeId == 1451);
                            }
                            else
                            {
                                arrivalStock = stockout;
                            }
                        }
                        if (chcn.IsDeliveryFailed)
                        {
                            if (chcn.DeliveryFailedDate.GetValueOrDefault(chcn.ShipmentDate.GetValueOrDefault()) ==
                                default(DateTime))
                            {
                                throw new BusinessException(ErrorCode.GLB106, "In case delivery failed, Delivery Failed Date is required.");
                            }

                            chcn.ArrivalQty = chcn.Qty;
                            chcn.Excess = 0;
                            chcn.Short = 0;
                            if (arrivalStock != null)
                            {
                                //AR Deleted as Arrival Date Has been removed
                                arrivalStock.ObjectState = ObjectState.Modified;
                                arrivalStock.LogTypeId = 1455;
                                arrivalStock.ExessQty = chcn.Excess;
                                arrivalStock.DamagedQty = chcn.Damaged;
                                arrivalStock.ShortageQty = chcn.Short;
                                arrivalStock.InQty = arrivalStock.OutQty;
                                arrivalStock.LogDate = chcn.DeliveryFailedDate.GetValueOrDefault(chcn.ShipmentDate.GetValueOrDefault());
                                stockRepo.AddOrUpdate(arrivalStock);
                            }
                        }

                        
                        bool createUpdateArrival = false;
                        //if ((arrivalDate.CurrentValue != null || (chcn.ObjectState != ObjectState.Added && arrivalDate.OriginalValue != null)) && !chcn.IsDeliveryFailed && !stockRepo.Any(x => x.ChallanCNId == chcn.Id && (x.LogTypeId == 1425 || x.LogTypeId == 1422)))
                        if(chcn.ArrivalDate != originalEntity.ArrivalDate)
                        {
                            //if (arrivalDate.CurrentValue != null)
                            //{

                            //    if (arrivalStock == null || stockRepo.Any(x => x.ChallanCNId == chcn.Id && (x.LogTypeId == 1425 || x.LogTypeId == 1422)))
                            //    {
                            //        throw new BusinessException(ErrorCode.GLB106, "Consignment should be in Transit/Out For Delivery stage, to do arrival or to acknowledge delivery");
                            //        //createUpdateArrival = false;
                            //    }
                            //}
                            if (chcn.ObjectState != ObjectState.Added)//&& arrivalDate.CurrentValue != arrivalDate.OriginalValue)
                            {
                                if (chcn.ArrivalDate == null && originalEntity.ArrivalDate != null)//Undo Arrival
                                {
                                    chcn.ArrivalQty = 0;
                                    chcn.Excess = 0;
                                    chcn.Damaged = 0;
                                    chcn.Short = 0;
                                    if (arrivalStock != null)
                                    {
                                        //AR Deleted as Arrival Date Has been removed
                                        arrivalStock.ObjectState = ObjectState.Modified;
                                        arrivalStock.LogTypeId = arrivalStock.LogTypeId == 1422 ? 1424 : 1451;
                                        arrivalStock.ExessQty = chcn.Excess;
                                        arrivalStock.DamagedQty = chcn.Damaged;
                                        arrivalStock.InQty = chcn.ArrivalQty;
                                        arrivalStock.ShortageQty = chcn.Short;
                                        arrivalStock.LogDate = chcn.ShipmentDate.Value;
                                        stockRepo.AddOrUpdate(arrivalStock);
                                    }
                                    createUpdateArrival = false;
                                }
                                else if (chcn.ArrivalDate != null && originalEntity.ArrivalDate == null)
                                {
                                    //Create Arrival
                                    createUpdateArrival = true;
                                }
                                else
                                {
                                    //Validate Arrival
                                    if (transitentry !=null&& chcn.ArrivalDate < transitentry.LogDate)
                                    {
                                        throw new BusinessException(ErrorCode.GLB106, $"Consignment Arrival cannot be done before it's Shipment Date {transitentry.LogDate:dd-MMM-yyyy ddd HH:mm}");
                                    }
                                    if (chcn.ArrivalQty + chcn.Short > chcn.Qty)
                                    {
                                        throw new BusinessException(ErrorCode.GLB106, $"Consignment Arrival Qty cannot be done greater than {chcn.ArrivalQty}");
                                    }
                                    createUpdateArrival = true;
                                }
                            }
                        }
                        if (arrivalStock != null && /*arrivalStock.ObjectState == ObjectState.Added &&*/ chcn.ArrivalDate != null)
                        {
                            if (transitentry != null && chcn.ArrivalDate < transitentry.LogDate)
                            {
                                throw new BusinessException(ErrorCode.GLB106, $"Consignment Arrival cannot be done before it's Shipment Date {transitentry.LogDate:dd-MMM-yyyy ddd HH:mm}");
                            }
                            if (chcn.ArrivalQty + chcn.Short > chcn.Qty)
                            {
                                throw new BusinessException(ErrorCode.GLB106, $"Consignment Arrival Qty cannot be done greater than {chcn.ArrivalQty}");
                            }
                            createUpdateArrival = true;
                        }
                        var defaultlocaldispatchlogtypeid = _db.GetApiConfig<long>("DefaultDispatchLogType");
                        if (createUpdateArrival&& (chcn.fk_Triplog.TripTypeId!=1453/*LocalDispatch*/||(chcn.fk_Triplog.TripTypeId == 1453 && defaultlocaldispatchlogtypeid == 1425)))
                        {
                            arrivalStock.ChallanCNId = chcn.Id;
                            arrivalStock.LogDate = chcn.ArrivalDate.Value;
                            arrivalStock.LogTypeId = chcn.LogTypeId == 1423 ? 1422 : 1425;//fk_Triplog.TripTypeId == 1453 ? 1425 : (chcn.DeliveryTypeId == 1472 ? 1425 : 1422);
                            arrivalStock.CNId = chcn.CNId;
                            arrivalStock.InQty = chcn.ArrivalQty;
                            arrivalStock.ShortageQty = chcn.Short;
                            arrivalStock.ExessQty = chcn.Excess;
                            arrivalStock.DamagedQty = chcn.Damaged;
                            arrivalStock.ObjectState = arrivalStock.Id > 0
                                ? ObjectState.Modified
                                : ObjectState.Added;
                            stockRepo.AddOrUpdate(arrivalStock);
                        }

                    }
                    #endregion
                    #endregion
                    //foreach (var item in chcn.CnStockLogs)
                    //{
                    //    AddStatusMap(item);
                    //}
                    break;
                case ObjectState.Deleted:
                    //Try to fetch challan if challanid has value and officeid or routeid don't have value
                    if (chcn.fk_Triplog == null && chcn.fk_Challan == null && chcn.ChallanId.HasValue && !chcn.TriplogId.HasValue)
                    {
                        chcn.fk_Challan = _db.Set<ChallanMaster>().Find(chcn.ChallanId);
                        if (chcn.fk_Challan == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "CE:Invalid Challan");
                        }
                    }
                    if (chcn.fk_Triplog == null && chcn.fk_Challan == null && (chcn.ChallanId.HasValue || chcn.TriplogId.HasValue))
                    {
                        chcn.fk_Triplog = _db.Set<VehicleMovementLog>().Find(chcn.TriplogId);
                        if (chcn.fk_Triplog == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "CE:Invalid TripLog");
                        }
                    }
                    //if (stockRepo.Any(x => x.ChallanCNId == chcn.Id && (x.LogTypeId == 1425 || x.LogTypeId == 1422)))
                    //{
                    //    throw new BusinessException(ErrorCode.GLB106, "One of Attached Consignment has been delivered or has been arrived at it's destination. So cannot remove it from system.");
                    //}
                    if (chcn.CnStockLogs == null || !chcn.CnStockLogs.Any())
                    {
                        _db.Database.ExecuteSqlCommand($"EXEC Proc_DeleteOutStockLog @ChallanCnId={chcn.Id}");
                    }
                    if (chcn.fk_Triplog != null)
                    {
                        chcn.fk_Triplog.LoadingQty = chcn.fk_Triplog.LoadingQty - chcn.Qty;
                        if (chcn.fk_Triplog.LoadingQty < 0)
                            chcn.fk_Triplog.LoadingQty = 0;
                        chcn.fk_Triplog.LoadedWeight = chcn.fk_Triplog.LoadedWeight - chcn.Weight;
                        if (chcn.fk_Triplog.LoadedWeight < 0)
                            chcn.fk_Triplog.LoadedWeight = 0;
                        chcn.fk_Triplog.ObjectState = ObjectState.Modified;
                        _db.Set<VehicleMovementLog>().AddOrUpdate(chcn.fk_Triplog);
                    }
                    if (chcn.fk_Challan != null)
                    {
                        chcn.fk_Challan.Quantity = chcn.fk_Challan.Quantity - chcn.Qty;
                        if (chcn.fk_Challan.Quantity < 0)
                            chcn.fk_Challan.Quantity = 0;
                        chcn.fk_Challan.Weight = chcn.fk_Challan.Weight - chcn.Weight;
                        if (chcn.fk_Challan.Weight < 0)
                            chcn.fk_Challan.Weight = 0;
                        chcn.fk_Challan.ObjectState = ObjectState.Modified;
                        _db.Set<ChallanMaster>().AddOrUpdate(chcn.fk_Challan);
                    }
                    if (chcn.CnStockLogs != null)
                    {
                        foreach (var item in chcn.CnStockLogs)
                        {
                            RemoveStatusMap(item);
                        }
                    }
                    break;
            }
        }
        public void PreLogic(DbEntityEntry entry)
        {
            var chcn = entry.Entity as CnChallan;
            var cnmmstockRepo = _db.Set<CNStockMMLog>();
            var stockRepo = _db.Set<CNStockLog>();
            switch (chcn.ObjectState)
            {
                case ObjectState.Added:
                case ObjectState.Modified:
                    long viewid = 0;
                    if (long.TryParse(chcn.fk_Triplog?.FormId, out viewid))
                    {
                        chcn.ViewId = viewid;
                    }
                    var arrivalDate = entry.Property("ArrivalDate");
                    if (chcn.ObjectState == ObjectState.Modified && arrivalDate.CurrentValue != null &&
                        arrivalDate.CurrentValue == arrivalDate.OriginalValue)
                    {
                        break;
                    }
                    if (chcn.LogTypeId.GetValueOrDefault() == 0)
                    {
                        chcn.LogTypeId = chcn.TriplogId.GetValueOrDefault() == 0 ? 1454/*Loading Awaited*/ : (chcn.fk_Triplog.TripTypeId == 1453 ? 1451 : 1423);//TriplogId== null?Loading awaited:Stock Out
                    }

                    if (chcn.RefStockId.GetValueOrDefault() == 0) throw new BusinessException(ErrorCode.GLB106, "One of attached CN is not valid for stock movement.\nVerify each attached cn are in stock of selected office.");

                    //Try to fetch challan if challanid has value and officeid or routeid don't have value
                    if (chcn.fk_Triplog == null && chcn.fk_Challan == null && chcn.ChallanId.HasValue && !chcn.TriplogId.HasValue && (chcn.OfficeId == null || chcn.RouteId == null))
                    {
                        chcn.fk_Challan = _db.Set<ChallanMaster>().Find(chcn.ChallanId);
                        if (chcn.fk_Challan == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "CE:Invalid Challan");
                        }
                        chcn.OfficeId = chcn.fk_Challan.OfficeID;
                        chcn.RouteId = chcn.fk_Challan.RouteId;
                    }
                    if (chcn.fk_Triplog == null && chcn.fk_Challan == null && (chcn.ChallanId.HasValue || chcn.TriplogId.HasValue) && (chcn.OfficeId == null || chcn.RouteId == null))
                    {
                        chcn.fk_Triplog = _db.Set<VehicleMovementLog>().Find(chcn.TriplogId);
                        if (chcn.fk_Triplog == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "CE:Invalid TripLog");
                        }
                        chcn.OfficeId = chcn.fk_Triplog.OfficeId;
                        chcn.RouteId = chcn.fk_Triplog.RouteId;
                    }
                    
                    if (chcn.OfficeId == null || chcn.RouteId == null)
                    {
                        chcn.OfficeId = chcn.fk_Challan?.OfficeID ?? chcn.fk_Triplog?.OfficeId;
                        chcn.RouteId = chcn.fk_Challan?.RouteId ?? chcn.fk_Triplog?.RouteId;
                    }
                    if (chcn.fk_Triplog == null || chcn.fk_Triplog.Id <= 0)
                    {
                        chcn.fk_Triplog = _db.Set<VehicleMovementLog>().Find(chcn.TriplogId);
                    }
                    chcn.ShipmentDate = chcn.fk_Challan?.ChallanDate ?? chcn.fk_Triplog.LoadingDate.GetValueOrDefault(chcn.fk_Triplog.TripStartDate);
                    if (chcn.fk_Triplog != null && chcn.fk_Triplog.Id > 0 && !chcn.ArrivalDate.HasValue && chcn.fk_Triplog.UnloadingDate.HasValue&& chcn.fk_Triplog.FormId!= "1503")
                    {
                        chcn.ArrivalDate = chcn.fk_Triplog.UnloadingDate;
                        if (chcn.ArrivalQty <= 0)
                        {
                            chcn.ArrivalQty = chcn.Qty;
                        }
                    }
                    #region Stock Movement
                    if (chcn.ObjectState == ObjectState.Modified && arrivalDate.CurrentValue == null && arrivalDate.OriginalValue != null && chcn?.fk_Triplog?.UnloadingDate != null)
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                            "Trip has ended so you cannot undo arrival of consignment.");
                    }
                    CNStockLog stockIn = null;
                    CNStockLog stockout = null;
                    CNStockLog transitentry = null;
                    CNStockLog arrivalStock;
                    List<CNStockMMLog> refMMStocks = null;//mm1
                    #region PrePare Stock Out & Transit Stock and Out For Delivery stock


                    #region Prepare Stock Out and Out for delivery
                    if (chcn.tempCNStockMMLogs != null && chcn.tempCNStockMMLogs.Any())
                    {
                        var refmmids = chcn.tempCNStockMMLogs.Select(x => x.RefStockId.GetValueOrDefault()).Distinct().ToList();
                        refMMStocks =
                            cnmmstockRepo.Where(x => x.CNId == chcn.CNId &&
                                            x.InQty > (x.Outwards.Sum(y => (decimal?)y.OutQty) ?? 0) && refmmids.Contains(x.Id) && (x.LogTypeId == 1422 || x.LogTypeId == 1455))
                                .Include(x => x.fk_StockLog)
                                .ToList();
                        if (chcn.tempCNStockMMLogs.Where(x => x.Id == 0).Any(log => refMMStocks == null || refMMStocks.All(x => x.Id != log.RefStockId)))
                        {
                            throw new BusinessException(ErrorCode.GLB106, "One of attached Part is out of stock");
                        }
                    }
                    if ((chcn.CnStockLogs == null || !chcn.CnStockLogs.Any()) && chcn.Id > 0 &&
                        stockRepo.Any(x => x.ChallanCNId == chcn.Id))
                    {
                        stockout =
                            stockRepo.Include(x => x.Outwards.Select(y => y.StockMMLogs))
                                .Include(x => x.RefStock)
                                .Include(x => x.StockMMLogs)
                                .FirstOrDefault(
                                    x =>
                                        x.ChallanCNId == chcn.Id && x.CNId == chcn.CNId &&
                                        (x.LogTypeId == 1423 || x.LogTypeId == 1454 || x.LogTypeId == 1451));
                        if (stockout != null && stockout.Outwards.Any())
                        {
                            transitentry = stockout.Outwards.FirstOrDefault();
                        }
                        if (stockout?.RefStock != null)
                        {
                            stockIn = stockout.RefStock;
                            if (stockIn?.StockMMLogs != null && stockIn.StockMMLogs.Any() && chcn.tempCNStockMMLogs != null && chcn.tempCNStockMMLogs.Any(x => x.Id == 0))
                            {
                                foreach (var log in chcn.tempCNStockMMLogs.Where(x => x.Id <= 0))
                                {
                                    var emml =
                                        stockIn.StockMMLogs.FirstOrDefault(x => x.Id == log.RefStockId.GetValueOrDefault());
                                    if (emml == null) continue;
                                    var r = new CNStockMMLog
                                    {
                                        Id = 0,
                                        ObjectState = ObjectState.Added,
                                        OfficeId = chcn.OfficeId.GetValueOrDefault(),
                                        CNId = emml.CNId,
                                        CNMMId = emml.CNMMId,
                                        ChallanCNId = chcn.Id,
                                        ExessQty = 0,
                                        InQty = 0,
                                        OutQty = emml.InQty,
                                        LogDate = stockout.LogDate,
                                        LogTypeId = stockout.LogTypeId,
                                        MaterialId = emml.MaterialId,
                                        RefStockId = emml.Id,
                                        TriplogId = stockout.TriplogId,
                                        RefStock = emml,
                                        ShortageQty = 0,
                                        StockLogId = stockout.Id,
                                        fk_StockLog = stockout,
                                        fk_ChallanCN = chcn,
                                        Ref1 = log.Ref1,
                                        Ref2 = log.Ref2,
                                        Date2= log.Date2
                                    };
                                    stockout.StockMMLogs.Add(r);
                                    cnmmstockRepo.AddOrUpdate(r);
                                }
                            }
                        }
                        if (stockout != null && stockout.StockMMLogs.Any() && chcn.tempCNStockMMLogs.Any())
                        {
                            foreach (var s in chcn.tempCNStockMMLogs.Where(x=>x.Id>0))
                            {
                                var b = stockout.StockMMLogs.FirstOrDefault(x => x.Id == s.Id);
                                if (b != null)
                                {
                                    b.OutQty = s.OutQty;
                                    b.ObjectState = ObjectState.Modified;
                                    b.Ref2 = s.Ref2;
                                    b.Date2 = s.Date2;
                                    b.LogDate = (chcn.ShipmentDate??chcn.fk_Triplog.LoadingDate)?? b.LogDate;
                                }                                
                            }
                            stockout.LogDate =  (chcn.ShipmentDate ?? chcn.fk_Triplog.LoadingDate) ?? stockout.LogDate;
                            stockout.ObjectState = ObjectState.Modified;
                        }
                    }

                    if (chcn.ObjectState == ObjectState.Added || ((chcn.CnStockLogs == null || !chcn.CnStockLogs.Any()) && stockRepo.Any(x => x.ChallanCNId == chcn.Id)))
                    {
                        //If CN Challan is Existing try to fetch the Out StockLog from Database if it's not in db assign new
                        stockout = chcn.Id > 0
                            ? stockRepo.Include(x => x.Outwards.Select(y => y.StockMMLogs)).Include(x => x.RefStock).Include(x => x.StockMMLogs).FirstOrDefault(
                                x => x.ChallanCNId == chcn.Id && x.CNId == chcn.CNId && (x.LogTypeId == 1423 || x.LogTypeId == 1454 || x.LogTypeId == 1451))
                            : new CNStockLog() { StockMMLogs = new List<CNStockMMLog>() };
                        stockout = stockout ?? new CNStockLog() { StockMMLogs = new List<CNStockMMLog>() };
                        stockout.CNId = chcn.CNId;
                        stockout.InQty = 0;
                        stockout.LogDate = chcn.ShipmentDate.Value;
                        if (chcn.LogTypeId.GetValueOrDefault() == 0)
                        {
                            stockout.LogTypeId = chcn.TriplogId.GetValueOrDefault() == 0
                                ? 1454
                                : (chcn.fk_Triplog.TripTypeId == 1453 ? 1451 : 1423);
                            //TriplogId== null?Loading awaited:Stock Out
                        }
                        else
                        {
                            stockout.LogTypeId = chcn.LogTypeId.GetValueOrDefault();
                        }
                        stockout.ObjectState = stockout.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        stockout.OfficeId = chcn.OfficeId.GetValueOrDefault();
                        stockout.OutQty = chcn.Qty;
                        stockout.ShortageQty = 0;
                        stockout.ChallanCNId = chcn.Id;
                        stockout.fk_ChallanCN = chcn;
                        stockout.TriplogId = chcn.TriplogId;
                        if (stockout.Id == 0 || stockout.StockMMLogs == null || !stockout.StockMMLogs.Any())
                        {
                            //get the StockRef Entry from Database
                            if (refMMStocks != null && refMMStocks.Any())
                            {
                                stockIn = refMMStocks.FirstOrDefault().fk_StockLog;
                            }
                            if (stockIn == null)
                            {
                                stockIn =
                                stockRepo
                                    .Include(x => x.StockMMLogs)
                                    .OrderBy(
                                        x =>
                                            x.LogDate)
                                    .FirstOrDefault(x => x.CNId == chcn.CNId &&
                                            x.InQty > (x.Outwards.Sum(y => (decimal?)y.OutQty) ?? 0) && (x.LogTypeId == 1422 || x.LogTypeId == 1455) && x.Id == chcn.RefStockId);
                            }
                            if (stockIn == null)
                            {
                                throw new BusinessException(ErrorCode.GLB106,
                                    "CN InStock Reference Not Found or CN is Out of Stock");
                            }
                            stockout.RefStockId = stockIn.Id;

                        }
                        else
                        {
                            if (stockout.RefStockId.GetValueOrDefault() <= 0)
                            {
                                throw new BusinessException(ErrorCode.GLB106, "Stock Reference is Required");
                            }
                        }
                        //Create or Update CN Multi material Stock
                        #region CN Multi Material Stock Log out
                        //If CNChallan don't have MaterialStock Logs and Db have the stock entries for this ChallanCN then fetch them from db and assign them to ChllanCN
                        if ((chcn.CnMMLogs == null || !chcn.CnMMLogs.Any(x => (x.LogTypeId == 1423 || x.LogTypeId == 1451))) &&
                            cnmmstockRepo.Any(x => x.CNId == chcn.CNId && x.ChallanCNId == chcn.Id))
                        {
                            chcn.CnMMLogs =
                                cnmmstockRepo.Where(x => x.CNId == chcn.CNId && x.ChallanCNId == chcn.Id && (x.LogTypeId == 1423 || x.LogTypeId == 1451))
                                    .ToList();
                        }
                        //If ChlaanCN and StockOut both don't have MMStockEntries the get the mmstock entries from stock in and assign them to ChllanCN
                        if ((chcn.CnMMLogs == null || !chcn.CnMMLogs.Any(x => (x.LogTypeId == 1423 || x.LogTypeId == 1451))) && stockIn != null &&
                            stockIn.StockMMLogs.Any(x => x.LogTypeId == 1422 || x.LogTypeId == 1455))
                        {
                            chcn.CnMMLogs = new List<CNStockMMLog>();
                            foreach (var x in stockIn.StockMMLogs)
                            {
                                if (x.LogTypeId == 1422 || x.LogTypeId == 1455)
                                {
                                    var log = new CNStockMMLog()
                                    {
                                        Id = 0,
                                        ObjectState = ObjectState.Added,
                                        OfficeId = chcn.OfficeId.GetValueOrDefault(),
                                        CNId = x.CNId,
                                        CNMMId = x.CNMMId,
                                        ChallanCNId = chcn.Id,
                                        ExessQty = 0,
                                        DamagedQty = 0,
                                        InQty = 0,
                                        OutQty = x.InQty,
                                        LogDate = stockout.LogDate,
                                        LogTypeId = stockout.LogTypeId, //TriplogId== null?Loading awaited:Stock Out
                                        MaterialId = x.MaterialId,
                                        RefStockId = x.Id,
                                        TriplogId = stockout.TriplogId,
                                        RefStock = x,
                                        ShortageQty = 0,
                                        StockLogId = stockout.Id,
                                        fk_StockLog = stockout,
                                        fk_ChallanCN = chcn
                                    };
                                    if (chcn.tempCNStockMMLogs != null && chcn.tempCNStockMMLogs.Any())
                                    {
                                        var reflog =
                                            chcn.tempCNStockMMLogs.FirstOrDefault(
                                                y =>
                                                    y.CNId == log.CNId && y.CNMMId == log.CNMMId &&
                                                    y.RefStockId == log.RefStockId);
                                        if (reflog != null)
                                        {
                                            log.OutQty = reflog.OutQty;
                                            log.Ref1 = reflog.Ref1;
                                            log.Ref2 = reflog.Ref2;
                                            log.Date2 = reflog.Date2;
                                            chcn.CnMMLogs.Add(log);
                                        }
                                    }
                                    else
                                    {
                                        chcn.CnMMLogs.Add(log);
                                    }

                                }
                            }
                        }
                        if (stockout.StockMMLogs == null) stockout.StockMMLogs = new List<CNStockMMLog>();
                        stockRepo.AddOrUpdate(stockout);
                        if (chcn.CnMMLogs != null)
                        {
                            foreach (var log in chcn.CnMMLogs.Where(x => (x.LogTypeId == 1423 || x.LogTypeId == 1451)))
                            {
                                var msl = stockout.StockMMLogs.Where(x => (x.LogTypeId == 1423 || x.LogTypeId == 1451)).FirstOrDefault(x => x.Id == log.Id && x.MaterialId == log.MaterialId && x.CNMMId == log.CNMMId);
                                if (msl == null)
                                {
                                    msl = log;
                                    stockout.StockMMLogs.Add(msl);
                                }
                                else
                                {
                                    msl.CNId = log.CNId;
                                    msl.CNMMId = log.CNMMId;
                                    msl.InQty = log.InQty;
                                    msl.MaterialId = log.MaterialId;
                                    msl.StockLogId = log.StockLogId;
                                    msl.ChallanCNId = log.ChallanCNId;
                                    msl.ExessQty = log.ExessQty;
                                    msl.DamagedQty = log.DamagedQty;
                                    msl.LogDate = log.LogDate;
                                    msl.LogTypeId = log.LogTypeId;
                                    msl.OfficeId = log.OfficeId;
                                    msl.TriplogId = log.TriplogId;
                                    msl.OutQty = log.OutQty;
                                    msl.ShortageQty = log.ShortageQty;
                                    msl.RefStockId = log.RefStockId;
                                    msl.ObjectState = msl.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                                    msl.Ref1 = log.Ref1;
                                    msl.Ref2 = log.Ref2;
                                    msl.Date2 = log.Date2;
                                }
                                if (msl.RefStockId.GetValueOrDefault() == 0) throw new BusinessException(ErrorCode.GLB106, "CE:Stock Reference is Required for Material Stock Movement");
                                cnmmstockRepo.AddOrUpdate(msl);
                            }
                            foreach (var log in stockout.StockMMLogs.Where(x => (x.LogTypeId == 1423 || x.LogTypeId == 1451)))
                            {
                                if (!chcn.CnMMLogs.Where(x => (x.LogTypeId == 1423 || x.LogTypeId == 1451)).ToList().Exists(x => x.Equals(log)))
                                {
                                    if (log.Id > 0)
                                    {
                                        log.ObjectState = ObjectState.Deleted;
                                        cnmmstockRepo.Remove(log);
                                    }
                                    stockout.StockMMLogs.Remove(log);
                                }
                            }
                        }
                    }
                    #endregion
                    #endregion

                    #region Prepare Transit Stock
                    if (chcn.TriplogId > 0 && stockout != null && stockout.ObjectState != ObjectState.Unchanged)
                    {
                        if (stockout == null && chcn.Id == 0)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Unable to Trigger Stock Out for one of CN");
                        }
                        //Create or update Transit Entry only when ChallanCN is mapped to TripLog and Out Stock Log Type is 1423 i.e. Stock Out
                        if ((chcn.TriplogId > 0 || (chcn.fk_Triplog != null && chcn.fk_Triplog.ObjectState == ObjectState.Added)) && stockout.LogTypeId == 1423)
                        {
                            if (chcn.fk_Triplog == null && chcn.TriplogId > 0)
                            {
                                chcn.fk_Triplog = _db.Set<VehicleMovementLog>().Find(chcn.TriplogId);
                            }
                            if (transitentry == null)
                            {
                                transitentry = stockRepo.Include(x => x.StockMMLogs).FirstOrDefault(
                                    x => x.ChallanCNId == chcn.Id && x.CNId == chcn.CNId && x.RefStockId == stockout.Id) ??
                                               new CNStockLog() { StockMMLogs = new List<CNStockMMLog>() };
                            }
                            //if (transitentry.Id > 0 && transitentry.LogTypeId == 1424)
                            if (stockout.LogTypeId == 1423)
                            {
                                if (transitentry == null)
                                {
                                    transitentry = new CNStockLog() { StockMMLogs = new List<CNStockMMLog>() };
                                }
                                //Check if user has not done arrival
                                if (transitentry.Id > 0 && stockRepo.Any(x => x.RefStockId == transitentry.Id))
                                {
                                    //Incase you want to delete challan or deattach cn from challan delete all entries that are child of Stock in.
                                    throw new BusinessException(ErrorCode.GLB106, "CE:Cannot Modify Arrived Challan/TripLog");
                                }
                                transitentry.CNId = chcn.CNId;
                                transitentry.InQty = 0;
                                transitentry.LogDate = chcn.fk_Triplog.TripStartDate;
                                transitentry.LogTypeId = 1424;
                                transitentry.ObjectState = transitentry.Id > 0
                                    ? ObjectState.Modified
                                    : ObjectState.Added;
                                transitentry.OutQty = chcn.Qty;
                                transitentry.ShortageQty = 0;
                                transitentry.ChallanCNId = chcn.Id;
                                transitentry.fk_ChallanCN = chcn;
                                transitentry.RefStockId = stockout.Id;
                                transitentry.RefStock = stockout;
                                transitentry.TriplogId = stockout.TriplogId;
                                var targetOfficeId =
                                    _db.Set<RouteMaster>()
                                        .Where(x => x.Id == chcn.RouteId)
                                        .Select(x => x.fk_ToPlace.ControllingOfficeId)
                                        .FirstOrDefault();
                                transitentry.OfficeId = targetOfficeId.GetValueOrDefault() != 0 ? targetOfficeId.Value : stockout.OfficeId;

                                #region CN Multi Material Stock Log Transit

                                //If StockOut and StockOut both don't have MMStockEntries the get the mmstock entries from stock in and assign them to ChllanCN
                                if (stockout.StockMMLogs != null && stockout.StockMMLogs.Any(x => x.LogTypeId == 1423))
                                {
                                    foreach (var log in (stockout.StockMMLogs.Where(x => x.LogTypeId == 1423) ?? new List<CNStockMMLog>()))
                                    {
                                        CNStockMMLog stocklog = transitentry.StockMMLogs?.FirstOrDefault(y => y.RefStockId == log.Id && y.LogTypeId == 1424 && y.MaterialId == log.MaterialId && y.CNMMId == log.CNMMId) ?? new CNStockMMLog();
                                        stocklog.ObjectState = stocklog.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                                        stocklog.OfficeId = transitentry.OfficeId;
                                        stocklog.CNId = log.CNId;
                                        stocklog.CNMMId = log.CNMMId;
                                        stocklog.ChallanCNId = chcn.Id;
                                        stocklog.OutQty = log.OutQty;
                                        stocklog.LogDate = transitentry.LogDate;
                                        stocklog.LogTypeId = transitentry.LogTypeId;
                                        stocklog.MaterialId = log.MaterialId;
                                        stocklog.RefStockId = log.Id;
                                        stocklog.RefStock = log;
                                        stocklog.StockLogId = transitentry.Id;
                                        stocklog.fk_StockLog = transitentry;
                                        stocklog.fk_ChallanCN = chcn;
                                        stocklog.TriplogId = log.TriplogId;
                                        stocklog.Ref1 = log.Ref1;
                                        stocklog.Ref2 = log.Ref2;
                                        stocklog.Date2 = log.Date2;
                                        cnmmstockRepo.AddOrUpdate(stocklog);
                                    }

                                }
                                foreach (var log in (transitentry.StockMMLogs.Where(x => x.LogTypeId == 1424) ?? new List<CNStockMMLog>()))
                                {
                                    if (chcn.CnMMLogs.All(x => x.Id != log.RefStockId))
                                    {
                                        if (log.Id > 0)
                                        {
                                            log.ObjectState = ObjectState.Deleted;
                                            cnmmstockRepo.Remove(log);
                                        }
                                        transitentry.StockMMLogs.Remove(log);
                                    }
                                }
                                #endregion
                                stockRepo.AddOrUpdate(transitentry);
                            }


                        }

                    }
                    #endregion
                    #endregion
                    #region Prepare Arrival/Delivery Of Stock
                    if (chcn.TriplogId.GetValueOrDefault() > 0)
                    {

                        long stockType = 0;
                        if (stockout == null || stockout.LogTypeId <= 0)
                        {
                            stockType =
                                stockRepo.Where(x => x.ChallanCNId == chcn.Id && (x.LogTypeId == 1423 || x.LogTypeId == 1451))
                                    .Select(x => x.LogTypeId)
                                    .FirstOrDefault();
                        }
                        else
                        {
                            stockType = stockout.LogTypeId;
                        }
                        if (stockType == 1423) //StockOut
                        {
                            if (transitentry == null || transitentry.LogTypeId <= 0)
                            {
                                arrivalStock =
                                    stockRepo.Include(x => x.StockMMLogs).FirstOrDefault(
                                        x => x.ChallanCNId == chcn.Id && x.LogTypeId == 1424);
                            }
                            else
                            {
                                arrivalStock = transitentry;
                            }
                        }
                        else//OutFor Delivery
                        {
                            if (stockout == null || stockout.LogTypeId <= 0)
                            {
                                arrivalStock =
                                    stockRepo.Include(x => x.StockMMLogs).FirstOrDefault(
                                        x => x.ChallanCNId == chcn.Id && x.LogTypeId == 1451);
                            }
                            else
                            {
                                arrivalStock = stockout;
                            }
                        }
                        if (chcn.IsDeliveryFailed)
                        {
                            if (chcn.DeliveryFailedDate.GetValueOrDefault(chcn.ShipmentDate.GetValueOrDefault()) ==
                                default(DateTime))
                            {
                                throw new BusinessException(ErrorCode.GLB106, "In case delivery failed, Delivery Failed Date is required.");
                            }
                            if (arrivalStock != null)
                            {
                                //AR Deleted as Arrival Date Has been removed
                                arrivalStock.ObjectState = ObjectState.Modified;
                                arrivalStock.LogTypeId = 1455;
                                arrivalStock.ExessQty = 0;
                                arrivalStock.DamagedQty = 0;
                                arrivalStock.InQty = arrivalStock.OutQty;
                                arrivalStock.ShortageQty = 0;
                                arrivalStock.LogDate = chcn.DeliveryFailedDate.GetValueOrDefault(chcn.ShipmentDate.GetValueOrDefault());
                                if (arrivalStock.StockMMLogs != null && arrivalStock.StockMMLogs.Any(x => x.LogTypeId == 1422 || x.LogTypeId == 1425))
                                {
                                    arrivalStock.StockMMLogs.Where(x => x.LogTypeId == 1422 || x.LogTypeId == 1425).ToList().ForEach(x =>
                                    {
                                        x.LogTypeId = arrivalStock.LogTypeId;
                                        x.ObjectState = ObjectState.Modified;
                                        x.ExessQty = 0;
                                        x.DamagedQty = 0;
                                        x.InQty = x.OutQty;
                                        x.ShortageQty = 0;//Working
                                        x.LogDate = chcn.ShipmentDate.Value;
                                    });
                                }
                                stockRepo.AddOrUpdate(arrivalStock);
                            }
                            chcn.ArrivalQty = chcn.Qty;
                            chcn.Excess = 0;
                            chcn.Short = 0;
                        }

                        bool createUpdateArrival = false;
                        if ((arrivalDate.CurrentValue != null || (chcn.ObjectState != ObjectState.Added && arrivalDate.OriginalValue != null)) && !chcn.IsDeliveryFailed&& !stockRepo.Any(x => x.ChallanCNId == chcn.Id && (x.LogTypeId == 1425 || x.LogTypeId == 1422)))
                        {
                            if (arrivalDate.CurrentValue != null)
                            {

                                if (arrivalStock == null || stockRepo.Any(x => x.ChallanCNId == chcn.Id && (x.LogTypeId == 1425 || x.LogTypeId == 1422)))
                                {
                                    throw new BusinessException(ErrorCode.GLB106, "Consignment should be in Transit/Out For Delivery stage, to do arrival or to acknowledge delivery");
                                    //createUpdateArrival = false;
                                }
                            }
                            if (chcn.ObjectState != ObjectState.Added && arrivalDate.CurrentValue != arrivalDate.OriginalValue)
                            {
                                if (arrivalDate.CurrentValue == null && arrivalDate.OriginalValue != null)
                                {
                                    if (arrivalStock != null)
                                    {
                                        //AR Deleted as Arrival Date Has been removed
                                        arrivalStock.ObjectState = ObjectState.Modified;
                                        arrivalStock.LogTypeId = arrivalStock.LogTypeId == 1422 ? 1424 : 1451;
                                        arrivalStock.ExessQty = 0;
                                        arrivalStock.InQty = 0;
                                        arrivalStock.ShortageQty = 0;
                                        arrivalStock.LogDate = chcn.ShipmentDate.Value;
                                        if (arrivalStock.StockMMLogs != null && arrivalStock.StockMMLogs.Any(x => x.LogTypeId == 1422 || x.LogTypeId == 1425))
                                        {
                                            arrivalStock.StockMMLogs.Where(x => x.LogTypeId == 1422 || x.LogTypeId == 1425).ToList().ForEach(x =>
                                            {
                                                x.LogTypeId = arrivalStock.LogTypeId;
                                                x.ObjectState = ObjectState.Modified;
                                                x.ExessQty = 0;
                                                x.DamagedQty = 0;
                                                x.InQty = 0;
                                                x.ShortageQty = 0;//Working
                                                x.LogDate = chcn.ShipmentDate.Value;
                                            });
                                        }
                                        stockRepo.AddOrUpdate(arrivalStock);
                                    }
                                    chcn.ArrivalQty = 0;
                                    chcn.Excess = 0;
                                    chcn.Short = 0;
                                    createUpdateArrival = false;
                                }
                                else if (arrivalDate.CurrentValue != null && (chcn.ObjectState == ObjectState.Added || arrivalDate.OriginalValue == null))
                                {
                                    //Create Arrival
                                    createUpdateArrival = true;
                                }
                                else
                                {
                                    //Validate Arrival
                                    if (chcn.ArrivalDate < transitentry.LogDate)
                                    {
                                        throw new BusinessException(ErrorCode.GLB106, $"Consignment Arrival cannot be done before it's Shipment Date {transitentry.LogDate.ToString("yy-MMM-dd ddd h:mm:ss tt")}");
                                    }
                                    if (chcn.ArrivalQty + chcn.Short > chcn.Qty)
                                    {
                                        throw new BusinessException(ErrorCode.GLB106, $"Consignment Arrival Qty cannot be done greater than {chcn.ArrivalQty}");
                                    }
                                    createUpdateArrival = true;
                                }
                            }
                        }
                        if (arrivalStock != null && arrivalStock.ObjectState == ObjectState.Added && chcn.ArrivalDate != null)
                        {
                            if (transitentry!=null&&chcn.ArrivalDate < transitentry.LogDate)
                            {
                                throw new BusinessException(ErrorCode.GLB106, $"Consignment Arrival cannot be done before it's Shipment Date {transitentry.LogDate.ToString("yy-MMM-dd ddd h:mm:ss tt")}");
                            }
                            if (chcn.ArrivalQty + chcn.Short > chcn.Qty)
                            {
                                throw new BusinessException(ErrorCode.GLB106, $"Consignment Arrival Qty cannot be done greater than {chcn.ArrivalQty}");
                            }
                            createUpdateArrival = true;
                        }
                        if (createUpdateArrival)
                        {
                            arrivalStock.ChallanCNId = chcn.Id;
                            arrivalStock.LogDate = chcn.ArrivalDate.Value;
                            arrivalStock.LogTypeId = chcn.fk_Triplog.TripTypeId == 1453 ? 1425 : (chcn.DeliveryTypeId== 1472?1425:1422);
                            arrivalStock.CNId = chcn.CNId;
                            arrivalStock.InQty = chcn.ArrivalQty;
                            arrivalStock.ShortageQty = chcn.Short;
                            arrivalStock.ExessQty = chcn.Excess;
                            arrivalStock.DamagedQty = chcn.Damaged;
                            arrivalStock.ObjectState = arrivalStock.Id > 0
                                ? ObjectState.Modified
                                : ObjectState.Added;
                            arrivalStock.StockMMLogs?.Where(x => x.LogTypeId == 1424 || x.LogTypeId == 1451).ToList().ForEach(x =>
                            {
                                x.LogTypeId = arrivalStock.LogTypeId;
                                x.ObjectState = ObjectState.Modified;
                                x.InQty = x.InQty > 0 ? x.InQty : x.OutQty;
                                x.LogDate = arrivalStock.LogDate;
                                cnmmstockRepo.AddOrUpdate(x);
                            });
                            stockRepo.AddOrUpdate(arrivalStock);
                        }

                    }
                    #endregion
                    #endregion
                    //foreach (var item in chcn.CnStockLogs)
                    //{
                    //    AddStatusMap(item);
                    //}
                    break;
                case ObjectState.Deleted:
                    //Try to fetch challan if challanid has value and officeid or routeid don't have value
                    if (chcn.fk_Triplog == null && chcn.fk_Challan == null && chcn.ChallanId.HasValue && !chcn.TriplogId.HasValue)
                    {
                        chcn.fk_Challan = _db.Set<ChallanMaster>().Find(chcn.ChallanId);
                        if (chcn.fk_Challan == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "CE:Invalid Challan");
                        }
                    }
                    if (chcn.fk_Triplog == null && chcn.fk_Challan == null && (chcn.ChallanId.HasValue || chcn.TriplogId.HasValue))
                    {
                        chcn.fk_Triplog = _db.Set<VehicleMovementLog>().Find(chcn.TriplogId);
                        if (chcn.fk_Triplog == null)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "CE:Invalid TripLog");
                        }
                    }
                    //if (stockRepo.Any(x => x.ChallanCNId == chcn.Id && (x.LogTypeId == 1425 || x.LogTypeId == 1422)))
                    //{
                    //    throw new BusinessException(ErrorCode.GLB106, "One of Attached Consignment has been delivered or has been arrived at it's destination. So cannot remove it from system.");
                    //}
                    if (chcn.CnStockLogs == null || !chcn.CnStockLogs.Any())
                    {
                        _db.Database.ExecuteSqlCommand($"EXEC Proc_DeleteOutStockLog @ChallanCnId={chcn.Id}");
                    }
                    if (chcn.fk_Triplog != null)
                    {
                        chcn.fk_Triplog.LoadingQty = chcn.fk_Triplog.LoadingQty - chcn.Qty;
                        if (chcn.fk_Triplog.LoadingQty < 0)
                            chcn.fk_Triplog.LoadingQty = 0;
                        chcn.fk_Triplog.LoadedWeight = chcn.fk_Triplog.LoadedWeight - chcn.Weight;
                        if (chcn.fk_Triplog.LoadedWeight < 0)
                            chcn.fk_Triplog.LoadedWeight = 0;
                        chcn.fk_Triplog.ObjectState = ObjectState.Modified;
                        _db.Set<VehicleMovementLog>().AddOrUpdate(chcn.fk_Triplog);
                    }
                    if (chcn.fk_Challan != null)
                    {
                        chcn.fk_Challan.Quantity = chcn.fk_Challan.Quantity - chcn.Qty;
                        if (chcn.fk_Challan.Quantity < 0)
                            chcn.fk_Challan.Quantity = 0;
                        chcn.fk_Challan.Weight = chcn.fk_Challan.Weight - chcn.Weight;
                        if (chcn.fk_Challan.Weight < 0)
                            chcn.fk_Challan.Weight = 0;
                        chcn.fk_Challan.ObjectState = ObjectState.Modified;
                        _db.Set<ChallanMaster>().AddOrUpdate(chcn.fk_Challan);
                    }
                    if (chcn.CnStockLogs != null)
                    {
                        foreach (var item in chcn.CnStockLogs)
                        {
                            RemoveStatusMap(item);
                        }
                    }
                    break;
            }
        }
        
        
        private void RemoveStatusMap(CNStockLog entity)
        {
            if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var statusid = GetStatusId(entity.LogTypeId);
            if (statusid == 0) return;
            
            var cndts = repo.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.CNId == entity.CNId && x.StatusId == statusid && x.StockLogId == entity.Id);
            if (cndts != null)
            {
                cndts.ObjectState = ObjectState.Deleted;
                new CNDTSStatusCoreLogic().Bind(_db).Execute(_db.Entry(cndts));
            }

        }
        private long GetStatusId(long stockLogTypeId)
        {
            return _db.Set<DTSStatus>().Where(x => x.DateId == stockLogTypeId).FromCacheFirstOrDefault()?.Id ?? 0;
        }

        public void PostLogic(long challanId)
        {

        }

    }
}
