using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System.Data.Entity;
using System.Linq;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;

namespace TrackoApi.Service.BMS
{
    public interface ICNMultiMaterialService : IService<CNMultiMaterial>
    {

    }
    public class CNMultiMaterialService : Service<CNMultiMaterial>, ICNMultiMaterialService
    {
        private readonly IRepositoryAsync<CNMultiMaterial> _repository;
        public CNMultiMaterialService(IRepositoryAsync<CNMultiMaterial> repository) : base(repository)
        {
            _repository = repository;
        }

        public override CNMultiMaterial Insert(CNMultiMaterial entity)
        {
            if (entity.MaterialId.GetValueOrDefault() == 0) return base.Insert(entity);
            var repo = _repository.GetRepository<CNStockLog>();
            var cnrepo = _repository.GetRepository<CNMaster>();

            var cnmaster = cnrepo.Queryable().FirstOrDefault(x => x.Id == entity.CnId);
            var stocks = repo.Queryable().Include(x => x.StockMMLogs).Where(x => x.CNId == entity.CnId);
            //if(stocks.Count(x=>x.TriplogId!=x.fk_CNMaster.TripLogId)>1) throw new BusinessException(ErrorCode.GLB106, "dispatched CN/LR cant be modified.");

            if (!_repository.GetRepository<CNMaster>().Queryable().Any(x => x.Id == entity.CnId))
            {
                throw new BusinessException(ErrorCode.GLB106, "CN/LR Not Found");
            }

            var createStockMMForDirectDelivery = (GetConfigValue("CreateStockMMLogForDD", 1) == 1 && cnmaster.DeliveryTypeId == 1472/*Direct Delivery*/) || cnmaster.DeliveryTypeId != 1472;
            if (createStockMMForDirectDelivery)
            {
                var stock = stocks.FirstOrDefault(x => x.LogTypeId == 1422 && x.RefStockId == null);
                if (stock == null) throw new BusinessException(ErrorCode.GLB106, "SE:Stock Data Integrity Failed...Hint:InStock was null");
                if (entity.MaterialId.GetValueOrDefault() > 0)
                {
                    var cnmmstock = new CNStockMMLog
                    {
                        CNId = entity.CnId,
                        CNMMId = entity.Id,
                        fk_CNMM = entity,
                        InQty = entity.ActualQty,
                        LogDate = stock.LogDate,
                        LogTypeId = stock.LogTypeId,
                        MaterialId = entity.MaterialId.GetValueOrDefault(),
                        ObjectState = ObjectState.Added,
                        OfficeId = stock.OfficeId,
                        fk_StockLog = stock,
                        StockLogId = stock.Id
                    };
                    stock.ObjectState = ObjectState.Modified;
                    stock.InQty = stock.InQty + cnmmstock.InQty;
                    stock.StockMMLogs.Add(cnmmstock);
                }
            }
            return base.Insert(entity);
        }

        public override void Update(CNMultiMaterial entity)
        {
            if (entity.MaterialId.GetValueOrDefault() == 0) base.Update(entity);
            var repo = _repository.GetRepository<CNStockLog>();
            var cnrepo = _repository.GetRepository<CNMaster>();
            var cnmaster = cnrepo.Queryable().FirstOrDefault(x => x.Id == entity.CnId);
            var stocks = repo.Queryable().Include(x => x.StockMMLogs).Where(x => x.CNId == entity.CnId);
            //if (stocks.Count(x => x.TriplogId != x.fk_CNMaster.TripLogId) > 1) throw new BusinessException(ErrorCode.GLB106, "dispatched CN/LR cant be modified.");
            if (!_repository.GetRepository<CNMaster>().Queryable().Any(x => x.Id == entity.CnId))
            {
                throw new BusinessException(ErrorCode.GLB106, "CN/LR Not Found");
            }

            var createStockMMForDirectDelivery = (GetConfigValue("CreateStockMMLogForDD", 1) == 1 && cnmaster.DeliveryTypeId == 1472/*Direct Delivery*/) || cnmaster.DeliveryTypeId != 1472;
            if (createStockMMForDirectDelivery)
            {
                var stock = stocks.FirstOrDefault(x => x.LogTypeId == 1422 && x.RefStockId == null);
                if (stock == null) throw new BusinessException(ErrorCode.GLB106, "SE:Stock Data Integrity Failed...Hint:InStock was null");
                var cnmmstock = stock.StockMMLogs.FirstOrDefault(x => x.CNMMId == entity.Id) ?? new CNStockMMLog();
                cnmmstock.CNId = entity.CnId;
                cnmmstock.CNMMId = entity.Id;
                cnmmstock.fk_CNMM = entity;
                cnmmstock.InQty = entity.ActualQty;
                cnmmstock.LogDate = stock.LogDate;
                cnmmstock.LogTypeId = stock.LogTypeId;
                cnmmstock.MaterialId = entity.MaterialId.GetValueOrDefault();
                cnmmstock.OfficeId = stock.OfficeId;
                cnmmstock.fk_StockLog = stock;
                cnmmstock.StockLogId = stock.Id;
                if (cnmmstock.Id > 0)
                {
                    cnmmstock.ObjectState = ObjectState.Modified;
                }
                else
                {
                    cnmmstock.ObjectState = ObjectState.Added;
                    stock.StockMMLogs.Add(cnmmstock);
                }
                stock.InQty = stock.StockMMLogs.Where(x => x.Id != cnmmstock.Id).Sum(x => x.InQty) + cnmmstock.InQty;
                stock.ObjectState = ObjectState.Modified;

                repo.Update(stock);
            }
            base.Update(entity);
        }

        public override void Delete(CNMultiMaterial entity)
        {
            var repo = _repository.GetRepository<CNStockLog>();
            var stocks = repo.Queryable().Include(x => x.StockMMLogs).Where(x => x.CNId == entity.CnId);
            //if (stocks.Count(x => x.TriplogId != x.fk_CNMaster.TripLogId) > 1) throw new BusinessException(ErrorCode.GLB106, "dispatched CN/LR cant be modified.");
            //if (!_repository.GetRepository<CNMaster>().Queryable().Any(x => x.Id == entity.CnId))
            //{
            //    throw new BusinessException(ErrorCode.GLB106, "CN/LR Not Found");
            //}
            var stock = stocks.FirstOrDefault(x => x.LogTypeId == 1422 && x.RefStockId == null);
            if (stock != null)
            {
                stock.InQty = stock.InQty - entity.ActualQty;
                stock.ObjectState = ObjectState.Modified;
            }
            base.Delete(entity);
        }
    }
}
