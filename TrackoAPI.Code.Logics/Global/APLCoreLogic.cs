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
using Microsoft.SqlServer.Server;

using Repository.Pattern.DataContext;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.Global;

namespace TrackoAPI.Code.Logics.Global
{
    public class APLCoreLogic : IBaseLogic
    {
        //private static TripAdvanceCoreLogic _instance;
        //public static TripAdvanceCoreLogic Instance => _instance ?? (_instance = new TripAdvanceCoreLogic());
        public bool SaveAfterPostLogic { get; private set; }
        private IDataContextAsync _db;

        public IBaseLogic Bind(IDataContextAsync db)
        {
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
                /*SaveAfterPostLogic = true;*/
            }
            else
            {
                PostAPLLogic(entry);
            }
        }
        public void PostAPLLogic(DbEntityEntry entry)
        {
            if (entry.Entity is VehicleMovementLog tl && tl.TripTypeId==1159)
            {
                long.TryParse(tl.FormId,out long _Viewid);
                CreateAPLEntry(tl.Id,tl.TriplogNo, _Viewid);
            }
            else if (entry.Entity is Voucher v)
            {
                if (v.VoucherTypeId == 1012)
                {
                    CreateAPLEntry(v.Id, v.VoucherNo, 1012);
                }
            }
            else if (entry.Entity is PurchaseRequisition pr)
            {
                CreateAPLEntry(pr.Id, pr.DocNo, pr.ViewId.GetValueOrDefault());
            }
            else if (entry.Entity is PurchaseOrder po)
            {
                CreateAPLEntry(po.Id, po.PONo, po.ViewId.GetValueOrDefault());
            }
        }

        private void CreateAPLEntry(long Id, string DocNo, long ViewId)
        {
            #region
            if ((Id > 0 || !string.IsNullOrWhiteSpace(DocNo)) && ViewId > 0)
            {
                try
                {
                    _db.ExecuteProcedureAsync("[dbo].[Proc_GBL_CreateAPLData]",
                    new[] { new SqlParameter("parameter1", Id), new SqlParameter("parameter2", ViewId), new SqlParameter("parameter3", DocNo) });
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
