using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IVehicleMasterService : IService<VehicleMaster>
    {
        IQueryable<VehicleMaster> GetAllVehicleMasterList(int id);
    }
    public class VehicleMasterService : Service<VehicleMaster>, IVehicleMasterService
    {
        private readonly IRepositoryAsync<VehicleMaster> _repository;
        public VehicleMasterService(IRepositoryAsync<VehicleMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleMaster> GetAllVehicleMasterList(int brandid)
        {
            return _repository.GetAllVehicleMasterList(brandid);
        }

        public override VehicleMaster Insert(VehicleMaster entity)
        {
            var ledger = new Ledger()
            {
                Alias = entity.VehicleNo,
                AccountName = string.IsNullOrWhiteSpace(entity.AccountDetail.AccountName)?entity.VehicleRegNo: entity.AccountDetail.AccountName,
                FleetAcName = entity.VehicleNo,
                BookingAcName = entity.VehicleNo,
                GroupId = entity.AccountDetail.GroupId,
                OfficeId = entity.OfficeId,
                IsAccountImpact = true,
                Id = entity.Id,
                ObjectState = ObjectState.Added,
                AccountRoleId = 1130,
                GSTNatureId=1670 /* Not Applicable */
            };
            
            if (ledger.GroupId.GetValueOrDefault(0) == 0)
            {
                var defaultGroup =
                this._repository
                    .GetRepository<ViewField>()
                    .Queryable()
                    .FirstOrDefault(x => x.ViewId == 1031);
                //if (setting?.DefaultGroupId == null) throw new BusinessException(ErrorCode.GLB103, "Vehicle Ledger Default Group Not Defined and it is required.");
                if (defaultGroup != null)
                {
                    ledger.GroupId = defaultGroup.DefaultGroupId;
                }
            }
            var setting = _repository.GetRepository<ApiConfiguration>().Find("ShowVehicleInAccounts");
            var result = false;
            if (setting != null)
            {
                bool.TryParse(setting.Value, out result);
            }
            ledger.IsAccountImpact = result;
            entity.fk_VehicleLedger = ledger;
            entity.Id = ledger.Id;
            return base.Insert(entity);
        }

        public override void Update(VehicleMaster entity)
        {
            var ledger = _repository.GetRepository<Ledger>().Find(entity.Id);
            if (ledger == null)
            {
                ledger = new Ledger()
                {
                    Alias = entity.VehicleNo,
                    AccountName =
                        string.IsNullOrWhiteSpace(entity.AccountDetail.AccountName)
                            ? entity.VehicleRegNo
                            : entity.AccountDetail.AccountName,
                    FleetAcName = entity.VehicleNo,
                    BookingAcName = entity.VehicleNo,
                    GroupId = entity.AccountDetail.GroupId,
                    OfficeId = entity.OfficeId,
                    IsAccountImpact = true,
                    Id = entity.Id,
                    ObjectState = ObjectState.Modified,
                    AccountRoleId = 1130,
                    GSTNatureId = 1670 /* Not Applicable */
                };
                if (ledger.GroupId.GetValueOrDefault(0) == 0)
                {
                    var defaultGroup =
                        this._repository
                            .GetRepository<ViewField>()
                            .Queryable()
                            .FirstOrDefault(x => x.ViewId == 1031);
                    //if (setting?.DefaultGroupId == null) throw new BusinessException(ErrorCode.GLB103, "Vehicle Ledger Default Group Not Defined and it is required.");
                    if (defaultGroup != null)
                    {
                        ledger.GroupId = defaultGroup.DefaultGroupId;
                    }
                }
            }
            else
            {
                ledger.Alias = entity.VehicleNo;
                ledger.AccountName = string.IsNullOrWhiteSpace(entity.AccountDetail.AccountName)
                    ? entity.VehicleRegNo
                    : entity.AccountDetail.AccountName;
                ledger.FleetAcName = entity.VehicleNo;
                ledger.BookingAcName = entity.VehicleNo;
                ledger.GroupId = entity.AccountDetail.GroupId;
                ledger.OfficeId = entity.OfficeId;
                ledger.IsAccountImpact = true;
                ledger.Id = entity.Id;
                ledger.ObjectState = ObjectState.Modified;
                ledger.AccountRoleId = 1130;
                ledger.GSTNatureId = 1670; /* Not Applicable */
                if (ledger.GroupId.GetValueOrDefault(0) == 0)
                {
                    var defaultGroup =
                        this._repository
                            .GetRepository<ViewField>()
                            .Queryable()
                            .FirstOrDefault(x => x.ViewId == 1031);
                    //if (setting?.DefaultGroupId == null) throw new BusinessException(ErrorCode.GLB103, "Vehicle Ledger Default Group Not Defined and it is required.");
                    if (defaultGroup != null)
                    {
                        ledger.GroupId = defaultGroup.DefaultGroupId;
                    }
                }
            }
            
            var setting = _repository.GetRepository<ApiConfiguration>().Find("ShowVehicleInAccounts");
            var result = false;
            if (setting != null)
            {
                bool.TryParse(setting.Value, out result);
            }
            ledger.IsAccountImpact = result;
            entity.fk_VehicleLedger = ledger;
            base.Update(entity);
        }

        public override void Patch(VehicleMaster entity)
        {
            var ledger = _repository.GetRepository<Ledger>().Find(entity.Id);
            if (ledger == null)
            {
                ledger = new Ledger()
                {
                    Alias = entity.VehicleNo,
                    AccountName =
                        string.IsNullOrWhiteSpace(entity.AccountDetail.AccountName)
                            ? entity.VehicleRegNo
                            : entity.AccountDetail.AccountName,
                    FleetAcName = entity.VehicleNo,
                    BookingAcName = entity.VehicleNo,
                    GroupId = entity.AccountDetail.GroupId,
                    OfficeId = entity.OfficeId,
                    IsAccountImpact = true,
                    Id = entity.Id,
                    ObjectState = ObjectState.Modified,
                    AccountRoleId = 1130,
                    GSTNatureId = 1670 /* Not Applicable */
                };
                if (ledger.GroupId.GetValueOrDefault(0) == 0)
                {
                    var defaultGroup =
                        this._repository
                            .GetRepository<ViewField>()
                            .Queryable()
                            .FirstOrDefault(x => x.ViewId == 1031);
                    //if (setting?.DefaultGroupId == null) throw new BusinessException(ErrorCode.GLB103, "Vehicle Ledger Default Group Not Defined and it is required.");
                    if (defaultGroup != null)
                    {
                        ledger.GroupId = defaultGroup.DefaultGroupId;
                    }
                }
            }
            else
            {
                ledger.Alias = entity.VehicleNo;
                ledger.AccountName = string.IsNullOrWhiteSpace(entity.AccountDetail.AccountName)
                    ? entity.VehicleRegNo
                    : entity.AccountDetail.AccountName;
                ledger.FleetAcName = entity.VehicleNo;
                ledger.BookingAcName = entity.VehicleNo;
                ledger.GroupId = entity.AccountDetail.GroupId;
                ledger.OfficeId = entity.OfficeId;
                ledger.IsAccountImpact = true;
                ledger.Id = entity.Id;
                ledger.ObjectState = ObjectState.Modified;
                ledger.AccountRoleId = 1130;
                ledger.GSTNatureId = 1670; /* Not Applicable */
                if (ledger.GroupId.GetValueOrDefault(0) == 0)
                {
                    var defaultGroup =
                        this._repository
                            .GetRepository<ViewField>()
                            .Queryable()
                            .FirstOrDefault(x => x.ViewId == 1031);
                    //if (setting?.DefaultGroupId == null) throw new BusinessException(ErrorCode.GLB103, "Vehicle Ledger Default Group Not Defined and it is required.");
                    if (defaultGroup != null)
                    {
                        ledger.GroupId = defaultGroup.DefaultGroupId;
                    }
                }
            }

            var setting = _repository.GetRepository<ApiConfiguration>().Find("ShowVehicleInAccounts");
            var result = false;
            if (setting != null)
            {
                bool.TryParse(setting.Value, out result);
            }
            ledger.IsAccountImpact = result;
            entity.fk_VehicleLedger = ledger;
            base.Patch(entity);
        }
    }
}
