using AutoMapper;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global.DTS;

namespace TrackoApi.Service.TMS
{
    public interface IConsignmentsService : IService<CNMaster>
    {
        
    }
    public class ConsignmentsService: Service<CNMaster>, IConsignmentsService
    {
        private IRepositoryAsync<CNMaster> _repo;
        private readonly IMapper _mapper;

        public ConsignmentsService(IRepositoryAsync<CNMaster> repository,IMapper mapper) : base(repository)
        {
            _repo = repository;
            _mapper = mapper;
        }

        public override async Task UpdateAsync(CNMaster cnMaster)
        {
            var stockRepo = _repo.GetRepository<CNStockLog>();
            var chcnRepo = _repo.GetRepository<CnChallan>();
            var mmRepo = _repo.GetRepository<CNMultiMaterial>();
            var mmstockRepo = _repo.GetRepository<CNStockMMLog>();
            //var config =
            //        new MapperConfiguration(cfg => cfg.CreateMap<vwCNMultiMaterial, CNMultiMaterial>())
            //            .CreateMapper();
            var isdispatched =await chcnRepo.Queryable().AnyAsync(x => x.CNId == cnMaster.Id &&x.ViewId!=cnMaster.ViewId);
            if (!isdispatched||cnMaster.DeliveryTypeId.GetValueOrDefault(0)== 1472/*Direct Delivery*/)
            {
                foreach (var material in cnMaster.MultiMaterialsView)
                {

                    var i = cnMaster.Materials.FirstOrDefault(x => x.Id == material.Id)??new CNMultiMaterial();
                    if (i.Id > 0)
                    {
                        if (material.IsDeleted)
                        {
                            i.ObjectState = ObjectState.Deleted;
                            cnMaster.StockLogs?.ForEach(x =>
                            {
                                x.StockMMLogs?.Where(z => z.CNMMId == i.Id).ToList().ForEach(y =>
                                {
                                    y.ObjectState = ObjectState.Deleted;
                                });
                                x.InQty =
                                    x.StockMMLogs?.Where(z => z.ObjectState != ObjectState.Deleted).Sum(z => z.InQty) ?? 0;
                                x.OutQty =
                                    x.StockMMLogs?.Where(z => z.ObjectState != ObjectState.Deleted).Sum(z => z.OutQty) ?? 0;
                            });
                            mmRepo.Delete(i);
                            continue;
                        }
                        var o = material; //config.Map<CNMultiMaterial>(material);
                        i.InvoiceNo = o.InvoiceNo;
                        i.InvoiceDate = o.InvoiceDate;
                        i.InvoiceValue = o.InvoiceValue;
                        i.ServiceTaxRate = o.ServiceTaxRate;
                        i.ServiceTaxAmount = o.ServiceTaxAmount;
                        i.ExciseRate = o.ExciseRate;
                        i.ExciseAmount = o.ExciseAmount;
                        i.InvoiceNetValue = o.InvoiceNetValue;
                        i.MaterialId = o.MaterialId;
                        i.ActualQty = o.ActualQty;
                        i.ActualQtyUnitId = o.ActualQtyUnitId;
                        i.ActualWeight = o.ActualWeight;
                        i.ActualWeightUnitId = o.ActualWeightUnitId;
                        i.ChargeWeight = o.ChargeWeight;
                        i.ChargeWeightUnitId = o.ChargeWeightUnitId;
                        i.ChargeQty = o.ChargeQty;
                        i.ChargeQtyUnitId = o.ChargeQtyUnitId;
                        i.TotalPackage = o.TotalPackage;
                        i.PkgUnitId = o.PkgUnitId;
                        i.Length = o.Length;
                        i.Height = o.Height;
                        i.Breadth = o.Breadth;
                        i.VolumeUnitId = o.VolumeUnitId;
                        i.CFT = o.CFT;
                        i.Rate = o.Rate;
                        i.Freight = o.Freight;
                        i.Remark = o.Remark;
                        i.Ref1Id = o.Ref1Id;
                        i.Ref1 = o.Ref1;
                        i.Ref2 = o.Ref2;
                        i.Ref3 = o.Ref3;
                        i.EWayBillMM = o.EWayBillMM;
                        i.eWayBillValidity = o.eWayBillValidity;
                        i.InvoiceRate = o.InvoiceRate;
                        i.ObjectState = ObjectState.Modified;
                        mmRepo.Update(i);
                    }
                    else
                    {
                        i = _mapper.Map<CNMultiMaterial>(material);
                        i.ObjectState = ObjectState.Added;
                        cnMaster.Materials.Add(i);
                        i.fk_CN = cnMaster;
                        i.CnId = cnMaster.Id;
                        mmRepo.Insert(i);
                    }
                    if (i.ActualQty <= 0)
                    {
                        i.ActualQty = i.ChargeQty > 0 ? i.ChargeQty : 1;
                    }
                    if (i.ChargeQty <= 0)
                    {
                        i.ChargeQty = i.ActualQty > 0 ? i.ActualQty : 1;
                    }
                    if (i.ActualWeight <= 0)
                    {
                        i.ActualWeight = i.ChargeWeight > 0 ? i.ChargeWeight : 1;
                    }
                    if (i.ChargeWeight <= 0)
                    {
                        i.ChargeWeight = i.ActualWeight > 0 ? i.ActualWeight : 1;
                    }
                }
                

                if (cnMaster.Materials != null && cnMaster.Materials.Any())
                {
                    if (GetConfigValue<int>("CNQtyEqualsMMQty") == 1)
                    {
                        cnMaster.ActualQty = cnMaster.Materials?.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => (decimal?)x.ActualQty) ?? cnMaster.ActualQty;
                        cnMaster.ChargedQty =
                            cnMaster.Materials?.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => (decimal?)x.ChargeQty) ?? cnMaster.ChargedQty;
                    }
                    if (GetConfigValue<int>("CNWeightEqualsMMWeight") == 1)
                    {
                        cnMaster.ActualWeight =
                            cnMaster.Materials?.Where(x => x.ObjectState != ObjectState.Deleted)
                                .Sum(x => (decimal?)x.ActualWeight) ?? cnMaster.ActualWeight;
                        cnMaster.ChargedWeight =
                            cnMaster.Materials?.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => (decimal?)x.ChargeWeight) ?? cnMaster.ChargedWeight;
                    }
                    if (GetConfigValue<int>("CNInvoiceValueEqualsMMInvoiceValue") == 1)
                    {
                        cnMaster.ValueofGoods =
                            cnMaster.Materials?.Where(x => x.ObjectState != ObjectState.Deleted)
                                .Sum(x => (decimal?)x.InvoiceNetValue) ?? cnMaster.ValueofGoods;
                    }
                }

                if (cnMaster.ActualQty <= 0)
                {
                    cnMaster.ActualQty = cnMaster.ChargedQty > 0 ? cnMaster.ChargedQty : 1;
                }
                if (cnMaster.ChargedQty <= 0 && !cnMaster.IsZeroFreightCN)
                {
                    cnMaster.ChargedQty = cnMaster.ActualQty > 0 ? cnMaster.ActualQty : 1;
                }
                if (cnMaster.ActualWeight <= 0)
                {
                    cnMaster.ActualWeight = cnMaster.ChargedWeight > 0 ? cnMaster.ChargedWeight : 1;
                }
                if (cnMaster.ChargedWeight <= 0 && !cnMaster.IsZeroFreightCN)
                {
                    cnMaster.ChargedWeight = cnMaster.ActualWeight > 0 ? cnMaster.ActualWeight : 1;
                }
                var stock =await stockRepo.Queryable().FirstOrDefaultAsync(x => x.CNId == cnMaster.Id && x.LogTypeId == 1422 && x.RefStockId == null);
                    //cnMaster.StockLogs?.FirstOrDefault(
                    //    x => x.CNId == cnMaster.Id && x.LogTypeId == 1422 && x.RefStockId == null);
                
                if (stock == null)
                {
                    stock = new CNStockLog()
                    {
                        CNId = cnMaster.Id,
                        InQty = cnMaster.ActualQty,
                        LogDate = cnMaster.CNDate,
                        LogTypeId = 1422,
                        ObjectState = ObjectState.Added,
                        OfficeId = cnMaster.LoadingOfficeId.GetValueOrDefault(),
                        OutQty = 0,
                        ShortageQty = 0,
                        fk_CNMaster = cnMaster                        
                    };
                    stockRepo.Insert(stock);
                }
                else
                {
                    stock.InQty = cnMaster.ActualQty;
                    stock.OfficeId = cnMaster.LoadingOfficeId.GetValueOrDefault();
                    stock.LogDate = cnMaster.CNDate;
                    stock.ObjectState = ObjectState.Modified;
                    stockRepo.Update(stock);
                }
                await _repo.UOW.SaveChangesAsync();
                var createStockMMForDirectDelivery = (GetConfigValue("CreateStockMMLogForDD",1)==1&&cnMaster.DeliveryTypeId== 1472/*Direct Delivery*/)|| cnMaster.DeliveryTypeId != 1472;
                if (cnMaster.Materials!=null&&cnMaster.Materials.Any()&& createStockMMForDirectDelivery)
                {
                    foreach (var material in cnMaster.Materials)
                    {
                        var mmst = stock.StockMMLogs.FirstOrDefault(x => x.CNMMId == material.Id&&x.CNMMId==material.Id) ?? new CNStockMMLog();
                        if (mmst.Id == 0 || (mmst.Id > 0 ))//Only update in case it's new or still in stock
                        {
                            mmst.CNId = material.CnId;
                            mmst.CNMMId = material.Id;
                            mmst.MaterialId = material.MaterialId.GetValueOrDefault();
                            mmst.OfficeId = stock.OfficeId;
                            mmst.OutQty = 0;
                            mmst.RefStockId = null;
                            mmst.ShortageQty = 0;
                            mmst.ChallanCNId = null;
                            mmst.ExessQty = 0;
                            mmst.InQty = material.ActualQty;
                            mmst.LogDate = stock.LogDate;
                            mmst.LogTypeId = stock.LogTypeId;
                            mmst.StockLogId = stock.Id;
                            mmst.ObjectState = mmst.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                            if (mmst.Id > 0)
                            {
                                mmstockRepo.Update(mmst);
                            }
                            else
                            {
                                mmstockRepo.Insert(mmst);
                                stock.StockMMLogs.Add(mmst);
                            }
                        }

                    }
                }
                //if (GetConfigValue<long>("CNStatusTemplateId") > 0)
                //{
                //    var dtslogrepo = _repo.GetRepository<CNDTSStatusLog>();
                //    var dtsRepo = _repo.GetRepository<DTSStatus>();
                //    var dtsstatusId =await dtsRepo.Queryable().Where(x => x.DateId == 1422).Select(x => x.Id).FirstOrDefaultAsync();
                //    if (dtsstatusId > 0)
                //    {
                //        var status = await dtslogrepo.Queryable().FirstOrDefaultAsync(x => x.CNId == cnMaster.Id && x.StatusId == 43);
                //        if (status == null)
                //        {
                //            status = new CNDTSStatusLog
                //            {
                //                CNId = cnMaster.Id,
                //                fk_CN = cnMaster,
                //                IsAuto = true,
                //                StartDate = cnMaster.CNDate,
                //                OfficeId1 = cnMaster.LoadingOfficeId.GetValueOrDefault(),
                //                ObjectState = ObjectState.Added,
                //                StatusId = dtsstatusId,
                //                Qty = cnMaster.ActualQty > 0 ? cnMaster.ActualQty : cnMaster.ChargedQty
                //            };
                //            dtslogrepo.Insert(status);
                //        }
                //        else if (status.NextLogId == null)
                //        {
                //            status.StartDate = cnMaster.CNDate;
                //            status.OfficeId1 = cnMaster.LoadingOfficeId.GetValueOrDefault();
                //            status.ObjectState = ObjectState.Modified;
                //        }
                //        var log = new CnStatusLog()
                //        {
                //            CNId = cnMaster.Id,
                //            DocDate = cnMaster.CNDate,
                //            DocId = cnMaster.Id,
                //            DocNumber = cnMaster.CNNo,
                //            DocTypeId = 1426,

                //            ObjectState = ObjectState.Added,
                //            OfficeId = cnMaster.LoadingOfficeId.GetValueOrDefault(),
                //            PartyId = cnMaster.BillingPartyId.GetValueOrDefault(),
                //            TransactionRemark = "New CN Created",
                //            fk_CNMaster = cnMaster
                //        };
                //        _repo.GetRepository<CnStatusLog>().Insert(log);
                //    }
                //}
                if (GetConfigValue<int>("CNQtyEqualsMMQty") == 1)
                {
                    var qty = cnMaster.Materials?.Sum(x => (decimal?) x.ActualQty).GetValueOrDefault();
                    stock.InQty = qty == 0? cnMaster.ActualQty: qty.GetValueOrDefault();
                    if (stock.StockMMLogs!=null&&stock.InQty != stock.StockMMLogs.Sum(x => x.InQty))
                    {
                        foreach (var mms in stock.StockMMLogs)
                        {
                            var i = cnMaster.Materials.Find(x => x.Id == mms.CNMMId);
                            mms.InQty = i.ActualQty;
                        }
                    }
                    stock.ObjectState=ObjectState.Modified;
                }
               var cnchallan=await _repo.GetRepository<CnChallan>()
                    .Queryable()
                    .FirstOrDefaultAsync(x => x.CNId == cnMaster.Id && x.TriplogId == cnMaster.TripLogId);
                
                if (cnchallan != null)
                {
                    foreach (var log in cnMaster.StockLogs.Where(x => x.ChallanCNId==cnchallan.Id))
                    {
                        log.OutQty = stock.InQty;
                        log.ObjectState = ObjectState.Modified;
                    }
                    cnchallan.Qty = stock.InQty;
                    cnchallan.Weight = cnMaster.ActualWeight;                    
                    cnchallan.Revenue = cnMaster.CNTotalFreight;
                    cnchallan.ObjectState = ObjectState.Modified;
                }
                else
                {
                    cnMaster.StockLogs.RemoveAll(x => x.RefStockId > 0);
                }
                
            }
            base.Update(cnMaster);
        }

        public override CNMaster Insert(CNMaster entity)
        {
            var stock=new CNStockLog()
            {
                CNId = entity.Id,
                InQty = entity.ActualQty,
                LogDate = entity.CNDate,
                LogTypeId = 1422,
                ObjectState = ObjectState.Added,
                OfficeId = entity.LoadingOfficeId.GetValueOrDefault(),
                OutQty = 0,
                ShortageQty = 0,
                fk_CNMaster = entity
            };
            var createStockMMForDirectDelivery = (GetConfigValue("CreateStockMMLogForDD", 1) == 1 && entity.DeliveryTypeId == 1472/*Direct Delivery*/) || entity.DeliveryTypeId != 1472;
            if (entity.Materials != null&& entity.Materials.Any()&& createStockMMForDirectDelivery)
            {
                foreach (var material in entity.Materials.Where(x=>x.MaterialId>0))
                {
                    var cnmmstock = new CNStockMMLog
                    {
                        CNId = entity.Id,
                        CNMMId = material.Id,
                        fk_CNMM = material,
                        InQty = material.ActualQty,
                        LogDate = stock.LogDate,
                        LogTypeId = stock.LogTypeId,
                        MaterialId = material.MaterialId.GetValueOrDefault(),
                        ObjectState = ObjectState.Added,
                        OfficeId = stock.OfficeId,
                        fk_StockLog = stock,
                        StockLogId = stock.Id
                    };
                    stock.StockMMLogs.Add(cnmmstock);
                }
                stock.InQty = stock.StockMMLogs.Sum(x => x.InQty);
            }
            
            _repo.GetRepository<CNStockLog>().Insert(stock);
            //if (GetConfigValue<long>("CNStatusTemplateId") > 0)
            //{
            //    var dtsRepo = _repo.GetRepository<DTSStatus>();
            //    var dtsstatusId = dtsRepo.Queryable().Where(x => x.DateId == 1422).Select(x => x.Id).FirstOrDefault();
            //    if (dtsstatusId > 0)
            //    {
            //        var cndts = new CNDTSStatusLog
            //        {
            //            CNId = entity.Id,
            //            fk_CN = entity,
            //            IsAuto = true,
            //            StartDate = entity.CNDate,
            //            OfficeId1 = entity.LoadingOfficeId.GetValueOrDefault(),
            //            ObjectState = ObjectState.Added,
            //            StatusId = dtsstatusId,
            //            Qty = entity.ActualQty > 0 ? entity.ActualQty : entity.ChargedQty
            //        };
            //        _repo.GetRepository<CNDTSStatusLog>().Insert(cndts);
            //        var log = new CnStatusLog()
            //        {
            //            CNId = entity.Id,
            //            DocDate = entity.CNDate,
            //            DocId = entity.Id,
            //            DocNumber = entity.CNNo,
            //            DocTypeId = 1426,
                        
            //            ObjectState = ObjectState.Added,
            //            OfficeId = entity.LoadingOfficeId.GetValueOrDefault(),
            //            PartyId = entity.BillingPartyId.GetValueOrDefault(),
            //            TransactionRemark = "New CN Created",
            //            fk_CNMaster = entity                        
            //        };
            //        _repo.GetRepository<CnStatusLog>().Insert(log);
            //    }
            //}
            return base.Insert(entity);
        }
    }
}
