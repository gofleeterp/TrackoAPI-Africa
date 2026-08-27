using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    /// <summary>
    /// Interface IVoucherTypeGroupMappingService
    /// </summary>
    //VoucherTypeGroupMappingTypeGroupMapping}" />
    public interface IVoucherTypeGroupMappingService:IService<VoucherTypeGroupMapping>
    {
        /// <summary>
        /// Gets the ledgers by voucher identifier.
        /// </summary>
        /// <param name="voucherId">The voucher identifier.</param>
        /// <param name="type">The type.</param>
        /// <returns>IQueryable&lt;Ledger&gt;.</returns>
        IQueryable<Ledger> GetLedgersByVoucherTypeId(long? voucherId, long type,long? viewId);

        void VerifyExistanceOfMapping(VoucherTypeGroupMapping entity);
    }
    /// <summary>
    /// Class VoucherTypeGroupMappingService.
    /// </summary>
    //VoucherTypeGroupMappingTypeGroupMapping}" />
    /// <seealso cref="TrackoApi.Service.IVoucherTypeGroupMappingService" />
    public class VoucherTypeGroupMappingService:Service<VoucherTypeGroupMapping>, IVoucherTypeGroupMappingService
    {
        private IRepositoryAsync<VoucherTypeGroupMapping> _repo;
        /// <summary>
        /// Initializes a new instance of the <see cref="VoucherTypeGroupMappingService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public VoucherTypeGroupMappingService(IRepositoryAsync<VoucherTypeGroupMapping> repository) : base(repository)
        {
            _repo = repository;
        }

        /// <summary>
        /// Gets the ledgers by voucher identifier.
        /// </summary>
        /// <param name="voucherId">The voucher identifier.</param>
        /// <param name="type">The type.</param>
        /// <returns>IQueryable&lt;Ledger&gt;.</returns>
        /// <exception cref="BusinessException">VoucherVisiblityFlag key not found in Configuration.</exception>
        public IQueryable<Ledger> GetLedgersByVoucherTypeId(long? voucherId, long type,long? viewId)
        {
           //return _repo.GetLedgersByVoucherTypeId(voucherId, type,viewId);
            return _repo.GetLedgersByFieldId(voucherId, type, viewId);
        }

        /// <exception cref="BusinessException">Mapping Already Exists.</exception>
        public override VoucherTypeGroupMapping Insert(VoucherTypeGroupMapping entity)
        {
            if (
                Queryable()
                .AsNoTracking()
                    .Any(
                        x =>
                            x.TypeId == entity.TypeId && x.LedgerRoleId==entity.LedgerRoleId&&
                            x.GroupId == entity.GroupId && x.VoucherTypeId == entity.VoucherTypeId&&x.Include==entity.Include&&x.Exclude==entity.Exclude))
            {
                throw new BusinessException(ErrorCode.GLB104,"Voucher Type Mapping Already Exists");
            }
            return base.Insert(entity);
        }

        /// <exception cref="BusinessException">Mapping Already Exists..</exception>
        public void VerifyExistanceOfMapping(VoucherTypeGroupMapping entity)
        {
            if (
                Queryable()
                .AsNoTracking()
                    .Any(
                        x =>
                            x.TypeId == entity.TypeId && x.LedgerRoleId == entity.LedgerRoleId &&
                            x.GroupId == entity.GroupId && x.VoucherTypeId == entity.VoucherTypeId&&x.Id!=entity.Id))
            {
                throw new BusinessException(ErrorCode.GLB104, "Voucher Type Mapping Already Exists");
            }
        }
    }
}
