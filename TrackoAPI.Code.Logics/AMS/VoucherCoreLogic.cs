using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Management;

using EntityFramework.Extensions;
using Repository.Pattern.DataContext;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.Code.Logics.AMS
{
    public class VoucherCoreLogic : IBaseLogic
    {
        //private static TripAdvanceCoreLogic _instance;
        //public static TripAdvanceCoreLogic Instance => _instance ?? (_instance = new TripAdvanceCoreLogic());
        public bool SaveAfterPostLogic { get; private set; }
        private IDataContextAsync _db;
        private DbSet<VoucherDetail> _vdRepo;

        public IBaseLogic Bind(IDataContextAsync db)
        {
            _vdRepo = db.Set<VoucherDetail>();
            _db = db;
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
                /*PostLogic(entry);*/
            }
            else
            {
                PostLogic(entry);
            }
        }
        public void PostLogic(DbEntityEntry entry)
        {
            if (!(entry.Entity is Voucher entity)) return;
                       
            CreateFxEntry(entity);
        }
        
        private void CreateFxEntry(Voucher voucher)
        {
            #region
            if (voucher.Id > 0 && voucher.VoucherDetails.Where(x=>x.VoucherDetailReferences.Any(k=>k.VDRTypeId==1014)).Any())
            {
                var fxledger = _db.Set<ApiConfiguration>().Where(k => k.Key == "DefaultForexGainLossAccountId").Select(x => new { x.Value }).FirstOrDefault();
                long.TryParse(fxledger.Value.ToString(), out long DefaultForexGainLossAccountId);
                if (DefaultForexGainLossAccountId <= 0)
                {
                    throw new BusinessException(ErrorCode.FOREXGNL100);
                }
                try
                {
                    _db.ExecuteProcedureAsync("[dbo].[Proc_GBL_Save_FxInVch]",
                    new[] { new SqlParameter("VoucherId", voucher.Id), new SqlParameter("FxAmount", voucher.VoucherAmount_FX), new SqlParameter("AccountId", DefaultForexGainLossAccountId) });
                }
                catch (SqlExecutionException ex)
                {
                    throw new BusinessException(ex.Message);
                }
            }
            #endregion
        }        
    }
}
