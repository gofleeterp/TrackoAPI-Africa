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
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IDriverMasterService : IService<DriverMaster>
    {
        IQueryable<DriverMaster> GetAllDriverMasterList(int id);
    }
    public class DriverMasterService : Service<DriverMaster>, IDriverMasterService
    {
        private readonly IRepositoryAsync<DriverMaster> _repository;
        public DriverMasterService(IRepositoryAsync<DriverMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<DriverMaster> GetAllDriverMasterList(int brandid)
        {
            return _repository.GetAllDriverMasterList(brandid);
        }

        public override DriverMaster Insert(DriverMaster entity)
        {
            var ledger = new Ledger()
            {
                Alias = entity.DriverCode,
                ReferenceFlag = true,
                AccountName = entity.AccountDetail?.AccountName ?? entity.DriverName,
                FleetAcName = entity.DriverName,
                BookingAcName = entity.DriverName,
                GroupId = entity.AccountDetail?.GroupId,
                OfficeId = entity.OfficeId,
                IsAccountImpact = true,
                PanNo = entity.AccountDetail?.PanNo,
                Id = entity.Id,
                ObjectState = ObjectState.Added,
                InvoicePrintingName = string.IsNullOrWhiteSpace(entity.NameOnLicence) ? entity.AccountDetail?.AccountName ?? entity.DriverName : entity.NameOnLicence,
                AccountRoleId = 1085,
                GSTNatureId = 1670, /* Not Applicable */
                #region Bank Details Start
                BankAc1Id = entity.AccountDetail?.Bank1Id,
                BankAcccoutName1 = entity.AccountDetail?.BankAcccoutName1,
                BankAcccoutNo1 = entity.AccountDetail?.BankAcccoutNo1,
                BankCode1 = entity.AccountDetail?.BankCode1,
                BankSwiftCode1 = entity.AccountDetail?.BankSwiftCode1,
                BankUPI1 = entity.AccountDetail?.BankUPI1,
                BankAdd1 = entity.AccountDetail?.BankAdd1,

                BankAc2Id = entity.AccountDetail?.Bank2Id,
                BankAcccoutName2 = entity.AccountDetail?.BankAcccoutName2,
                BankAcccoutNo2 = entity.AccountDetail?.BankAcccoutNo2,
                BankCode2 = entity.AccountDetail?.BankCode2,
                BankSwiftCode2 = entity.AccountDetail?.BankSwiftCode2,
                BankUPI2 = entity.AccountDetail?.BankUPI2,
                BankAdd2 = entity.AccountDetail?.BankAdd2,
                CurTypeId = entity.AccountDetail?.CurTypeId
                #endregion

            };
            if (ledger.GroupId.GetValueOrDefault(0) == 0)
            {
                var setting =
                this._repository
                    .GetRepository<ViewField>()
                    .Queryable()
                    .FirstOrDefault(x => x.ViewId == 1032);
                if (setting?.DefaultGroupId == null) throw new BusinessException(ErrorCode.GLB103, "Drivers Default Group Not Defined and it is required.");
                ledger.GroupId = setting.DefaultGroupId;
            }
            entity.fk_Ledger = ledger;
            return base.Insert(entity);
        }

        public override void Update(DriverMaster entity)
        {
            var ledger = _repository.GetRepository<Ledger>().Queryable().FirstOrDefault(x => x.Id == entity.Id)??new Ledger();
            ledger.ReferenceFlag = true;
            ledger.Alias = entity.DriverCode;
            ledger.AccountName = entity.AccountDetail?.AccountName??entity.DriverName;
            ledger.FleetAcName = entity.DriverName;
            ledger.BookingAcName = entity.DriverName;
            ledger.GroupId = entity.AccountDetail?.GroupId;
            ledger.OfficeId = entity.OfficeId;
            ledger.IsAccountImpact = true;
            ledger.PanNo = entity.AccountDetail?.PanNo;
            ledger.Id = entity.Id;
            ledger.ObjectState = ObjectState.Modified;
            ledger.InvoicePrintingName =string.IsNullOrWhiteSpace(entity.NameOnLicence)? entity.AccountDetail?.AccountName ?? entity.DriverName: entity.NameOnLicence;
            ledger.AccountRoleId = 1085;
            ledger.GSTNatureId = 1670; /* Not Applicable */
            #region Bank Details Start
            ledger.BankAc1Id = entity.AccountDetail?.Bank1Id;
            ledger.BankAcccoutName1 = entity.AccountDetail?.BankAcccoutName1;
            ledger.BankAcccoutNo1 = entity.AccountDetail?.BankAcccoutNo1;
            ledger.BankCode1 = entity.AccountDetail?.BankCode1;
            ledger.BankSwiftCode1 = entity.AccountDetail?.BankSwiftCode1;
            ledger.BankUPI1 = entity.AccountDetail?.BankUPI1;
            ledger.BankAdd1 = entity.AccountDetail?.BankAdd1;

            ledger.BankAc2Id = entity.AccountDetail?.Bank2Id;
            ledger.BankAcccoutName2 = entity.AccountDetail?.BankAcccoutName2;
            ledger.BankAcccoutNo2 = entity.AccountDetail?.BankAcccoutNo2;
            ledger.BankCode2 = entity.AccountDetail?.BankCode2;
            ledger.BankSwiftCode2 = entity.AccountDetail?.BankSwiftCode2;
            ledger.BankUPI2 = entity.AccountDetail?.BankUPI2;
            ledger.BankAdd2 = entity.AccountDetail?.BankAdd2;
            ledger.CurTypeId=entity.AccountDetail?.CurTypeId;
            #endregion
            if (ledger.GroupId.GetValueOrDefault(0) == 0)
            {
                var setting =
                this._repository
                    .GetRepository<ViewField>()
                    .Queryable()
                    .FirstOrDefault(x => x.ViewId == 1032);
                if (setting?.DefaultGroupId == null) throw new BusinessException(ErrorCode.GLB103, "Drivers Default Group Not Defined and it is required.");
            }
            entity.fk_Ledger = ledger;
            base.Update(entity);
        }

        public override void Delete(DriverMaster entity)
        {
            if (entity.fk_Ledger == null) entity.fk_Ledger = this._repository.GetRepository<Ledger>().Find(entity.Id);
            if (entity.fk_Ledger != null) entity.fk_Ledger.ObjectState=ObjectState.Deleted;
            base.Delete(entity);
        }
    }
}
