using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.DataContext;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;

namespace TrackoAPI.Code.Logics.BMS
{
    public class SalesLogCoreLogic : BaseLogic<SalesLog>
    {
        //protected static SalesLogCoreLogic _Instance;
        //public static SalesLogCoreLogic Instance => _Instance ?? (_Instance = new SalesLogCoreLogic());

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
            var cnbillog = entry.Entity as SalesLog;
            var salesConfig = _db.GetApiConfig<int>("GenerateSalesVoucher");
            if (cnbillog != null&& salesConfig==1)
            {
                Voucher salesVoucher = null;
                switch (cnbillog.ObjectState)
                {
                    case ObjectState.Added:
                        salesVoucher = new Voucher();
                        cnbillog.PrepareSalesVoucher(ref salesVoucher);
                        _db.Set<Voucher>().Add(salesVoucher ?? throw new InvalidOperationException("CE:Sales Voucher was null while creating"));
                        break;
                    case ObjectState.Modified:
                        if (cnbillog.VDRId > 0&&_db.Set<VoucherDetailReference>().Any(x=>x.RefId==cnbillog.VDRId))
                        {
                            throw new BusinessException(ErrorCode.GLB106,$"Cannot Update Sales Log as Some one has created Bill against it. Ref No:{cnbillog.DocNo}");
                        }
                        salesVoucher = _db.Set<Voucher>().Include(x=>x.VoucherDetails.Select(y=>y.VoucherDetailReferences)).FirstOrDefault(x => x.Id == cnbillog.SalesVoucherId);
                        cnbillog.PrepareSalesVoucher(ref salesVoucher);
                        _db.Set<Voucher>().AddOrUpdate(salesVoucher ?? throw new InvalidOperationException("CE:Sales Voucher was null while updating"));
                        break;
                    case ObjectState.Deleted:
                        if (cnbillog.VDRId > 0 && _db.Set<VoucherDetailReference>().Any(x => x.RefId == cnbillog.VDRId))
                        {
                            throw new BusinessException(ErrorCode.GLB106, $"Cannot Delete Sales Log as Some one has created Bill against it. Ref No:{cnbillog.DocNo}");
                        }
                        salesVoucher = _db.Set<Voucher>().FirstOrDefault(x => x.Id == cnbillog.SalesVoucherId);
                        cnbillog.SalesVoucherId = null;
                        cnbillog.fk_SalesVoucher = null;
                        cnbillog.VDRId = null;
                        cnbillog.fk_VDR = null;
                        if (salesVoucher != null)
                        {
                            salesVoucher.ObjectState = ObjectState.Deleted;
                            _db.Set<Voucher>().Remove(salesVoucher);
                        }
                        break;
                }
            }

        }
        //public override bool SaveAfterPostLogic { get; set; }
        //public override DbSet<SalesLog> DbSet => _db.Set<SalesLog>();
    }
}
