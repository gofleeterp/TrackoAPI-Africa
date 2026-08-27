using Repository.Pattern.Core.Repositories;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;

namespace TrackoAPI.Repository
{
    public static class VehicleMovementLogRepository
    {
        public static async Task AttachCNToTripLogAsync(this IRepository<VehicleMovementLog> repository, long? newtripLogId, long? oldtriplogid,long cnid, CNMaster cn, bool saveChanges = false)
        {
            if (oldtriplogid.GetValueOrDefault(0)==0 && newtripLogId.GetValueOrDefault(0) == 0) return;
            else if(oldtriplogid.GetValueOrDefault(0)==0 && newtripLogId.GetValueOrDefault(0) > 0)
            {
                //Create new Entries
                await repository.CreateUpdateRelatedTransaction(newtripLogId, cnid, cn, saveChanges);
            }
            else if(oldtriplogid.GetValueOrDefault(0)>0 && newtripLogId.GetValueOrDefault(0) == 0)
            {
                //Delete Old Entries
                await repository.DeleteRelatedTransaction(oldtriplogid ??0, cnid, saveChanges);
            }
            else if (oldtriplogid.GetValueOrDefault(0) ==newtripLogId.GetValueOrDefault(0))
            {
                //update
                await repository.CreateUpdateRelatedTransaction(newtripLogId, cnid, cn, saveChanges);
            }
            else if (oldtriplogid.GetValueOrDefault(0) != newtripLogId.GetValueOrDefault(0))
            {
                //Delete Old and Create New One
                await repository.DeleteRelatedTransaction(oldtriplogid ?? 0, cnid, saveChanges);
                await repository.CreateUpdateRelatedTransaction(newtripLogId, cnid, cn, saveChanges);
                
            }

        }
        private static async Task CreateUpdateRelatedTransaction(this IRepository<VehicleMovementLog> repository, long? triplogid, long cnid,CNMaster cnmaster=null, bool saveChanges = false)
        {
            var chcnRepo = repository.GetRepository<CnChallan>();
            var stockRepo = repository.GetRepository<CNStockLog>();
            //var stockMMLogRepo = repository.GetRepository<CNStockMMLog>();
            //var cndtsRepo = repository.GetRepository<CNDTSStatusLog>();
            //var cnstatusRepo = repository.GetRepository<CnStatusLog>();
            if (cnmaster == null) {
                cnmaster = await repository.GetRepository<CNMaster>().Queryable().FirstOrDefaultAsync(x => x.Id == cnid);
                if (cnmaster == null)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Provided Reference for CN is Invalid");
                }
            }
            //var triplog =await repository.GetRepository<VehicleMovementLog>().Queryable().FirstOrDefaultAsync(x => x.Id == triplogid);
            //if (triplog == null)
            //{
            //    throw new BusinessException(ErrorCode.GLB106, $"Provided Reference  for CN {cnmaster?.CNNo} of Triplog is Invalid");
            //}
            #region CN Challan  
            var stockId =
                    stockRepo.Queryable()
                        .Where(x => x.CNId == cnmaster.Id && x.RefStockId == null && x.OfficeId == cnmaster.LoadingOfficeId).Select(x => x.Id).FirstOrDefault();
            var chcn = await chcnRepo.Queryable().Include(x => x.CnStockLogs.Select(y => y.StockMMLogs)).FirstOrDefaultAsync(x => x.CNId == cnid && x.TriplogId == triplogid)??new CnChallan();
            if (cnmaster.TLLoadQty > cnmaster.ActualQty) cnmaster.TLLoadQty = cnmaster.ActualQty;
            chcn.Qty = cnmaster.TLLoadQty;
            chcn.Revenue = ((cnmaster.CNSubTotalII) / cnmaster.ActualQty) * cnmaster.TLLoadQty;
            chcn.Weight = (cnmaster.ActualWeight / cnmaster.ActualQty) * cnmaster.TLLoadQty;
            chcn.TriplogId = triplogid;
            chcn.RouteId = cnmaster.ActualRouteId ?? cnmaster.ChargedRouteId;
            chcn.DeliveryTypeId = cnmaster.DeliveryTypeId;
            chcn.OfficeId = cnmaster.LoadingOfficeId;
            chcn.CNId = cnmaster.Id;
            chcn.fk_CNMaster = cnmaster;
            chcn.ViewId = cnmaster.ViewId;
            chcn.RefStockId = stockId;
            if (chcn.Id > 0)
            {
                chcn.ObjectState = ObjectState.Modified;
            }
            else
            {
                chcn.ObjectState = ObjectState.Added;
                chcnRepo.Insert(chcn);

            }
            #endregion
            if (saveChanges)
            {
                await repository.UOW.SaveChangesAsync();
            }
        }
        private static async Task DeleteRelatedTransaction(this IRepository<VehicleMovementLog> repository, long triplogid,long cnid,bool saveChanges =false)
        {
            var chcnRepo = repository.GetRepository<CnChallan>();
            //var stockRepo = repository.GetRepository<CNStockLog>();
            //var stockMMLogRepo = repository.GetRepository<CNStockMMLog>();
            //var cndtsRepo = repository.GetRepository<CNDTSStatusLog>();
            var cnstatusRepo = repository.GetRepository<CnStatusLog>();

            var cnstatuses =await cnstatusRepo.Queryable().Where(x => x.CNId == cnid && x.DocTypeId == 1811 && x.DocId == triplogid).ToListAsync();
            if (cnstatuses != null)
            {
                cnstatuses.ForEach(x =>
                {
                    x.ObjectState = ObjectState.Deleted;
                });
            }
            
            var chcn =await chcnRepo.Queryable().Include(x=>x.CnStockLogs.Select(y=>y.StockMMLogs)).FirstOrDefaultAsync(x => x.CNId == cnid && x.TriplogId == triplogid);
            if(chcn!=null)
            {
                chcn.ObjectState = ObjectState.Deleted;
            }
            //var stocklogs =await stockRepo.Queryable().Where(x => x.CNId == cnid && x.TriplogId == triplogid).ToListAsync();
            //if (stocklogs != null)
            //{
            //    stocklogs.ForEach(x =>
            //    {
            //        x.ObjectState = ObjectState.Deleted;
            //    });
            //}
            //var stockmmlogs = await stockMMLogRepo.Queryable().Where(x => x.CNId == cnid && x.TriplogId == triplogid).ToListAsync();
            //if (stockmmlogs != null)
            //{
            //    stockmmlogs.ForEach(x =>
            //    {
            //        x.ObjectState = ObjectState.Deleted;
            //    });
            //}
            //var stockids = stocklogs.Select(x =>(long?) x.Id).ToArray();
            //var cndtsstatuses = await cndtsRepo.Queryable().Where(x => x.CNId == cnid && stockids.Contains(x.StockLogId)).ToListAsync();
            //if (cndtsstatuses != null)
            //{
            //    cndtsstatuses.ForEach(x =>
            //    {
            //        x.ObjectState = ObjectState.Deleted;
            //    });
            //}
            if (saveChanges)
            {
                await repository.UOW.SaveChangesAsync();
            }
        }
        public static void AttachCNToTripLog(this IRepository<VehicleMovementLog> repository, long? newtripLogId, long? oldtriplogid, CNMaster cn)
        {
            try
            {
                var stockRepo = repository.GetRepository<CNStockLog>();
                var oldchallancn = oldtriplogid>0?
                    repository
                        .GetRepository<CnChallan>()
                        .Queryable()
                        .Include(x => x.CnStockLogs.Select(y => y.StockMMLogs))
                        .FirstOrDefault(x => x.TriplogId == oldtriplogid && x.CNId == cn.Id):null;
                var stockmm = repository.GetRepository<CNStockMMLog>();
                if (newtripLogId.GetValueOrDefault() <= 0)
                {
                    if (oldchallancn != null) return;
                    oldchallancn.ObjectState = ObjectState.Deleted;
                    oldchallancn.CnStockLogs?.ForEach(x =>
                    {
                        x.ObjectState = ObjectState.Deleted;
                        x.StatusLogs?.ForEach(y =>
                        {
                            y.ObjectState = ObjectState.Deleted;
                            repository.GetRepository<CnStatusLog>().Delete(y);
                        });
                        x.StockMMLogs?.ForEach(y =>
                        {
                            y.ObjectState = ObjectState.Deleted;
                            stockmm.Delete(y);
                        });
                        stockRepo.Delete(x);
                    });
                    repository.GetRepository<CnChallan>().Delete(oldchallancn);
                    return;
                }
                //decimal oldRevenue = 0, oldqty = 0, oldweight = 0, oldmktfrht = 0;
                if (oldchallancn == null)
                {
                    oldchallancn = new CnChallan() { ObjectState = ObjectState.Added };
                    repository.GetRepository<CnChallan>().Insert(oldchallancn);
                }
                else
                {
                    //oldweight = challancn.Weight;
                    //oldRevenue = challancn.Revenue;
                    //oldqty = challancn.Qty;
                    //oldmktfrht = challancn.MarketFreight;

                    oldchallancn.ObjectState = ObjectState.Modified;
                }

                #region GetStock

                var stockId =
                    stockRepo.Queryable()
                        .Where(x => x.CNId == cn.Id && x.RefStockId == null && x.OfficeId == cn.LoadingOfficeId).Select(x => x.Id).FirstOrDefault();
                oldchallancn.Qty = cn.TLLoadQty;
                oldchallancn.Revenue = ((cn.CNSubTotalII) / cn.ActualQty) * cn.TLLoadQty;
                oldchallancn.Weight = (cn.ActualWeight / cn.ActualQty) * cn.TLLoadQty;
                oldchallancn.TriplogId = newtripLogId;
                oldchallancn.RouteId = cn.ActualRouteId ?? cn.ChargedRouteId;
                oldchallancn.DeliveryTypeId = cn.DeliveryTypeId;
                oldchallancn.OfficeId = cn.LoadingOfficeId;
                oldchallancn.CNId = cn.Id;
                oldchallancn.fk_CNMaster = cn;
                if (newtripLogId != oldtriplogid)
                {
                    oldchallancn.CnStockLogs?.ForEach(x =>
                    {
                        x.TriplogId = newtripLogId;
                        x.ObjectState = ObjectState.Modified;
                        x.StockMMLogs?.ForEach(y =>
                        {
                            y.TriplogId = newtripLogId;
                            y.ObjectState = ObjectState.Modified;
                        });
                    });
                }
                oldchallancn.RefStockId = stockId;

                #endregion GetStock
            }
            catch (DivideByZeroException ex)
            {
                throw new BusinessException(ErrorCode.GLB106, "One more of this CN/LR values are zero ActualWeight,(CNSubTotalII),ActualQty");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static IQueryable<VehicleMovementLog> GetAllVehicleMovementLogList(this IRepository<VehicleMovementLog> repository,
                     long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);
        }
    }
}