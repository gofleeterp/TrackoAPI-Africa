using EntityFramework.Extensions;

using MoreLinq;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations.AMS;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.FMS.Repairs;

namespace TrackoApi.Service
{
    public interface ISpareLogService : IService<SpareLog>
    {
        IQueryable<SpareLog> GetAllSpareLogList(int id);
        vwSparePurchaseBill GetPurchaseBillView(long id, long type);
        SpareLogExtraInfo InsertOrUpdateMaterialMRNView(vwSparePurchaseBill view);
        SpareLogExtraInfo InsertOrUpdateMaterialSettlementMRNView(vwSparePurchaseBill view);
        SpareLogExtraInfo InsertOrUpdateMaterialDeliveryChallanView(vwSparePurchaseBill view);
        Task DeleteGraph(long key);
        void BatchInsert(List<vwSparePurchaseBill> docs, IDbTransaction transaction);
        void AmcBatchInsert(List<vwSparePurchaseBill> docs, IDbTransaction transaction);
        SpareLogExtraInfo InsertOrUpdatePurchaseBillView(vwSparePurchaseBill view);
    }
    public class SpareLogService : Service<SpareLog>, ISpareLogService
    {
        private readonly IRepositoryAsync<SpareLog> _repository;
        public SpareLogService(IRepositoryAsync<SpareLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<SpareLog> GetAllSpareLogList(int brandid)
        {
            return _repository.GetAllSpareLogList(brandid);
        }

        public vwSparePurchaseBill GetPurchaseBillView(long id, long type)
        {
            return _repository.GetPurchaseBillView(id, type);
        }

        /// <exception cref="BusinessException"><see cref="ErrorCode.VCH108"/>Voucher with Provided VoucherId does not exists.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        public SpareLogExtraInfo InsertOrUpdatePurchaseBillView(vwSparePurchaseBill view)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.PrimaryCreditAccountId == 0 && view.PrimaryCreditAmount != 0)
            {
                throw new BusinessException(ErrorCode.GLB106,$"Primary Credit Account is Required.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault()<= 0 && view.PrimaryDebitAmount != 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account Amount should always be greater than Zero");
            }            
            long transitstoreId;
            long PricipalOwnerId=0;
            long VehicleOwnerId=0;
            if (view.VoucherTypeId == 25 || view.VoucherTypeId == 26)
            {
                var transitStoreQuery =
                    _repository.GetRepository<ApiConfiguration>()
                        .Queryable()
                        .Where(x => x.Key == "TransitStoreId")
                        .Select(x => x.Value)
                        .FromCacheFirstOrDefault();
                if (!long.TryParse(transitStoreQuery, out transitstoreId))
                {
                    throw new BusinessException(ErrorCode.GLB103,
                        "Transit store need to be configured first before storetransfer");
                }

                if (view.VoucherTypeId == 25)
                {
                    view.ProvisionalAcId = view.PrimaryDebitAccountId;
                    view.PrimaryDebitAccountId = transitstoreId;
                }
                else if (view.VoucherTypeId == 26)
                {
                    view.ProvisionalAcId = view.PrimaryCreditAccountId;
                    view.PrimaryCreditAccountId = transitstoreId;
                }

            }
            else if (view.VoucherTypeId == 24) {
                try
                {
                    var PricipalOwnerIdQuery =
                            _repository.GetRepository<ApiConfiguration>()
                                .Queryable()
                                .Where(x => x.Key == "PricipalOwnerId")
                                .Select(x => x.Value)
                                .FromCacheFirstOrDefault();
                    long.TryParse(PricipalOwnerIdQuery, out PricipalOwnerId);
                }
                catch { }

                if (PricipalOwnerId > 0)
                {

                    var vehlist = view.Spares.DistinctBy(k => k.VehicleId).Select(x => x.VehicleId).ToList();
                    if (vehlist != null && vehlist.Count > 0)
                    {
                        var vdata =
                                _repository.GetRepository<VehicleMaster>()
                                    .Queryable()
                                    .Where(x => vehlist.Contains(x.Id))
                                    .Include(x => x.fk_VehicleOwner)  // Ensure related data is loaded
                                    .DistinctBy(k => new { k.OwnerPartyId, k.fk_VehicleOwner.ReferenceFlag })
                                    .Select(x => new { x.OwnerPartyId, x.fk_VehicleOwner.ReferenceFlag })
                                    .ToList();

                        if (vdata?.Count() > 1)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Only one vehicle is permitted in the case of an non-company vehicle(s)");
                        }
                        VehicleOwnerId = vdata.FirstOrDefault().OwnerPartyId ?? 0;
                        if (PricipalOwnerId != VehicleOwnerId && VehicleOwnerId > 0)
                        {
                            if (!vdata.FirstOrDefault().ReferenceFlag)
                            {
                                throw new BusinessException(ErrorCode.GLB106, "Bill by Bill Flag should be ON for Vehicle Owner");
                            }
                            else
                            {
                                view.PrimaryDebitAccountId = VehicleOwnerId;
                            }
                        }
                    }
                }
            }

            var seiRepo = _repository.GetRepository<SpareLogExtraInfo>();
            Voucher v = new Voucher();
            Voucher tdsvoucher = new Voucher();
            SpareLogExtraInfo sei = new SpareLogExtraInfo();
            if (view.Id > 0)
            {
                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                sei = seiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id);
                if (sei == null)
                {
                    //Through error if voucher not found with provided vouchertypeid
                    throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
                }
                v = _repository.GetRepository<Voucher>().Query(x => x.Id == sei.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (sei.TDSVoucherId > 0)
                {
                    tdsvoucher = _repository.GetRepository<Voucher>().Query(x => x.Id == sei.TDSVoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                }
               
                if (v == null)
                {
                    //Through error if voucher not found with provided vouchertypeid
                    throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
                }
            }

            #region SpareLog Preparation
            //Collect Distinct ReferenceId's from Posted SpareLogs
            var uniqeref = view.Spares.Select(x => x.ReferenceId).Distinct().ToList();
            //Fetch Stock Information for all above ReferenceId's 
            var stockinfo = _repository.Queryable().AsNoTracking()
                 .Where(x => uniqeref.Contains(x.Id)).Select(x => new
                 {
                     x.Id,
                     x.Qty,
                     x.DrAccountId,
                     //also include existing issued log other than current voucher for above ReferenceId's 
                     Issued = x.IssuedLogs.Where(b => b.ExtraInfoId != view.Id).Select(y => new { y.Id, y.Qty })
                 }).ToList();
            
            var tsluniqeref = view.Spares.Select(x => x.TSLId).Distinct().ToList();

            var issuedtsl =
                _repository.GetRepository<SpareLog>()
                .Queryable()
                .Where(x => tsluniqeref.Contains(x.TSLId) && x.ExtraInfoId != view.Id)
                .GroupBy(y => y.TSLId)
                .Select(g => new
                {
                    TSLId = g.Key,
                    TotalQty = g.Sum(item => item.Qty)
                }).ToList();

            var tsl =
                _repository.GetRepository<TransactionSupportLog>()
                .Queryable()
                .Where(x => tsluniqeref.Contains(x.Id))
                .Select(g => new
                {
                    g.Id,
                    Qty = g.Value1
                }).ToList();



            #region Fatch All Existing Items From Database

            var sps= _repository.Queryable().Where(x => x.ExtraInfoId == view.Id).ToList();
            #endregion
            var sparelogs = new List<SpareLog>();
            //Prepare DbModel for each View Model
            foreach (var a in view.Spares)
            {
                SpareLog s = new SpareLog();
                //If it is existing record try to fatch it from database
                if (view.Id > 0)
                {
                    s = sps.FirstOrDefault(x => x.Id == a.Id);
                    //If Spare Part has been issued against purchase or stock Transfer In then mark as Unchanged
                    if ((view.VoucherTypeId == 130 || view.VoucherTypeId == 125 || view.VoucherTypeId == 61 || view.VoucherTypeId == 62 || view.VoucherTypeId == 26) && s != null&& s.Id > 0)
                    {
                        var count = _repository.Queryable().Any(x => x.ReferenceId == s.Id);
                        if (count)
                        {
                            throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Item Information that has been referenced/issued.Ref:{a.SpareName}[{a.Id}]");
                        }
                    }

                    if ((view.VoucherTypeId == 125) && s != null && s.Id > 0)
                    {
                        var tpno = _repository.GetRepository<VehicleMovementLog>()
                            .Queryable().Where(x => x.MaterialInvoiceId == s.ExtraInfoId)
                            .Select(y=>y.TriplogNo).FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(tpno))
                        {
                            throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Invoice is mapped with TripNo:{tpno}]");
                        }
                    }
                }
                if(s==null)s= new SpareLog();
                s.TSLId = a.TSLId;
                s.Amount = a.Amount;
                s.OtherAmount = a.OtherAmount;
                s.VehicleId = a.VehicleId;
                s.HireVehicleId = a.HireVehicleId;
                s.UnitId = a.UnitId;
                s.Remark = a.Remark;
                s.Qty = a.Qty;
                s.DepositedQty = a.DepositedQty;
                s.SparePartId = a.SpareId;
                s.TaxServiceTypeId = a.TaxServiceTypeId;
                
                //s.VatAmount = a.VatAmount;
                s.CGSTAmount = a.CGSTAmount;
                s.SGSTAmount = a.SGSTAmount;
                s.IGSTAmount = a.IGSTAmount;
                s.Rate = a.Rate;
                s.SubTotal = a.Amount - a.DiscountAmount + s.OtherAmount; //VatAmount
                s.DiscountAmount = a.DiscountAmount;
                s.DiscountPercent = a.DiscountPercent;
                s.MakeId = a.MakeId;
                s.BinId = a.BinId;
                s.NetAmount = a.NetAmount;
                s.POLogId = a.PurchaseId;
                // s.VatPercent = a.VatPercent;
                s.CGSTRate = a.CGSTRate;
                s.SGSTRate = a.SGSTRate;
                s.IGSTRate = a.IGSTRate;
                s.RoundOff = a.RoundOff;
                s.PostDisount = a.PostDisount;
                s.WarrantyDays = a.WarrantyDays;
                s.WarrantyKm = a.WarrantyKm;
                s.ODOKm = a.ODOKm;
                s.VoucherDate = view.DocumentDate;
                s.VoucherNo = view.DocumentNo;
                s.MechanicId = a.MechanicId;
                //if id is gt Zero Mark entity as Modified
                s.ObjectState = s.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    s.FittingPositionId = a.FittingPositionId;
                s.DrAccountId = view.PrimaryDebitAccountId;
                s.CrAccountId = view.PrimaryCreditAccountId;
                s.JobCardId = a.JobCardId;
                s.StockQty = (view.VoucherTypeId == 130 || view.VoucherTypeId == 125 || view.VoucherTypeId == 61 || view.VoucherTypeId == 62 || view.VoucherTypeId == 26 || view.VoucherTypeId == 25) ? s.Qty:0;
                s.VoucherTypeId = view.VoucherTypeId;
                if (!s.ReferenceId.HasValue && a.ReferenceId > 0)
                {
                    s.ReferenceId = a.ReferenceId;
                }

                if ((s.Amount - s.DiscountAmount + (!view.CalVat ? s.CGSTAmount + s.SGSTAmount + s.IGSTAmount  : 0)+ s.OtherAmount != s.NetAmount)) //VatAmount
                {
                    throw new BusinessException(ErrorCode.SPB100, $"Net Amount Doesn't tally with detail amounts for Spare Name:{a.SpareName}");
                }
                if (s.ObjectState == ObjectState.Added || s.ObjectState == ObjectState.Modified)
                {
                    if (s.ReferenceId.HasValue && (view.VoucherTypeId == 130/*goods return*/ || view.VoucherTypeId == 24/*Issue*/ || view.VoucherTypeId == 25/*Material Stock Transfer [O]*/|| view.VoucherTypeId == 26/*Stock Transfer in  --Mukesh, I have doubt on this*/))
                    {
                        var spi = stockinfo.FirstOrDefault(x => x.Id == s.ReferenceId);
                        if (spi == null)
                        {
                            throw new BusinessException(ErrorCode.SPB100, "Invalid Item selected for issue/transfer.=>ReferenceId");
                        }
                        if (s.CrAccountId != spi.DrAccountId)
                        {
                            throw new BusinessException(ErrorCode.SPB100, $"Selected Item has been Out of Stock for Selected Office.\n ItemName:{a.SpareName}");
                        }
                        if ((view.VoucherTypeId == 25 || view.VoucherTypeId == 26) && (view.PrimaryDebitAccountId.HasValue && view.PrimaryCreditAccountId > 0 && view.PrimaryDebitAccountId.Value == view.PrimaryCreditAccountId))
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Sender Store and Receiving Store should be different");
                        }
                        //SUM QtyIssued in other voucher+issued in this voucher
                        var issued = spi.Issued.Where(x => x.Id != s.Id).Sum(x => (decimal?)x.Qty).GetValueOrDefault(0) + sparelogs.Where(x => x.ReferenceId == s.ReferenceId).Sum(x => x.Qty);
                        var balance = (spi.Qty - issued);
                        if (balance == 0 || (balance != 0 && s.Qty > balance))
                        {
                            throw new BusinessException(ErrorCode.SPB102, new ValidationResult($"Issue Qty exceded Stock Qty for Item :{a.SpareName} or Item is not in stock"));
                        }

                        /*TSL*/
                        if (a.TSLId > 0)
                        {
                            var qtyissue = view.Spares.Where(x => x.TSLId == a.TSLId).Sum(x => (decimal?)x.Qty).GetValueOrDefault(0) 
                                + issuedtsl.Where(x => x.TSLId == a.TSLId).Sum(x => x.TotalQty);

                            var balancetsl = (tsl.Where(x => x.Id == a.TSLId).Sum(x => (decimal?)x.Qty) - qtyissue);
                            if (balancetsl<0)
                            {
                                throw new BusinessException(ErrorCode.SPB102, new ValidationResult($"Requested Qty already issued for Item :{a.SpareName}"));
                            }
                        }
                    }
                }
                s.fk_Voucher = v;
                sparelogs.Add(s);
                if (a.JsonData != null)
                {
                    foreach (var entity in a.JsonData)
                    {
                        s.DeleteAndAdd(entity);
                    }
                }
            }


            //Delete All the SpareLogs that was mapped to this voucherid before now but not now
            var spareids = sparelogs.Where(x => x.Id > 0).Select(x => x.Id).ToList();
            if (view.Id > 0)
            {
                var deleted =
                _repository.Queryable()
                    .Include(x => x.IssuedLogs)
                    .Where(x => x.ExtraInfoId == view.Id && !spareids.Contains(x.Id));

                var omitedTransaction =
                    deleted.Any(x => x.IssuedLogs.Any());
                if (omitedTransaction)
                {
                    throw new BusinessException(ErrorCode.GLB105, "Cannot Delete Item Information that has been referenced/issued.");
                }
                //Delete All the SpareLogs that was mapped to this voucherid before but not now
                foreach (var x in deleted)
                {
                    x.ObjectState = ObjectState.Deleted;
                    x.VoucherId = null;
                    x.fk_Voucher = null;
                    x.ExtraInfoId = null;
                    x.ExtraInfo = null;
                    //_repository.Delete(x.Id);
                }
            }
            
            if (stockinfo.Any())
            {
                //Less the issued items from stock
                var stockids = stockinfo.Select(m => m.Id).ToList();
                var stockinfos=_repository.Queryable().Where(x => stockids.Contains(x.Id)).ToList();
                foreach (var inf in stockinfo)
                {
                    var part = stockinfos.FirstOrDefault(x => x.Id == inf.Id);
                    var issued = inf.Issued.Where(x => !spareids.Contains(x.Id)).Sum(x => (decimal?)x.Qty).GetValueOrDefault(0) + sparelogs.Where(x => x.ReferenceId == inf.Id).Sum(x => x.Qty);
                    part.StockQty = part.Qty - issued;
                    part.ObjectState = ObjectState.Modified;
                    _repository.Update(part);
                }
            }
            #endregion
            #region LabourLog Preparation 
            var labours = new List<RepairLabourLog>();
            var labrepo = _repository.GetRepository<RepairLabourLog>();
            foreach (var a in view.Labors)
            {
                var l = _repository.GetRepository<RepairLabourLog>().Find(a.Id) ?? new RepairLabourLog();
                l.Amount = a.Amount;
                l.TSLId = a.TSLId;
                l.OtherAmount = a.OtherAmount;
                l.Remark = a.Remark;
                l.LaborQty = a.LaborQty;
                l.VehicleId = a.VehicleId;
                l.HireVehicleId = a.HireVehicleId;
                l.SubTotal=l.Amount - l.DiscountAmount + l.OtherAmount;
                l.DiscountAmount = a.DiscountAmount;
                l.DiscountPercent = a.DiscountPercent;
                l.NetAmount = a.NetAmount;
                l.POLogId = a.WorkOrderId;
                //l.ServiceTaxPercent = a.ServiceTaxPercent;
                //l.ServiceTaxAmount = a.ServiceTaxAmount;
                l.TaxServiceTypeId = a.GSTServiceTypeId;
                l.CGSTPercent = a.LCCGSTPercent;
                l.CGSTAmount = a.LCCGSTAmount;

                l.SGSTPercent = a.LCSGSTPercent;
                l.SGSTAmount = a.LCSGSTAmount;

                l.IGSTPercent = a.LCIGSTPercent;
                l.IGSTAmount = a.LCIGSTAmount;
                l.ODOKm = a.ODOKm;
                l.LaborId = a.LaborId;
                l.LaborRate = a.LaborRate;
                l.LaborUnitId = a.LaborUnitId;
                l.MechanicId = a.MechanicId;
                l.Compute();
                l.ObjectState = l.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                l.fk_Voucher = v;
                l.JobCardId = a.JobCardId;
                //l.ExtraInfo = sei;
                labours.Add(l);
                if (a.JsonData != null)
                {
                    foreach (var entity in a.JsonData)
                    {
                        l.DeleteAndAdd(entity);
                    }
                }
            }

            //Delete All the LabourJobs that was mapped to this voucherid before now but not now

            var labourids = labours.Where(x => x.Id > 0).Select(x => x.Id).ToList();
            if (view.Id > 0)
            {
                var deleted =
                labrepo.Queryable()
                    .Where(x => x.ExtraInfoId == view.Id && !labourids.Contains(x.Id));
                
                //Delete All the SpareLogs that was mapped to this voucherid before but not now
                foreach (var x in deleted)
                {
                    x.ObjectState = ObjectState.Deleted;
                    x.VoucherId = null;
                    x.fk_Voucher = null;
                    x.ExtraInfoId = null;
                    x.ExtraInfo = null;
                    //_repository.Delete(x.Id);
                }
            }

            #endregion

            #region VehiclePm Preparation 
            var pmVehicle = new List<VehiclePreventiveLog>();
            var pmVehiclerepo = _repository.GetRepository<VehiclePreventiveLog>();
            if (view.VehiclePm != null)
            {
               
                foreach (var a in view.VehiclePm)
                {
                    var p = pmVehiclerepo.Find(a.Id) ?? new VehiclePreventiveLog();
                    p.AlertDays = a.AlertDays;
                    p.AlertKM = a.AlertKM;
                    p.ClassId = a.ClassId;
                    p.DueAlertDate = a.DueAlertDate;
                    p.DueDate = a.DueDate;
                    p.DueDays = a.DueDays;
                    p.DueKM = a.DueKM;
                    p.JobDate = a.JobDate;
                    p.NewPMId = a.NewPMId;
                    p.PMId = a.PMId;
                    p.ScheduleId = a.ScheduleId;
                    p.StartKM = a.StartKM;
                    p.VehicleId = a.VehicleId;

                    p.ObjectState = p.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    pmVehicle.Add(p);
                }

                var pmvehids = pmVehicle.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                if (view.Id > 0)
                {
                    var deleted =
                    pmVehiclerepo.Queryable()
                        .Where(x => x.BillId == view.Id && !pmvehids.Contains(x.Id));

                    foreach (var x in deleted)
                    {
                        x.ObjectState = ObjectState.Deleted;
                    }
                }
            }

            #endregion



            #region Prepare Voucher
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.ConstCurTypeId = view.ConstCurTypeId;
            v.ViewId = view.ViewId;
            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.Value;
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount + (view.RoundOffAcId.GetValueOrDefault() <= 0 ? view.RoundOff : 0);//Spare
            v.Account1Id = view.PrimaryDebitAccountId;
            v.Account2Id = view.PrimaryCreditAccountId;
            v.Amount2 = -Math.Abs(view.PrimaryCreditAmount - (view.RoundOffAcId.GetValueOrDefault() <= 0 ? view.RoundOff : 0));//Party

            v.Account3Id = view.CGSTLedgerId;
            v.Amount3 = view.CGSTAmount;
            v.Account4Id = view.SGSTLedgerId;
            v.Amount4 = view.SGSTAmount;

            v.Account7Id = view.IGSTLedgerId;
            v.Amount7 = view.IGSTAmount;

            v.Account8Id = view.TCSAccountId;
            v.Amount8 = view.TCSAmount;

            v.Account5Id = view.OtherLedgerId;
            v.Amount5 = view.OtherAmount;
            //v.Amount6 = view.ExpenseAmount2 + (view.PrimaryDebitAmount > 0 || view.RoundOffAcId.GetValueOrDefault() <= 0 ? 0 : view.RoundOff);
            v.Amount6 = view.ExpenseAmount2 + (view.PrimaryDebitAmount > 0 || ((view.RoundOffAcId.GetValueOrDefault()) <= 0 && view.RoundOff == 0) ? 0 : view.RoundOff);
            v.Account6Id = view.ExpenseLedgerId2;

            v.Account9Id = view.RoundOffAcId;
            v.Amount9 = view.RoundOffAcId > 0 ? view.RoundOff : 0;

            v.Account10Id = view.PostDiscountAcId;
            v.Amount10 = (view.PostDiscAmount>=0?-1:1)* view.PostDiscAmount;

            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = view.VendorReferenceNo;            
            #endregion

            #region Prepare SpareLogExtraInfo
            if (sei == null)
            {
                sei = new SpareLogExtraInfo();
            }

            #region Prepare SpareLogExtraInfo
            sei.CurTypeId = view.CurTypeId;
            sei.CurRate = view.CurRate;
            sei.ConstCurTypeId = view.ConstCurTypeId;

            sei.DocNo = view.DocumentNo;
            sei.PageId = view.PageId;
            sei.VoucherTypeId = view.VoucherTypeId.Value;
            sei.OfficeId = view.OfficeId;
            sei.DocDate = view.DocumentDate;
            sei.CrAccountId = view.PrimaryCreditAccountId;
            sei.DrAccountId = view.PrimaryDebitAccountId;
            sei.CrAmount = view.PrimaryCreditAmount;
            sei.DrAmount = view.PrimaryDebitAmount;
            sei.VendorReferenceNo = view.VendorReferenceNo;

            sei.OtherChargeRatioId = view.OtherChgRatioId;
            sei.OtherChargeRatio = null;
            sei.CalculateVat = view.CalVat;
            sei.OrmId = view.ORMId;
            sei.Remark = view.Narration;
            sei.VehicleId = view.GPVehicleId;
            sei.HireVehicleId = view.HireVehicleId;
            sei.GatepassNo = view.GatepassNo;
            sei.GatepassType = view.GatepassType;
            sei.ProvisionalAcId = (view.ExpenseLedgerId2.GetValueOrDefault(0) > 0 && view.ProvisionalAcId.GetValueOrDefault(0)==0) ? view.ExpenseLedgerId2 : view.ProvisionalAcId;
            sei.ChallanSlipNo = view.ChallanSlipNo;
            sei.ChallanSlipDate = view.ChallanSlipDate;
            sei.ViewId = view.ViewId;
            sei.TypeId = view.TypeId;
            sei.fk_Voucher = v;
            sei.fk_TDSVoucher = view.TDSAmount>0 ? tdsvoucher:null;
            sei.TDSVoucherId = tdsvoucher?.Id;
            sei.TDSAccountId = view.TDSLedgerId;
            sei.ObjectState = sei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            sei.IGSTACId = view.IGSTLedgerId;
            sei.IGSTPercent = view.IGSTPercent;
            sei.IGSTAmount = view.IGSTAmount;
            sei.CGSTACId = view.CGSTLedgerId;
            sei.CGSTPercent = view.CGSTPercent;
            sei.CGSTAmount = view.CGSTAmount;
            sei.SGSTACId = view.SGSTLedgerId;
            sei.SGSTPercent = view.SGSTPercent;
            sei.SGSTAmount = view.SGSTAmount;
            sei.OtherAccountId = view.OtherLedgerId;
            sei.PostDiscAmount = view.PostDiscAmount;
            sei.PostDiscountAcId = view.PostDiscountAcId;
            sei.RoundOff = view.RoundOff;
            sei.RoundOffAcId = view.RoundOffAcId;
            sei.OtherAmount = view.OtherAmount;
            sei.TDSRate = view.TDSRate;
            sei.TCSAccountId = view.TCSAccountId;
            sei.TCSAmount = view.TCSAmount;
            sei.TCSRate = view.TCSRate;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    sei.DeleteAndAdd(entity);
                }
            }

            #endregion

            #endregion

            PrepareVoucherDetails(_repository, v, view.VendorReferenceNo,sei);
            if (view.TDSAmount > 0)
            {
                PrepareTDSVoucher(view, v.VoucherDetails.FirstOrDefault(x => x.OrderId == 2 && x.ObjectState != ObjectState.Deleted)?.VoucherDetailReferences?.FirstOrDefault(), tdsvoucher);
            }
            else
            {
                sei.TDSVoucherId = null;
                sei.fk_TDSVoucher = null;
                sei.TDSAccountId = null;
                sei.fk_TDSAccount = null;
                if (tdsvoucher != null)
                {
                    tdsvoucher.ObjectState = ObjectState.Deleted;
                    tdsvoucher?.VoucherDetails?.ForEach(x =>
                    {
                        x.ObjectState = ObjectState.Deleted;
                        x.VoucherDetailReferences?.ForEach(y =>
                        {
                            y.ObjectState = ObjectState.Deleted;
                        });
                    });
                    tdsvoucher = null;
                }
                
            }
            if (sei.Id > 0) seiRepo.Update(sei);
            else seiRepo.Insert(sei);
            #region Validations
            var vdrrequired =
                _repository.GetRepository<VoucherType>()
                    .Queryable()
                    .Where(x => x.Id == v.VoucherTypeId)
                    .Select(x => new
                    {
                        x.VDRRequired,
                        x.VDRequired
                    })
                    .FirstOrDefault();
            if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
            {
                throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
            }

            if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
            {
                throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
            }
            if (!v.VoucherDetails.TrueForAll(AccountValidations.VoucherDetailAmountIsValid))
            {
                throw new BusinessException(ErrorCode.VCH106);//VoucherDetail and VoucherDetailReference Amount Doesn't Tally
            }
            #endregion

            foreach (SpareLog log in sparelogs)
            {
                log.ExtraInfoId = sei.Id;
                log.ExtraInfo = sei;
                if (log.Id > 0)
                {
                    _repository.Update(log);
                }
                else
                {
                    _repository.Insert(log);
                }
            }

            foreach (var log in labours)
            {
                log.ExtraInfoId = sei.Id;
                log.ExtraInfo = sei;
                if (log.Id > 0)
                {
                    labrepo.Update(log);
                }
                else
                {
                    labrepo.Insert(log);
                }
            }

            if (pmVehicle.Any())
            {
                foreach (var log in pmVehicle)
                {
                    log.BillId = sei.Id;
                    log.fk_BillId = sei;
                    if (log.Id > 0)
                    {
                        pmVehiclerepo.Update(log);
                    }
                    else
                    {
                        pmVehiclerepo.Insert(log);
                    }
                }
            }
            var tpt = _repository.GetRepository<TPTRequestPool>();
            try
            {
                if (view.Id == 0 && PricipalOwnerId > 0 && VehicleOwnerId > 0 && (PricipalOwnerId != VehicleOwnerId))
                {
                    TPTRequestPool tpr = new TPTRequestPool();
                    tpr.ObjectState = ObjectState.Added;
                    tpr.RequestId = Guid.NewGuid().ToString();
                    tpr.ViewId = sei.ViewId.GetValueOrDefault();
                    tpr.RecordId = sei.Id;
                    tpr.DocNo = sei.DocNo;
                    tpr.BatchId = tpr.RequestId;
                    tpr.IsProceeded = false;
                    tpr.CreatedTime = DateTime.Now;
                    tpr.TypeKey = "ZRA_MAT_ISSUE_SALE";
                    tpt.Insert(tpr);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            return sei;
        }

        private void PrepareTDSVoucher(vwSparePurchaseBill view, VoucherDetailReference vdr, Voucher tdsVoucher)
        {
            #region Prepare TDSVoucher
            tdsVoucher.CurTypeId = view.CurTypeId;
            tdsVoucher.CurRate = view.CurRate;
            tdsVoucher.ConstCurTypeId = view.ConstCurTypeId;

            tdsVoucher.VoucherDate = view.DocumentDate;
            tdsVoucher.VoucherDateTime = view.DocumentDate;
            tdsVoucher.VoucherTypeId = 92;
            tdsVoucher.VoucherNo = "TDS-" + view.DocumentNo;

            tdsVoucher.Account1Id = view.TDSLedgerId;
            tdsVoucher.Amount1 = -view.TDSAmount;

            tdsVoucher.Account2Id = view.PrimaryCreditAccountId;
            tdsVoucher.Amount2 = view.TDSAmount;
            tdsVoucher.OfficeId = view.OfficeId;
            tdsVoucher.AccountingRemark = $"Being TDS deducted against bill no {view.DocumentNo} on amount {view.PrimaryCreditAmount - view.RoundOff}";
            tdsVoucher.ObjectState = tdsVoucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;

            #endregion

            #region Prepare TDSVoucherVd and VDR

            var ledgerRepo = _repository.GetRepository<Ledger>().Queryable();
            var vdrflag = ledgerRepo.Where(x => x.Id == tdsVoucher.Account1Id || x.Id == tdsVoucher.Account2Id)
                .Select(x => new { x.Id, x.OfficeId, x.ReferenceFlag }).ToList();

            if (tdsVoucher.Account1Id.HasValue && tdsVoucher.Amount1 != 0)
            {
                var vd1 =tdsVoucher.VoucherDetails.FirstOrDefault(x=>x.OrderId==1)?? new VoucherDetail() { };
                vd1.AccountId = tdsVoucher.Account1Id.Value;
                vd1.Amount = tdsVoucher.Amount1;
                vd1.OrderId = 1;

                vd1.CurTypeId = tdsVoucher.CurTypeId;
                vd1.CurRate = tdsVoucher.CurRate;
                vd1.ConstCurTypeId = tdsVoucher.ConstCurTypeId;

                vd1.Constant1Id = view.TdsNatureId;
                vd1.Rate = view.TDSRate;
                vd1.Amount1 = Math.Abs(view.PrimaryCreditAmount);
                vd1.Account1Id = view.PrimaryCreditAccountId;
                var ledger = vdrflag.Where(x => x.Id == tdsVoucher.Account1Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification for TDS Voucher  :{tdsVoucher.Account1Id}");
                }
                vd1.OfficeId = ledger.OfficeId.Value == 0 ? tdsVoucher.OfficeId : ledger.OfficeId.Value;
                
                if (vd1.Id == 0)
                {
                    vd1.ObjectState = ObjectState.Added;
                    tdsVoucher.VoucherDetails.Add(vd1);
                }
                else
                {
                    vd1.ObjectState = ObjectState.Modified;
                }
                //if (ledger.ReferenceFlag)
                //{
                //    var vdr1 =vd1.VoucherDetailReferences.FirstOrDefault()?? new VoucherDetailReference();
                //    vdr1.Amount = vdr1.Amount;
                //    vdr1.ReferenceNo = tdsVoucher.VoucherNo;
                //    vdr1.VDRTypeId = 1013;
                //    vdr1.ObjectState = ObjectState.Added;
                //}
            }

            if (tdsVoucher.Account2Id.HasValue && tdsVoucher.Amount2 != 0)
            {
                var a2 = tdsVoucher.VoucherDetails.FirstOrDefault(x => x.OrderId == 2) ?? new VoucherDetail() { };
                a2.AccountId = tdsVoucher.Account2Id.Value;
                a2.Amount = tdsVoucher.Amount2;
                a2.OrderId = 2;

                a2.CurTypeId = tdsVoucher.CurTypeId;
                a2.CurRate = tdsVoucher.CurRate;
                a2.ConstCurTypeId = tdsVoucher.ConstCurTypeId;

                a2.Constant1Id = view.TdsNatureId;
                a2.Rate = view.TDSRate;
                a2.Amount1 = Math.Abs(view.PrimaryCreditAmount);
                a2.Account1Id = view.PrimaryCreditAccountId;
                var ledger = vdrflag.Where(x => x.Id == tdsVoucher.Account2Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{tdsVoucher.Account2Id}");
                }
                a2.OfficeId = ledger.OfficeId.Value == 0 ? tdsVoucher.OfficeId : ledger.OfficeId.Value;

                if (a2.Id == 0)
                {
                    a2.ObjectState = ObjectState.Added;
                    tdsVoucher.VoucherDetails.Add(a2);
                }
                else
                {
                    a2.ObjectState = ObjectState.Modified;
                }

                a2.VoucherDetailReferences.ForEach(x => x.ObjectState = ObjectState.Deleted);
                #region Prepare TDSVoucherVDR
                if (vdr != null)
                {

                    var vdr1 = new VoucherDetailReference()
                    {
                        Amount = a2.Amount,
                        ObjectState = ObjectState.Added,
                        ReferenceNo = vdr.ReferenceNo,
                        RefId = vdr.Id,
                        fk_ParentReference = vdr,
                        VDRTypeId = 1014,
                        CurTypeId = a2.CurTypeId,
                        CurRate = a2.CurRate,
                        ConstCurTypeId = a2.ConstCurTypeId
                    };
                    a2.VoucherDetailReferences = new List<VoucherDetailReference>() { vdr1 };
                }

                #endregion
            }

            #endregion

        }

        public SpareLogExtraInfo InsertOrUpdateMaterialMRNView(vwSparePurchaseBill view)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            var vRepo = _repository.GetRepository<Voucher>();
            long InventoryControlAcId;
            var GRNControlAcQuery =
                    _repository.GetRepository<ApiConfiguration>()
                        .Queryable()
                        .Where(x => x.Key == "InventoryControlAcId")
                        .Select(x => x.Value)
                        .FirstOrDefault();
            if (!long.TryParse(GRNControlAcQuery, out InventoryControlAcId))
            {
                throw new BusinessException(ErrorCode.GLB103,
                    "Inventory Control Account need to be configured.");
            }
            
            if (!_repository.GetRepository<Ledger>().Queryable().Any(x => x.Id == InventoryControlAcId && x.ReferenceFlag)) {
                throw new BusinessException(ErrorCode.GLB103,
                    "Inventory Control Account need to be configured Bill By Bill");
            }

            if (view.VoucherTypeId == 23) {
                view.ProvisionalAcId = view.PrimaryCreditAccountId;
                view.PrimaryCreditAccountId = InventoryControlAcId;
            }
            if (view.PrimaryCreditAccountId > 0 && view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account Ammount should always be negative.");
            }
            if (view.PrimaryDebitAccountId > 0 && view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account Ammount should always be greater than Zero");
            }
            var bill = _repository.Queryable().Include(x => x.fk_Bill).FirstOrDefault(x => x.ExtraInfoId == view.Id && x.BillExtraInfoId > 0);
            if (bill != null)
            {
                throw new BusinessException(ErrorCode.GLB105,
                    $"MRN has been referenced in Bill hence it cannot be updated BillNo:[{bill.fk_Bill.DocNo}]");
            }
            Voucher v = new Voucher();
            var seiRepo = _repository.GetRepository<SpareLogExtraInfo>();
            SpareLogExtraInfo sei = new SpareLogExtraInfo();
            if (view.Id > 0)
            {
                sei = seiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id);
                if (sei != null && sei.VoucherId > 0)
                {
                    v = vRepo.Queryable().Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).FirstOrDefault(x => x.Id == sei.VoucherId);
                }
            }
            if (sei == null)
            {
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }

            #region SpareLog Preparation

            var sps = _repository.Queryable().Where(x => x.ExtraInfoId == view.Id).ToList();
            
            var sparelogs = new List<SpareLog>();
            //Prepare DbModel for each View Model
            foreach (var a in view.Spares)
            {
                SpareLog s = new SpareLog();
                
                //If it is existing record try to fatch it from database
                if (view.Id > 0)
                {
                    s = sps.FirstOrDefault(x => x.Id == a.Id);
                    if (s == null && a.Id > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB106, $"One of transaction didn't found for update. Spare:'{ a.SpareName }'");
                    }

                    //If Spare Part has been issued against purchase or stock Transfer In then mark as Unchanged
                    if (s != null)
                    {
                        var count = _repository.Queryable().Any(x => x.ReferenceId == s.Id);
                        if (count)
                        {
                            sparelogs.Add(s); //No update
                            continue;
                            //throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Spare Part Information that has been referenced/issued.Ref:{a.SpareName}[{a.Id}]");
                        }
                    }
                }

                s=s ?? new SpareLog();
                s.ReferenceId = a.ReferenceId;
                s.UnitTypeId = a.UnitTypeId;
                s.Amount = a.Amount;
                s.OtherAmount = a.OtherAmount;
                s.VehicleId = a.VehicleId;
                s.HireVehicleId = a.HireVehicleId;
                s.Remark = a.Remark;
                s.Qty = a.Qty;
                s.DepositedQty = a.DepositedQty;
                s.SparePartId = a.SpareId;
                //s.VatAmount = a.VatAmount;
                s.TaxServiceTypeId = a.TaxServiceTypeId;
                s.CGSTAmount = a.CGSTAmount;
                s.SGSTAmount = a.SGSTAmount;
                s.IGSTAmount = a.IGSTAmount;
                s.Rate = a.Rate;
                s.UnitId = a.UnitId;
                s.SubTotal = a.Amount - a.DiscountAmount;
                s.DiscountAmount = a.DiscountAmount;
                s.DiscountPercent = a.DiscountPercent;
                s.MakeId = a.MakeId;
                s.BinId = a.BinId;
                s.NetAmount = a.NetAmount;
                s.POLogId = a.PurchaseId;
                //s.VatPercent = a.VatPercent;
                s.CGSTRate = a.CGSTRate;
                s.SGSTRate = a.SGSTRate;
                s.IGSTRate = a.IGSTRate;
                s.WarrantyDays = a.WarrantyDays;
                s.ODOKm = a.ODOKm;
                s.WarrantyKm = a.WarrantyKm;
                s.VoucherDate = view.DocumentDate;
                s.VoucherNo = view.DocumentNo;
                s.MechanicId = a.MechanicId;
                //if id is gt Zero Mark entity as Modified
                s.ObjectState = s.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                
                s.DrAccountId = view.PrimaryDebitAccountId;
                s.CrAccountId = view.PrimaryCreditAccountId;
                s.StockQty = s.Qty;
                s.VoucherTypeId = view.VoucherTypeId;

                if ((s.SubTotal + s.CGSTAmount + s.SGSTAmount + s.IGSTAmount + s.OtherAmount) != s.NetAmount) //vatAmount
                {
                    throw new BusinessException(ErrorCode.SPB100, $"Net Amount Doesn't tally with detail amounts for Spare Name:{a.SpareName}");
                }
               
                sparelogs.Add(s);
                if (a.JsonData != null)
                {
                    foreach (var entity in a.JsonData)
                    {
                        s.DeleteAndAdd(entity);
                    }
                }
            }
            //Prevent user from deleting Spare Log those have been issued/transfered from store
            var spareids = sparelogs.Where(x => x.Id > 0).Select(x => x.Id).ToList();
            

            if (view.Id > 0)
            {
                var deleted =
                _repository.Queryable()
                    .Include(x => x.IssuedLogs)
                    .Where(x => x.ExtraInfoId == view.Id && !spareids.Contains(x.Id));

                var omitedTransaction =
                    deleted.Any(x=>x.IssuedLogs.Any());
                if (omitedTransaction)
                {
                    throw new BusinessException(ErrorCode.GLB105, "Cannot Delete Spare Part Information that has been referenced/issued.");
                }
                //Delete All the SpareLogs that was mapped to this voucherid before but not now
                foreach (var x in deleted)
                {
                    x.ExtraInfoId = null;
                    x.ExtraInfo = null;
                    x.ObjectState = ObjectState.Deleted;
                    _repository.Delete(x);
                }
            }
            #endregion

            if (view.VoucherTypeId == 23)
            {
                #region Prepare Voucher

                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
                v.ConstCurTypeId = view.ConstCurTypeId;

                v.OfficeId = view.OfficeId;
                v.VoucherDate = view.DocumentDate;
                v.VoucherDateTime = view.DocumentDate;
                v.VoucherTypeId = view.VoucherTypeId.Value;
                v.VoucherNo = view.DocumentNo;

                v.Account1Id = view.PrimaryDebitAccountId;
                v.Amount1 = view.PrimaryDebitAmount + (view.RoundOffAcId.GetValueOrDefault() <= 0 ? view.RoundOff : 0);//Spare

                /*provisional account shall be credit inspite of vendor*/
                v.Account2Id = view.PrimaryCreditAccountId;
                v.Amount2 = view.PrimaryCreditAmount - (view.RoundOffAcId.GetValueOrDefault() <= 0 ? view.RoundOff : 0);//Party

                v.Account3Id = view.CGSTLedgerId;
                v.Amount3 = 0;

                v.Account4Id = view.SGSTLedgerId;
                v.Amount4 = 0;

                v.Account5Id = view.OtherLedgerId;
                v.Amount5 = view.OtherAmount;

                v.Account6Id = view.ExpenseLedgerId2;
                v.Amount6 = view.ExpenseAmount2 + (view.PrimaryDebitAmount > 0 || view.RoundOffAcId.GetValueOrDefault() <= 0 ? 0 : view.RoundOff);

                v.Account7Id = view.IGSTLedgerId;
                v.Amount7 = 0;

                v.Account8Id = view.TCSAccountId;
                v.Amount8 = view.TCSAmount;

                v.Account9Id = view.RoundOffAcId;
                v.Amount9 = view.RoundOffAcId > 0 ? view.RoundOff : 0;

                v.Account10Id = view.PostDiscountAcId;
                v.Amount10 = (view.PostDiscAmount >= 0 ? -1 : 1) * view.PostDiscAmount;


                v.UserRemark = view.Narration;
                v.AccountingRemark = "";
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                if (v.Id == 0)
                {
                    vRepo.Insert(v);
                }
                #endregion
            }
            #region Prepare SpareLogExtraInfo

            sei.CurTypeId = view.CurTypeId;
            sei.CurRate = view.CurRate;
            sei.ConstCurTypeId = view.ConstCurTypeId;

            sei.DocNo = view.DocumentNo;
            sei.PageId = view.PageId;
            sei.VoucherTypeId = view.VoucherTypeId;
            sei.OfficeId = view.OfficeId;
            sei.DocDate = view.DocumentDate;
            sei.CrAccountId =  view.PrimaryCreditAccountId;
            sei.DrAccountId = view.PrimaryDebitAccountId;
            
            sei.ProvisionalAcId = view.ProvisionalAcId;

            sei.IGSTACId = view.IGSTLedgerId;
            sei.CGSTACId = view.CGSTLedgerId;
            sei.SGSTACId = view.CGSTLedgerId;
            sei.VendorReferenceNo = view.VendorReferenceNo;
            sei.ObjectState = sei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            sei.Remark = view.Narration;
            sei.ChallanSlipNo = view.ChallanSlipNo;
            sei.ChallanSlipDate = view.ChallanSlipDate;
            sei.ViewId = view.ViewId;
            sei.TCSAmount = view.TCSAmount;
            sei.TCSAccountId = view.TCSAccountId;
            sei.TCSRate = view.TCSRate;
            sei.PostDiscountAcId = view.PostDiscountAcId;
            sei.PostDiscAmount = view.PostDiscAmount;
            sei.RoundOffAcId = view.RoundOffAcId;
            sei.RoundOff = view.RoundOff;
            if (view.VoucherTypeId == 23) {
                sei.fk_Voucher = v;
                sei.VoucherId = v.Id;
            }
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    sei.DeleteAndAdd(entity);
                }
            }
            if (sei.Id > 0) seiRepo.Update(sei);
            else seiRepo.Insert(sei);


            #endregion
            if (view.VoucherTypeId == 23)
            {
                PrepareVoucherDetails(_repository, v, view.VendorReferenceNo, sei);
            }

            foreach (SpareLog log in sparelogs)
            {
                log.ExtraInfoId = sei.Id;
                log.ExtraInfo = sei;
                if (sei.VoucherTypeId == 23)
                {
                    log.VoucherId = v.Id;
                    log.fk_Voucher = v;
                }

                if (log.Id > 0)
                {
                    _repository.Update(log);
                }
                else
                {
                    _repository.Insert(log);
                }
            }
            return sei;
        }

        public SpareLogExtraInfo InsertOrUpdateMaterialSettlementMRNView(vwSparePurchaseBill view)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;

            view.VoucherTypeId = 62;
            var vRepo = _repository.GetRepository<Voucher>();
            long InventoryControlAcId;
            
            var GRNControlAcQuery =
                    _repository.GetRepository<ApiConfiguration>()
                    .Queryable()
                    .Where(x => x.Key == "InventoryControlAcId")
                    .Select(x => x.Value)
                    .FirstOrDefault();

            if (!long.TryParse(GRNControlAcQuery, out InventoryControlAcId))
            {
                throw new BusinessException(ErrorCode.GLB103,
                    "Inventory Control Account need to be configured.");
            }
            if (!_repository.GetRepository<Ledger>().Queryable().Any(x => x.Id == InventoryControlAcId && x.ReferenceFlag))
            {
                throw new BusinessException(ErrorCode.GLB103,
                    "Inventory Control Account need to be configured Bill By Bill");
            }
            /*forcily debit accountis control account*/
            view.ProvisionalAcId = view.PrimaryDebitAccountId;
            view.PrimaryDebitAccountId = InventoryControlAcId;

            if (view.PrimaryCreditAccountId > 0 && view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account Amount should always be negative.");
            }
            if (view.PrimaryDebitAccountId > 0 && view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account Amount should always be greater than Zero");
            }
            Voucher v = new Voucher();
            var seiRepo = _repository.GetRepository<SpareLogExtraInfo>();
            SpareLogExtraInfo sei = new SpareLogExtraInfo();
            if (view.Id > 0)
            {
                sei = seiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id);
                if (sei != null&&sei.VoucherId>0)
                {
                    v = vRepo.Queryable().Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).FirstOrDefault(x => x.Id == sei.VoucherId);
                }
            }
            if (sei == null)
            {
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }

            #region SpareLog Preparation


            var slrepo = _repository.GetRepository<SpareLog>();
            var existingids= view.Spares.Select(x => x.Id).ToList();
            
            if (view.Id > 0)
            {
                var removespare = _repository.Queryable().Include(x => x.IssuedLogs)
                    .Where(x => x.BillExtraInfoId == view.Id && !existingids.Contains(x.Id));

                var omitedTransaction = removespare.Any(x => x.IssuedLogs.Any());
                if (omitedTransaction)
                {
                    throw new BusinessException(ErrorCode.GLB105, "Cannot Delete Spare Part Information that has been referenced/issued.");
                }
                foreach (var x in removespare)
                {
                    x.BillExtraInfoId = null;
                    x.fk_Bill = null;
                    x.ObjectState = ObjectState.Modified;
                    _repository.Update(x);
                }
            }
            #endregion
            #region Prepare Voucher

            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.ConstCurTypeId = view.ConstCurTypeId;

            v.OfficeId = view.OfficeId;
            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.Value;
            v.VoucherNo = view.DocumentNo;

            v.Account1Id = view.PrimaryDebitAccountId;
            v.Amount1 = view.PrimaryDebitAmount;//Spare

            v.Account2Id = view.PrimaryCreditAccountId;
            v.Amount2 = view.PrimaryCreditAmount;//Party

            v.Account3Id = view.CGSTLedgerId;
            v.Amount3 = view.CGSTAmount;

            v.Account4Id = view.SGSTLedgerId;
            v.Amount4 = view.SGSTAmount;

            v.Account5Id = view.OtherLedgerId;
            v.Amount5 = view.OtherAmount;

            v.Account6Id = view.ExpenseLedgerId2;
            v.Amount6 = 0;
            v.Account7Id = view.IGSTLedgerId;
            v.Amount7 = view.IGSTAmount;

            v.Account8Id = view.TCSAccountId;
            v.Amount8 = view.TCSAmount;

            v.Account9Id = view.RoundOffAcId;
            v.Amount9 =view.RoundOffAcId>0? view.RoundOff:0;

            v.Account10Id = view.PostDiscountAcId;
            v.Amount10 = (view.PostDiscAmount >= 0 ? -1 : 1) * view.PostDiscAmount;


            v.UserRemark = view.Narration;
            v.AccountingRemark = "";
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;

            if (v.Id == 0)
            {
                vRepo.Insert(v);
            }
            #endregion

            #region Prepare SpareLogExtraInfo


            sei.CurTypeId = view.CurTypeId;
            sei.CurRate = view.CurRate;
            sei.ConstCurTypeId = view.ConstCurTypeId;

            sei.DocNo = view.DocumentNo;
            sei.PageId = view.PageId;
            sei.VoucherTypeId = 62;
            sei.OfficeId = view.OfficeId;
            sei.DocDate = view.DocumentDate;
            sei.CrAccountId = view.PrimaryCreditAccountId;
            sei.DrAccountId = view.PrimaryDebitAccountId;
            sei.IGSTACId = view.IGSTLedgerId;
            sei.IGSTPercent = view.IGSTPercent;
            sei.IGSTAmount = view.IGSTAmount;
            sei.CGSTACId = view.CGSTLedgerId;
            sei.CGSTPercent = view.CGSTPercent;
            sei.CGSTAmount = view.CGSTAmount;
            sei.SGSTACId = view.SGSTLedgerId;
            sei.SGSTPercent = view.SGSTPercent;
            sei.SGSTAmount = view.SGSTAmount;
            sei.OtherAccountId = view.OtherLedgerId;
            sei.PostDiscountAcId = view.PostDiscountAcId;
            sei.PostDiscAmount = view.PostDiscAmount;
            sei.RoundOff = view.RoundOff;
            sei.RoundOffAcId = view.RoundOffAcId;
            sei.VendorReferenceNo = view.VendorReferenceNo;
            sei.ObjectState = sei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            sei.Remark = view.Narration;
            sei.ChallanSlipNo = view.ChallanSlipNo;
            sei.ChallanSlipDate = view.ChallanSlipDate;
            sei.ViewId = view.ViewId;
            sei.fk_Voucher = v;
            sei.VoucherId = v.Id;
            sei.TCSRate = view.TCSRate;
            sei.TCSAmount = view.TCSAmount;
            sei.TCSAccountId = view.TCSAccountId;
            sei.ProvisionalAcId = view.ProvisionalAcId;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    sei.DeleteAndAdd(entity);
                }
            }
            if (sei.Id > 0) seiRepo.Update(sei);
            else seiRepo.Insert(sei);
            #endregion
            
            var existinglogs = slrepo.Queryable().Where(x => existingids.Contains(x.Id)).ToList();
            List<VoucherDetailReference> vdrids = new List<VoucherDetailReference>();

            foreach (var x in existinglogs.GroupBy(p => p.VoucherId))
            {
                try
                {
                    var _vdrId = _repository.GetRepository<VoucherDetailReference>().Queryable().Where(k => k.fk_VoucherDetail.VoucherId == x.Key && k.fk_VoucherDetail.AccountId == InventoryControlAcId).FirstOrDefault();
                    if (_vdrId != null)
                    {
                        _vdrId.Amount = x.Sum(y => y.SubTotal);
                        vdrids.Add(_vdrId);
                    }
                }
                catch { }
            }

            foreach (SpareLog log in existinglogs)
            {
                log.BillExtraInfoId = sei.Id;
                log.fk_Bill = sei;
                log.ObjectState = ObjectState.Modified;

                if (log.Id > 0)
                {
                    _repository.Update(log);
                }
            }
            PrepareVoucherDetails(_repository, v, view.VendorReferenceNo, sei,vdrids);
            return sei;
        }

        public SpareLogExtraInfo InsertOrUpdateMaterialDeliveryChallanView(vwSparePurchaseBill view)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;

            if (view.PrimaryCreditAccountId == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Account is required");
            }
            if (view.PrimaryDebitAccountId == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Client Account is required");
            }
            var _TriplogNo = _repository.GetRepository<VehicleMovementLog>().Queryable()
                .Where(x => x.MaterialInvoiceId == view.Id)
                .Select(y=>y.TriplogNo)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(_TriplogNo))
            {
                throw new BusinessException(ErrorCode.GLB105,
                    $"Modify Failed: Delivery Challan is linked with TripNo:[{_TriplogNo}]");
            }

            var seiRepo = _repository.GetRepository<SpareLogExtraInfo>();
            SpareLogExtraInfo sei = new SpareLogExtraInfo();
            if (view.Id > 0)
            {
                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                sei = seiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id);
            }
            if (sei == null)
            {
                //Through error if voucher not found with provided vouchertypeid
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }

            #region SpareLog Preparation
            //Collect Distinct ReferenceId's from Posted SpareLogs
            var uniqeref = view.Spares.Select(x => x.ReferenceId).Distinct().ToList();
            //Fetch Stock Information for all above ReferenceId's 
            var stockinfo = _repository.Queryable()
                 .Where(x => uniqeref.Contains(x.Id)).Select(x => new
                 {
                     x.Id,
                     x.Qty,
                     x.DrAccountId,
                     //also include existing issued log other than current voucher for above ReferenceId's 
                     Issued = x.IssuedLogs.Where(b => b.ExtraInfoId != view.Id).Select(y => new { y.Id, y.Qty })
                 }).ToList();
            var sps = _repository.Queryable().Where(x => x.ExtraInfoId == view.Id).ToList();

            var sparelogs = new List<SpareLog>();
            //Prepare DbModel for each View Model
            foreach (var a in view.Spares)
            {
                SpareLog s = new SpareLog(); 

                //If it is existing record try to fatch it from database
                if (view.Id > 0)
                {
                    s = sps.FirstOrDefault(x => x.Id == a.Id);
                    if (s == null && a.Id > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB106, $"One of transaction didn't found for update. Spare:'{a.SpareName}'");
                    }

                    //If Spare Part has been issued against purchase or stock Transfer In then mark as Unchanged
                    if (s != null)
                    {
                        var count = _repository.Queryable().Any(x => x.ReferenceId == s.Id);
                        if (count)
                        {
                            sparelogs.Add(s); //No update
                            continue;
                            //throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Spare Part Information that has been referenced/issued.Ref:{a.SpareName}[{a.Id}]");
                        }
                    }
                }

                s = s ?? new SpareLog();
                
                s.Rate = a.Rate;
                s.Qty = a.Qty;                
                s.Amount = a.Amount;
                s.OtherAmount = a.OtherAmount;
                s.VehicleId = a.VehicleId;

                s.HireVehicleId = a.HireVehicleId;
                
                s.Remark = a.Remark;

                s.SparePartId = a.SpareId;
                s.TaxServiceTypeId = null;

                
                s.DepositedQty = a.DepositedQty;
                s.CGSTAmount = 0;
                s.SGSTAmount = 0;
                s.IGSTAmount = 0;               
                s.SubTotal = a.Amount;
                s.DiscountAmount = a.DiscountAmount;
                s.DiscountPercent = a.DiscountPercent;
                s.NetAmount = a.NetAmount;
                s.CGSTRate = 0;
                s.SGSTRate = 0;
                s.IGSTRate = 0;
                s.WarrantyDays = 0;
                s.ODOKm = 0;
                s.WarrantyKm = 0;
                s.VoucherDate = view.DocumentDate;
                s.VoucherNo = view.DocumentNo;

                s.ObjectState = s.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                s.DrAccountId = view.PrimaryDebitAccountId;
                s.CrAccountId = view.PrimaryCreditAccountId;
                
                s.VoucherTypeId = view.VoucherTypeId;

                s.StockQty = s.VoucherTypeId == 128 ? a.Qty : 0;
                if (!s.ReferenceId.HasValue && a.ReferenceId > 0)
                {
                    s.ReferenceId = a.ReferenceId;
                }

                if (s.ObjectState == ObjectState.Added || s.ObjectState == ObjectState.Modified)
                {
                    if (s.ReferenceId.HasValue && (view.VoucherTypeId == 128/*Inward*/ || view.VoucherTypeId == 129/*Outward*/))
                    {
                        var spi = stockinfo.FirstOrDefault(x => x.Id == s.ReferenceId);
                        if (spi == null)
                        {
                            throw new BusinessException(ErrorCode.SPB100, "Invalid Item selected for issue/transfer.=>ReferenceId");
                        }
                        if (s.CrAccountId != spi.DrAccountId) { 
                            throw new BusinessException(ErrorCode.SPB100, $"Selected Item has been Out of Stock.\n Item:{a.SpareName} Qty:{a.Qty}");
                        }

                        if ((view.VoucherTypeId == 128 || view.VoucherTypeId == 129) && (view.PrimaryDebitAccountId.HasValue && view.PrimaryCreditAccountId > 0 && view.PrimaryDebitAccountId.Value == view.PrimaryCreditAccountId))
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Sending & Receiving Parties should be different");
                        }
                        //SUM QtyIssued in other voucher+issued in this voucher
                        var issued = spi.Issued.Where(x => x.Id != s.Id).Sum(x => (decimal?)x.Qty).GetValueOrDefault(0) + sparelogs.Where(x => x.ReferenceId == s.ReferenceId).Sum(x => x.Qty);
                        var balance = (spi.Qty - issued);
                        if (balance == 0 || (balance != 0 && s.Qty > balance))
                        {
                            throw new BusinessException(ErrorCode.SPB102, new ValidationResult($"Issued item:{a.SpareName} exceded from available stock"));
                        }
                    }
                }

                sparelogs.Add(s);

                if (a.JsonData != null)
                {
                    foreach (var entity in a.JsonData)
                    {
                        s.DeleteAndAdd(entity);
                    }
                }
            }

            var spareids = sparelogs.Where(x => x.Id > 0).Select(x => x.Id).ToList();

            if (view.Id > 0)
            {
                var deleted =
                _repository.Queryable()
                    .Include(x => x.IssuedLogs)
                    .Where(x => x.ExtraInfoId == view.Id && !spareids.Contains(x.Id));

                var omitedTransaction =
                    deleted.Any(x => x.IssuedLogs.Any());

                if (omitedTransaction)
                {
                    throw new BusinessException(ErrorCode.GLB105, "Cannot delete record information that has been referenced/issued.");
                }
                
                foreach (var x in deleted)
                {
                    x.ExtraInfoId = null;
                    x.ExtraInfo = null;
                    x.ObjectState = ObjectState.Deleted;
                    _repository.Delete(x);
                }
            }
            #endregion
            if (stockinfo.Any())
            {
                //Less the issued items from stock
                var stockids = stockinfo.Select(m => m.Id).ToList();
                var stockinfos = _repository.Queryable().Where(x => stockids.Contains(x.Id)).ToList();
                foreach (var inf in stockinfo)
                {
                    var part = stockinfos.FirstOrDefault(x => x.Id == inf.Id);
                    var issued = inf.Issued.Where(x => !spareids.Contains(x.Id)).Sum(x => (decimal?)x.Qty).GetValueOrDefault(0) + sparelogs.Where(x => x.ReferenceId == inf.Id).Sum(x => x.Qty);
                    part.StockQty = part.Qty - issued;
                    part.ObjectState = ObjectState.Modified;
                    _repository.Update(part);
                }
            }
            #region Prepare SpareLogExtraInfo

            sei.CurTypeId = view.CurTypeId;
            sei.CurRate = view.CurRate;
            sei.ConstCurTypeId = view.ConstCurTypeId;

            sei.DocNo = view.DocumentNo;
            sei.PageId = view.PageId;
            sei.VoucherTypeId = view.VoucherTypeId;
            sei.OfficeId = view.OfficeId;
            sei.DocDate = view.DocumentDate;
            sei.CrAccountId = view.PrimaryCreditAccountId;
            sei.DrAccountId = view.PrimaryDebitAccountId;
            sei.VendorReferenceNo = view.VendorReferenceNo;
            sei.ObjectState = sei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            sei.Remark = view.Narration;

            sei.ChallanSlipNo = view.ChallanSlipNo;
            sei.ChallanSlipDate = view.ChallanSlipDate;
            sei.DrAmount = view.PrimaryDebitAmount;
            sei.CrAmount = view.PrimaryCreditAmount;
            sei.OtherAmount = view.OtherAmount;
            sei.ViewId = view.ViewId;
            sei.RoundOffAcId= view.RoundOffAcId;
            sei.TCSAmount = 0;
            sei.TCSRate = 0;           
            sei.PostDiscAmount = 0;
            sei.RoundOff = view.RoundOff;

            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    sei.DeleteAndAdd(entity);
                }
            }
            if (sei.Id > 0) seiRepo.Update(sei);
            else seiRepo.Insert(sei);


            #endregion

            foreach (SpareLog log in sparelogs)
            {
                log.ExtraInfoId = sei.Id;
                log.ExtraInfo = sei;


                if (log.Id > 0)
                {
                    _repository.Update(log);
                }
                else
                {
                    _repository.Insert(log);
                }
            }
            return sei;
        }

        private static void PrepareVoucherDetails(IRepository<SpareLog> repository, Voucher v, string vendorRefNo, SpareLogExtraInfo sei, List<VoucherDetailReference> againstrefvdrs = null)
        {

            foreach (VoucherDetail vd in v.VoucherDetails)
            {
                vd.ObjectState = ObjectState.Deleted;
                foreach (VoucherDetailReference reference in vd.VoucherDetailReferences)
                {
                    reference.ObjectState = ObjectState.Deleted;
                }
            }
            var ledgerRepo = repository.GetRepository<Ledger>().Queryable();
            var acids = new long[] { v.Account1Id ?? 0, v.Account2Id ?? 0, v.Account3Id ?? 0, v.Account4Id ?? 0, v.Account5Id ?? 0, v.Account6Id ?? 0, v.Account7Id ?? 0, v.Account8Id ?? 0, v.Account9Id ?? 0, v.Account10Id ?? 0 }.Where(x => x > 0).Distinct().ToArray();
            var offices = ledgerRepo.Where(x => acids.Contains(x.Id))
                .Select(x => new { x.Id,x.OfficeId, x.ReferenceFlag }).ToList();

            if (v.Account1Id.HasValue && v.Amount1 != 0)
            {
                var a1 = new VoucherDetail() { };
                a1.AccountId = v.Account1Id.Value;
                a1.Amount = v.Amount1;
                a1.OrderId = 1;

                a1.CurTypeId = v.CurTypeId;
                a1.CurRate = v.CurRate;
                a1.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account1Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account1Id}");
                }
                a1.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a1.ObjectState = ObjectState.Added;
                if (v.VoucherTypeId == 62)
                {
                    a1.Particular = "Control Account Reversed";
                }
                a1.Particular = "Spare Part Purchased";
                v.VoucherDetails.Add(a1);
                if (ledger.ReferenceFlag || v.VoucherTypeId == 62) { PrepareVDR(a1, string.IsNullOrWhiteSpace(vendorRefNo) ?v.VoucherNo: vendorRefNo, againstrefvdrs); }
            }
            if (v.Account2Id.HasValue && v.Amount2 != 0)
            {
                var a2 = new VoucherDetail() { };
                a2.AccountId = v.Account2Id.Value;
                a2.Amount = v.Amount2;
                a2.OrderId = 2;
                a2.Particular = "Spare Part Purchased";

                a2.CurTypeId = v.CurTypeId;
                a2.CurRate = v.CurRate;
                a2.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account2Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account2Id}");
                }
                a2.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a2.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a2);
                if (ledger.ReferenceFlag || v.VoucherTypeId == 62 || v.VoucherTypeId==23/*Mandatory incase of GRN*/) { PrepareVDR(a2, string.IsNullOrWhiteSpace(vendorRefNo) ? v.VoucherNo : vendorRefNo); }
            }
            if (v.Account3Id > 0 && v.Amount3!=0)
            {
                var a3 = new VoucherDetail() { };
                a3.AccountId = v.Account3Id.GetValueOrDefault(0);
                a3.Amount = v.Amount3;
                a3.OrderId = 3;

                a3.CurTypeId = v.CurTypeId;
                a3.CurRate = v.CurRate;
                a3.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account3Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account3Id}");
                }
                a3.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a3.ObjectState = ObjectState.Added;
                a3.Rate = sei.CGSTPercent;
                a3.TaxTypeId = sei.SpareLogs?.Where(x => x.TaxServiceTypeId > 0)?.FirstOrDefault()?.TaxServiceTypeId;
                a3.Particular = $"Spare Part Purchased of Rs.{v.Amount1+v.Amount6}";
                v.VoucherDetails.Add(a3);
                if (ledger.ReferenceFlag) { PrepareVDR(a3, string.IsNullOrWhiteSpace(vendorRefNo) ? v.VoucherNo : vendorRefNo); }
            }
            if (v.Account4Id > 0 && v.Amount4!=0)
            {
                var a4 = new VoucherDetail() { };
                a4.AccountId = v.Account4Id.GetValueOrDefault(0);
                a4.Amount = v.Amount4;
                a4.OrderId = 4;

                a4.CurTypeId = v.CurTypeId;
                a4.CurRate = v.CurRate;
                a4.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account4Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account4Id}");
                }
                a4.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a4.ObjectState = ObjectState.Added;
                a4.Rate = sei.SGSTPercent;
                a4.TaxTypeId = sei.SpareLogs?.Where(x => x.TaxServiceTypeId > 0)?.FirstOrDefault()?.TaxServiceTypeId;
                a4.Particular = $"Spare Part Purchased of Rs.{v.Amount1+v.Amount6}";
                v.VoucherDetails.Add(a4);
                if (ledger.ReferenceFlag) { PrepareVDR(a4, string.IsNullOrWhiteSpace(vendorRefNo) ? v.VoucherNo : vendorRefNo); }
            }
            if (v.Account5Id > 0 && v.Amount5!=0)
            {
                var a5 = new VoucherDetail() { };
                a5.AccountId = v.Account5Id.GetValueOrDefault(0);
                a5.Amount = v.Amount5;
                a5.OrderId = 5;

                a5.CurTypeId = v.CurTypeId;
                a5.CurRate = v.CurRate;
                a5.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account5Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account5Id}");
                }
                a5.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a5.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a5);
                if (ledger.ReferenceFlag) { PrepareVDR(a5, string.IsNullOrWhiteSpace(vendorRefNo) ? v.VoucherNo : vendorRefNo); }
            }
            if (v.Account6Id > 0 && v.Amount6!=0)
            {
                var a6 = new VoucherDetail() { };
                a6.AccountId = v.Account6Id.GetValueOrDefault(0);
                a6.Amount = v.Amount6;
                a6.OrderId = 6;

                a6.CurTypeId = v.CurTypeId;
                a6.CurRate = v.CurRate;
                a6.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account6Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account6Id}");
                }
                a6.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a6.Particular = $"Spare Part & Labour Purchased of Rs.{v.Amount1+v.Amount6}";
                a6.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a6);
                if (ledger.ReferenceFlag) { PrepareVDR(a6, string.IsNullOrWhiteSpace(vendorRefNo) ? v.VoucherNo : vendorRefNo); }
            }
            if(v.Account7Id>0 && v.Amount7!=0)
            {
                var a7 = new VoucherDetail() { };
                a7.AccountId = v.Account7Id.GetValueOrDefault(0);
                a7.Amount = v.Amount7;
                a7.OrderId = 7;

                a7.CurTypeId = v.CurTypeId;
                a7.CurRate = v.CurRate;
                a7.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account7Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if(ledger==null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger Identification :{v.Account7Id}");
                }
                a7.Rate = sei.IGSTPercent;
                a7.TaxTypeId = sei.SpareLogs?.Where(x => x.TaxServiceTypeId > 0)?.FirstOrDefault()?.TaxServiceTypeId;
                a7.Particular = $"Spare Part Purchased of Rs.{v.Amount1+v.Amount6}";
                a7.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a7.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a7);
                if (ledger.ReferenceFlag) { PrepareVDR(a7, string.IsNullOrWhiteSpace(vendorRefNo) ? v.VoucherNo : vendorRefNo); }
            }
            if (v.Account8Id > 0 && v.Amount8 != 0)
            {
                var a8 = new VoucherDetail() { };
                a8.AccountId = v.Account8Id.GetValueOrDefault(0);
                a8.Amount = v.Amount8;
                a8.OrderId = 8;
                a8.Rate = sei.TCSRate;

                a8.CurTypeId = v.CurTypeId;
                a8.CurRate = v.CurRate;
                a8.ConstCurTypeId = v.ConstCurTypeId;

                a8.Particular = $"Spare Part & Labour Purchased of Rs.{v.Amount1+v.Amount6}";
                var ledger = offices.Where(x => x.Id == v.Account8Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger Identification :{v.Account8Id}");
                }
                a8.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a8.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a8);
                if (ledger.ReferenceFlag) { PrepareVDR(a8, string.IsNullOrWhiteSpace(vendorRefNo) ? v.VoucherNo : vendorRefNo); }
            }
            if (v.Account9Id > 0 && v.Amount9 != 0)
            {
                var vd = new VoucherDetail() { };
                vd.AccountId = v.Account9Id.GetValueOrDefault(0);
                vd.Amount = v.Amount9;
                vd.OrderId = 9;

                vd.CurTypeId = v.CurTypeId;
                vd.CurRate = v.CurRate;
                vd.ConstCurTypeId = v.ConstCurTypeId;

                vd.Particular = $"Spare Part & Labour Purchased of Rs.{v.Amount1+v.Amount6}";
                var ledger = offices.Where(x => x.Id == v.Account9Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger Identification :{v.Account9Id}");
                }
                vd.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                vd.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(vd);
                if (ledger.ReferenceFlag) { PrepareVDR(vd, string.IsNullOrWhiteSpace(vendorRefNo) ? v.VoucherNo : vendorRefNo); }
            }
            if (v.Account10Id > 0 && v.Amount10 != 0)
            {
                var vd = new VoucherDetail() { };
                vd.AccountId = v.Account9Id.GetValueOrDefault(0);
                vd.Amount = v.Amount10;
                vd.OrderId = 10;

                vd.CurTypeId = v.CurTypeId;
                vd.CurRate = v.CurRate;
                vd.ConstCurTypeId = v.ConstCurTypeId;

                vd.Particular = $"Spare Part & Labour Purchased of Rs.{v.Amount1+v.Amount6}";
                var ledger = offices.Where(x => x.Id == v.Account10Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger Identification :{v.Account10Id}");
                }
                vd.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                vd.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(vd);
                if (ledger.ReferenceFlag) { PrepareVDR(vd, string.IsNullOrWhiteSpace(vendorRefNo) ? v.VoucherNo : vendorRefNo); }
            }
        }

        /// <summary>
        /// Batches the insert.
        /// </summary>
        /// <param name="docs">The docs.</param>
        /// <param name="transaction">The database transaction.</param>

        private static void PrepareVDR(VoucherDetail vd, string voucherNo, List<VoucherDetailReference> againstrefvdrs = null)
        {
            if (againstrefvdrs!=null && againstrefvdrs.Any()) {
                foreach (var avdr in againstrefvdrs.GroupBy(x => x.Id))
                {
                    var vdr = new VoucherDetailReference()
                    {
                        Amount = avdr.Sum(x => x.Amount),
                        ObjectState = ObjectState.Added,
                        ReferenceNo = avdr.FirstOrDefault().ReferenceNo,
                        VDRTypeId = 1014,
                        RefId = avdr.Key,
                        CurTypeId = vd.CurTypeId,
                        CurRate = vd.CurRate,
                        ConstCurTypeId = vd.ConstCurTypeId,
                        AccountId=vd.AccountId
                    };
                    vd.VoucherDetailReferences.Add(vdr);
                }
            }
            else
            {
                var vdr = new VoucherDetailReference()
                {
                    Amount = vd.Amount,
                    ObjectState = ObjectState.Added,
                    ReferenceNo = voucherNo,
                    VDRTypeId = 1013,
                    CurTypeId = vd.CurTypeId,
                    CurRate = vd.CurRate,
                    ConstCurTypeId = vd.ConstCurTypeId
                };
                vd.VoucherDetailReferences = new List<VoucherDetailReference>() { vdr };
            }            
        }

        /// <exception cref="BusinessException">Invalid VoucherId.</exception>
        public async Task DeleteGraph(long key)
        {
            var seiRepo = _repository.GetRepository<SpareLogExtraInfo>();
            var slRepo = _repository.GetRepository<SpareLog>();
            var lbRepo = _repository.GetRepository<RepairLabourLog>();
            var vpmRepo = _repository.GetRepository<VehiclePreventiveLog>();
            var sei =await seiRepo.Queryable().FirstOrDefaultAsync(x=>x.Id==key);

            var typeCanBeDeleted = new List<long?>() { 22, 23, 24, 25, 26, 61, 62, 125, 126, 128,129,130, };
            var stockAffectingType = new List<long>() { 24, 25, 26,125, 128,129,130 };

            if (sei == null)
            {
                throw new BusinessException(ErrorCode.VCH108);
            }
            if (!typeCanBeDeleted.Contains(sei.VoucherTypeId))
            {
                throw new BusinessException(ErrorCode.VCH108, "Cannot proceed this action from this resource");
            }
            if (await slRepo.Queryable().AnyAsync(x => x.IssuedLogs.Any() && x.ExtraInfoId == sei.Id))
            {
                throw new BusinessException(ErrorCode.SPB103);
            }
            if (stockAffectingType.Contains(sei.VoucherTypeId.GetValueOrDefault()))
            {
                await
                    slRepo.UOW.ExecSqlQueryAsync(
                        $"update b set b.StockQty=b.StockQty+a.Qty from tSpareLog a left join tSpareLog b on a.RefId=b.Id where a.ExtraInfoId={sei.Id}");
                
            }
            if (sei.VoucherId.GetValueOrDefault()>0)
            {
                var vrepo = _repository.GetRepository<Voucher>();
                var vdrrepo = _repository.GetRepository<VoucherDetailReference>();
                //var acstatus = Helper.GetFinanceStatus();
                var voucher =await vrepo.Queryable().FirstOrDefaultAsync(x=>x.Id== sei.VoucherId);
                Voucher tdsvoucher = null;
                if (sei.TDSVoucherId>0)
                {
                    tdsvoucher = await vrepo.Queryable().FirstOrDefaultAsync(x => x.Id == sei.TDSVoucherId);

                    tdsvoucher.ObjectState = ObjectState.Deleted;
                }
                if (voucher.IsAudited)
                {
                    throw new BusinessException(ErrorCode.VCH102);
                }
                //if (voucher.IsAccepted && acstatus == FinanceStatus.ApprovalRequired)
                //{
                //    throw new BusinessException(ErrorCode.VCH101);
                //}
                var tdsvoucherid = tdsvoucher?.Id ?? 0;
                if (await vdrrepo.Queryable().AnyAsync(x => x.AgainstReferences.Any(y=>y.fk_VoucherDetail.VoucherId!= tdsvoucherid) && x.fk_VoucherDetail.VoucherId == voucher.Id))
                {
                    throw new BusinessException(ErrorCode.VCH103);
                }
               //await seiRepo.Queryable().Where(x=>x.Id==sei.Id).UpdateAsync(x => new SpareLogExtraInfo() {VoucherId = null, TDSVoucherId=null});
                
               voucher.ObjectState=ObjectState.Deleted;
                await _repository.ExecuteSqlAsync($"DELETE vdr FROM [dbo].[tVoucherVDR] vdr JOIN [dbo].[tVoucherVD] vd on vdr.VDId=vd.Id WHERE vd.VoucherId in({tdsvoucherid},{voucher?.Id ?? 0})");
            }
            if (sei.VoucherTypeId != 62)
            {
                await
                        slRepo.Queryable()
                            .Where(x => x.ExtraInfoId == sei.Id)
                            .DeleteAsync();
                await
                        lbRepo.Queryable()
                            .Where(x => x.ExtraInfoId == sei.Id)
                            .DeleteAsync();

                await vpmRepo.Queryable().Where(x => x.BillId == sei.Id).DeleteAsync();
            }
            if(sei.VoucherTypeId==62)
            {
                await slRepo.UOW.ExecSqlQueryAsync(
                $"update sl set sl.BillExtraInfoId=null from [dbo].[tSpareLog] sl join [dbo].[tSpareLogExtraInfo] ei on ei.Id=sl.BillExtraInfoId where ei.Id={sei.Id}");
            }
            sei.ObjectState = ObjectState.Deleted;
        }
        
        public void BatchInsert(List<vwSparePurchaseBill> docs, IDbTransaction transaction)
        {
            if (docs.Any(x => x.Spares == null || x.Spares.Count <= 0) && docs.Any(x => x.Labors == null || x.Labors.Count <= 0)) throw new BusinessException(ErrorCode.GLB106, "One of Voucher does not have Advance Details");
            var vs = new List<Voucher>();
            var vds = new List<VoucherDetail>();
            var vdrs = new List<VoucherDetailReference>();
            var sparelogextrainfo = new List<SpareLogExtraInfo>();
            var spareloglist = new List<SpareLog>();
            var labourlist = new List<RepairLabourLog>();
            var acids = docs.Select(x => x.PrimaryDebitAccountId).Union(docs.Select(x => x.PrimaryDebitAccountId)).Union(docs.Select(x => x.IGSTLedgerId)).Union(docs.Select(x => x.SGSTLedgerId)).Union(docs.Select(x => x.TDSLedgerId)).Union(docs.Select(x => x.CGSTLedgerId)).Where(x=>x.GetValueOrDefault()>0).Select(x=>(long)x).Distinct().ToList();
            var acrefs = _repository.GetRepository<Ledger>().Queryable().AsNoTracking().Select(x => new { x.Id, x.ReferenceFlag,x.OfficeId }).Where(x => acids.Contains(x.Id)).ToList();
            var doe = DateTime.Now;
            var sessionid=Helper.SessionId();
            var fys = _repository.GetRepository<FinancialYear>().Queryable().ToList();
            var CT = Helper.ConstCurTypeId;
            foreach (var doc in docs)
            {
                doc.ConstCurTypeId = CT;
                var fy = fys.FirstOrDefault(x => x.OpeningDate.Date <= doc.DocumentDate.Date && x.ClosingDate.Date >= doc.DocumentDate.Date);
                if (fy == null)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Financial Year not found for DocumentNo:[{doc.DocumentNo}]");
                }
                if (fy.IsLocked)
                {
                    throw new BusinessException(ErrorCode.GLB106,"Finalcial Year locked.");
                }
                var batchid = Guid.NewGuid().ToString("N");
                var sle=new SpareLogExtraInfo { PageId = doc.PageId,BatchId= batchid };
                var vch = new Voucher
                {
                    BatchId = batchid,
                    VoucherNo = doc.DocumentNo,

                    CurTypeId = doc.CurTypeId,
                    CurRate = doc.CurRate,
                    ConstCurTypeId = doc.ConstCurTypeId
                };
                Voucher tdsvoucher = new Voucher()
                {
                    BatchId = batchid,
                    VoucherNo = $"TDS-{doc.DocumentNo}",

                    CurTypeId = doc.CurTypeId,
                    CurRate = doc.CurRate,
                    ConstCurTypeId = doc.ConstCurTypeId
                };
                if (doc.VoucherTypeId > 0)
                {
                    sle.VoucherTypeId = doc.VoucherTypeId;
                    vch.VoucherTypeId = doc.VoucherTypeId.GetValueOrDefault();
                }
                else
                {
                    doc.VoucherTypeId = 22;/*Direct Consumption*/
                    sle.VoucherTypeId = 22;/*Direct Consumption*/
                    vch.VoucherTypeId = 22;/*Direct Consumption*/
                }
                
                var sparestotalAmt = Math.Round(doc.Spares.Sum(x => x.SubTotal), 2);
                var laborstotalAmt = Math.Round(doc.Labors.Sum(x => x.SubTotal), 2);
                #region Spare Extra info

                sle = new SpareLogExtraInfo
                {
                    Id = doc.Id,

                    CurTypeId=doc.CurTypeId,
                    CurRate=doc.CurRate,
                    ConstCurTypeId=doc.ConstCurTypeId,

                    TypeId = doc.TypeId,
                    OfficeId = doc.OfficeId,
                    DocDate = doc.DocumentDate,
                    DocNo = doc.DocumentNo,
                    CrAccountId = doc.PrimaryCreditAccountId, //VendorId
                    DrAccountId = doc.PrimaryDebitAccountId, //drSpareAccountId
                    ProvisionalAcId = doc.ExpenseLedgerId2, //drLabourAccountId
                    OtherAccountId=doc.OtherLedgerId,
                    CGSTACId=doc.CGSTLedgerId,
                    SGSTACId=doc.SGSTLedgerId,
                    IGSTACId=doc.IGSTLedgerId,
                    VendorReferenceNo = string.IsNullOrEmpty(doc.VendorReferenceNo) ? doc.DocumentNo : doc.VendorReferenceNo,
                    Remark = doc.Narration,
                    CalculateVat = doc.CalVat,
                    VoucherTypeId = vch.VoucherTypeId,
                    VoucherId = vch.Id,
                    ViewId = doc.ViewId,
                    CreatedDOE = doe,
                    CreatedSessionId = sessionid,
                    BatchId = batchid,
                    TCSAccountId=doc.TCSAccountId,
                    TCSAmount=doc.TCSAmount,
                    TCSRate=doc.TCSRate,
                    TDSAccountId=doc.TDSLedgerId,
                    TDSRate=doc.TDSRate,
                    TDSAmount=doc.TDSAmount,
                    SGSTAmount=doc.SGSTAmount,
                    CGSTAmount=doc.CGSTAmount,
                    IGSTAmount=doc.IGSTAmount,
                    IGSTPercent=doc.IGSTPercent,
                    SGSTPercent = doc.SGSTPercent,
                    CGSTPercent = doc.CGSTPercent,
                    CrAmount=doc.PrimaryCreditAmount,
                    DrAmount=doc.PrimaryDebitAmount,
                    RoundOff=doc.RoundOff,
                    OtherAmount=doc.OtherAmount,
                    VehicleId=doc.VehicleId,
                    HireVehicleId = doc.HireVehicleId,
                    JsonData = doc.JsonData!=null?JsonConvert.SerializeObject(doc.JsonData):null,
                    Qty = doc.Qty,
                    GatepassNo = doc.GatepassNo,
                    ChallanSlipDate = doc.ChallanSlipDate,
                    ChallanSlipNo = doc.ChallanSlipNo,
                    GroupVoucherId = doc.GroupVoucherId,
                    PageId = doc.PageId,
                    PostDiscAmount = doc.PostDiscAmount,
                    RelatedVoucherId = doc.RelatedVoucherId,
                    OtherChargeRatioId = doc.OtherChargeRatioId,
                    PostDiscountAcId = doc.PostDiscountAcId,
                    RoundOffAcId = doc.RoundOffAcId,
                    TDSVoucherId = doc.TDSVoucherId
                };
                sle.Data = doc.JsonData;
                sparelogextrainfo.Add(sle);
                #endregion
                for (var i = 0; i < doc.Spares.Count; i++)
                {
                    var ad = doc.Spares.ElementAt(i);
                    ad.Qty = (ad.Qty <= 0 ? 1 : ad.Qty);
                    ad.Amount = (ad.Amount <= 0 ? 1 : ad.Amount);
                    ad.Rate = (ad.Rate <= 0 ? (ad.Amount/ ad.Qty) : ad.Rate);
                    var spl = new SpareLog
                    {
                        Id = ad.Id,
                        VehicleId = ad.VehicleId,
                        HireVehicleId=ad.HireVehicleId,
                        SparePartId = ad.SpareId,
                        POLogId = ad.PurchaseId,
                        Qty = ad.Qty,
                        Rate = ad.Rate,
                        Amount = ad.Amount,
                        DiscountPercent = ad.DiscountPercent,
                        DiscountAmount = ad.DiscountAmount,
                        //VatPercent = ad.VatPercent,
                        //VatAmount = ad.VatAmount,
                        SubTotal=ad.SubTotal,
                        TaxServiceTypeId=ad.TaxServiceTypeId,
                        CGSTRate = ad.CGSTRate,
                        CGSTAmount = ad.CGSTAmount,
                        SGSTRate = ad.SGSTRate,
                        SGSTAmount = ad.SGSTAmount,
                        IGSTRate = ad.IGSTRate,
                        IGSTAmount = ad.IGSTAmount,
                        //NetAmount=ad.Amount+ad.VatAmount,
                        NetAmount = ad.NetAmount,
                        WarrantyKm = ad.WarrantyKm,
                        ODOKm=ad.ODOKm,
                        WarrantyDays = ad.WarrantyDays,
                        MechanicId = ad.MechanicId,
                        Remark = ad.Remark,
                        ExtraInfo = sle,
                        VoucherTypeId = sle.VoucherTypeId,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        BatchId = batchid,
                        VoucherDate = doc.DocumentDate,
                        CrAccountId=sle.CrAccountId,
                        DrAccountId=sle.DrAccountId,
                        VoucherNo=sle.DocNo,
                        Data=ad.JsonData,
                        JsonData= JsonConvert.SerializeObject(ad.JsonData),
                        OtherAmount=ad.OtherAmount,
                        PostDisount=ad.PostDisount,
                        JobCardId=ad.JobCardId,
                        ReferenceId=ad.ReferenceId,
                        RoundOff=ad.RoundOff,
                        UnitId=ad.UnitId,
                        BinId = ad.BinId,
                        DepositedQty = ad.DepositedQty,
                        MakeId = ad.MakeId,
                        StockQty =ad.StockQty,
                        VoucherId = ad.VoucherId,
                        FittingPositionId = ad.FittingPositionId
                    };
                    spareloglist.Add(spl);
                }
                for (int i = 0; i < doc.Labors.Count; i++)
                {
                    var ad = doc.Labors.ElementAt(i);
                    var rll = new RepairLabourLog
                    {
                        Id = ad.Id,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        VehicleId = ad.VehicleId,
                        HireVehicleId = ad.HireVehicleId,
                        MechanicId = ad.MechanicId,
                        LaborId = ad.LaborId,
                        LaborQty = ad.LaborQty,
                        LaborRate = ad.LaborRate,
                        Amount = ad.Amount,
                        //ServiceTaxAmount = 0,
                        TaxServiceTypeId=ad.GSTServiceTypeId,
                        SubTotal = ad.SubTotal,
                        CGSTPercent=ad.LCCGSTPercent,
                        CGSTAmount = ad.LCCGSTAmount,
                        SGSTPercent=ad.LCSGSTPercent,
                        SGSTAmount = ad.LCSGSTAmount,
                        IGSTPercent=ad.LCIGSTPercent,
                        IGSTAmount = ad.LCIGSTAmount,
                        ODOKm=ad.ODOKm,
                        NetAmount = ad.NetAmount,
                        Remark = ad.Remark,
                        ExtraInfo = sle,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        BatchId = batchid,
                        DiscountAmount=ad.DiscountAmount,
                        DiscountPercent=ad.DiscountPercent,
                        ExtraInfoId=sle.Id,         
                       JobCardId=ad.JobCardId,
                        Value1 = ad.Value1,
                        Value2 = ad.Value2,
                        JsonData = ad._JsonData,
                        OtherAmount = ad.OtherAmount,
                        LaborUnitId = ad.LaborUnitId,
                        POLogId = ad.PurchaseOrderId
                    };
                    labourlist.Add(rll);
                }


                #region Main Voucher, VDs & VDRs

                #region Voucher

                vch.OfficeId = doc.OfficeId;
                vch.VoucherNo = doc.DocumentNo;
                vch.VoucherDate = doc.DocumentDate;
                vch.VoucherDateTime = doc.DocumentDate;
                vch.ObjectState = vch.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                
                vch.VoucherTypeId = doc.VoucherTypeId.GetValueOrDefault();
                vch.Account1Id = doc.PrimaryCreditAccountId; //Vender
                vch.Account2Id = doc.PrimaryDebitAccountId; //DrSpareAccount
                vch.Account6Id = doc.ExpenseLedgerId2; //drLabour
                vch.Account7Id = doc.TCSAccountId;

                vch.IsAccepted = true;
                vch.IsAccountsVisiblity = true;
                vch.FinancialYearId = fy.Id;
                vch.UserRemark = doc.Narration;
                vch.AccountingRemark = "";
                vch.BatchId = doc.BatchId = batchid;
                vch.ViewId = doc.ViewId;
                vch.CreatedSessionId = sessionid;
                vch.CreatedDOE = doe;
                var lbcgst = doc.Labors.Sum(x => x.LCCGSTAmount);
                var lbsgst = doc.Labors.Sum(x => x.LCSGSTAmount);
                var lbigst = doc.Labors.Sum(x => x.LCIGSTAmount);
                var labourgst = lbcgst + lbsgst + lbigst;

                var spcgst = doc.Spares.Sum(x => x.CGSTAmount);
                var spsgst = doc.Spares.Sum(x => x.SGSTAmount);
                var spigst = doc.Spares.Sum(x => x.IGSTAmount);
                var sparegst = spcgst + spsgst + spigst;

                if (doc.CalVat)
                {
                    vch.VoucherAmount = sparestotalAmt + laborstotalAmt+ labourgst+ sparegst;
                    vch.Amount1 = -(vch.VoucherAmount);
                    vch.Amount2 = sparestotalAmt;
                    vch.Amount6 = laborstotalAmt;


                    vch.Amount3 = lbcgst+ spcgst;
                    vch.Account3Id = doc.CGSTLedgerId;                    

                    vch.Amount4 = lbsgst+ spsgst;
                    vch.Account4Id = doc.SGSTLedgerId;

                    vch.Amount5 = lbigst+ spigst;
                    vch.Account5Id = doc.IGSTLedgerId;
                }
                else
                {
                    vch.VoucherAmount = sparestotalAmt + laborstotalAmt + labourgst + sparegst;
                    vch.Amount1 = -(vch.VoucherAmount);
                    vch.Amount2 = sparestotalAmt+ sparegst;
                    vch.Amount6 = laborstotalAmt+ labourgst;
                }
                vch.Amount8 = doc.TCSAmount;
                vs.Add(vch);

                #endregion

                #region VD-1=Account1Id
                VoucherDetailReference vendorvdr = null;
                if (vch.VoucherDetails == null)
                {
                    vch.VoucherDetails = new List<VoucherDetail>();
                }
                if (vch.Account1Id > 0 && vch.Amount1 != 0)
                {
                    var vd1 = new VoucherDetail
                    {
                        OfficeId =vch.OfficeId,
                        AccountId = vch.Account1Id.Value,
                        OrderId = 1,
                        Amount = vch.Amount1,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        BatchId = vch.BatchId,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        CurTypeId = doc.CurTypeId,
                        CurRate = doc.CurRate,
                        ConstCurTypeId = doc.ConstCurTypeId
                    };
                    vch.VoucherDetails.Add(vd1);
                    vds.Add(vd1);
                    
                    if (vd1.VoucherDetailReferences == null) vd1.VoucherDetailReferences = new List<VoucherDetailReference>();
                    var acflag = acrefs.FirstOrDefault(x => x.Id == vd1.AccountId);
                    vd1.OfficeId = acflag?.OfficeId ?? vd1.OfficeId;
                    if (acflag?.ReferenceFlag??false)
                    {
                        vendorvdr = new VoucherDetailReference
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd1.Amount,
                            ReferenceNo = sle.VendorReferenceNo,
                            VDRTypeId = 1013,
                            VoucherDetailId = 0,
                            fk_VoucherDetail = vd1,
                            BatchId = batchid,
                            CreatedDOE = doe,
                            CreatedSessionId = sessionid,
                            DueDate = vch.VoucherDate,
                            CurTypeId = doc.CurTypeId,
                            CurRate = doc.CurRate,
                            ConstCurTypeId = doc.ConstCurTypeId
                        };
                        vd1.VoucherDetailReferences.Add(vendorvdr);
                        vdrs.Add(vendorvdr);
                    }
                }
                #endregion

                #region VD-2=Account2Id
                if (vch.Account2Id > 0 && vch.Amount2 != 0)
                {
                    var vd2 = new VoucherDetail
                    {
                        OfficeId = vch.OfficeId,
                        AccountId = vch.Account2Id.Value,
                        OrderId = 2,
                        Amount = vch.Amount2,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        BatchId = vch.BatchId,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        CurTypeId = doc.CurTypeId,
                        CurRate = doc.CurRate,
                        ConstCurTypeId = doc.ConstCurTypeId
                    };
                    vch.VoucherDetails.Add(vd2);
                    vds.Add(vd2);
                    if (vd2.VoucherDetailReferences == null)
                        vd2.VoucherDetailReferences = new List<VoucherDetailReference>();
                    //var lRepo = _repository.GetRepository<Ledger>().Queryable();
                    var acflag = acrefs.FirstOrDefault(x => x.Id == vd2.AccountId);
                    vd2.OfficeId = acflag?.OfficeId ?? vd2.OfficeId;
                    if (acflag?.ReferenceFlag??false)
                    {
                        var vdr = new VoucherDetailReference
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd2.Amount,
                            ReferenceNo = sle.VendorReferenceNo,
                            VDRTypeId = 1013,
                            VoucherDetailId = 0,
                            fk_VoucherDetail = vd2,
                            BatchId = batchid,
                            CreatedDOE = doe,
                            CreatedSessionId = sessionid,
                            DueDate = vch.VoucherDate,
                            CurTypeId = doc.CurTypeId,
                            CurRate = doc.CurRate,
                            ConstCurTypeId = doc.ConstCurTypeId
                        };
                        vd2.VoucherDetailReferences.Add(vdr);
                        vdrs.Add(vdr);
                    }
                }
                #endregion

                #region VD-3=Account6Id
                if (vch.Account6Id > 0 && vch.Amount6 != 0)
                {
                    var vd3 = new VoucherDetail
                    {
                        OfficeId = vch.OfficeId,
                        AccountId = vch.Account6Id.Value,
                        OrderId = 3,
                        Amount = vch.Amount6,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        BatchId = vch.BatchId,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        CurTypeId = doc.CurTypeId,
                        CurRate = doc.CurRate,
                        ConstCurTypeId = doc.ConstCurTypeId
                    };
                    vch.VoucherDetails.Add(vd3);
                    vds.Add(vd3);
                    if (vd3.VoucherDetailReferences == null)
                        vd3.VoucherDetailReferences = new List<VoucherDetailReference>();
                    //var lRepo = _repository.GetRepository<Ledger>().Queryable();
                    var isRefEnabled3 = acrefs.Any(x => x.Id == vd3.AccountId && x.ReferenceFlag);
                    if (isRefEnabled3)
                    {
                        var vdr = new VoucherDetailReference
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd3.Amount,
                            ReferenceNo = sle.VendorReferenceNo,
                            VDRTypeId = 1013,
                            VoucherDetailId = 0,
                            fk_VoucherDetail = vd3,
                            BatchId = batchid,
                            CreatedDOE = doe,
                            CreatedSessionId = sessionid,
                            DueDate = vch.VoucherDate,
                            CurTypeId = doc.CurTypeId,
                            CurRate = doc.CurRate,
                            ConstCurTypeId = doc.ConstCurTypeId
                        };
                        vd3.VoucherDetailReferences.Add(vdr);
                        vdrs.Add(vdr);
                    }
                }
                #endregion
                #region VD-4=Account3Id
                if (vch.Account3Id > 0 && vch.Amount3 != 0)
                {
                    var vd4 = new VoucherDetail
                    {
                        OfficeId = vch.OfficeId,
                        AccountId = vch.Account3Id.Value,
                        OrderId = 4,
                        Amount = vch.Amount3,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        BatchId = vch.BatchId,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        CurTypeId = doc.CurTypeId,
                        CurRate = doc.CurRate,
                        ConstCurTypeId = doc.ConstCurTypeId
                    };
                    vch.VoucherDetails.Add(vd4);
                    vds.Add(vd4);
                    if (vd4.VoucherDetailReferences == null)
                        vd4.VoucherDetailReferences = new List<VoucherDetailReference>();
                    //var lRepo = _repository.GetRepository<Ledger>().Queryable();
                    var isRefEnabled3 = acrefs.Any(x => x.Id == vd4.AccountId && x.ReferenceFlag);
                    if (isRefEnabled3)
                    {
                        var vdr = new VoucherDetailReference
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd4.Amount,
                            ReferenceNo = sle.VendorReferenceNo,
                            VDRTypeId = 1013,
                            VoucherDetailId = 0,
                            fk_VoucherDetail = vd4,
                            BatchId = batchid,
                            CreatedDOE = doe,
                            CreatedSessionId = sessionid,
                            DueDate = vch.VoucherDate,
                            CurTypeId = doc.CurTypeId,
                            CurRate = doc.CurRate,
                            ConstCurTypeId = doc.ConstCurTypeId
                        };
                        vd4.VoucherDetailReferences.Add(vdr);
                        vdrs.Add(vdr);
                    }
                }
                #endregion
                #region VD-5=Account4Id
                if (vch.Account4Id > 0 && vch.Amount4 != 0)
                {
                    var vd5 = new VoucherDetail
                    {
                        OfficeId = vch.OfficeId,
                        AccountId = vch.Account4Id.Value,
                        OrderId = 5,
                        Amount = vch.Amount4,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        BatchId = vch.BatchId,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        CurTypeId = doc.CurTypeId,
                        CurRate = doc.CurRate,
                        ConstCurTypeId = doc.ConstCurTypeId
                    };
                    vch.VoucherDetails.Add(vd5);
                    vds.Add(vd5);
                    if (vd5.VoucherDetailReferences == null)
                        vd5.VoucherDetailReferences = new List<VoucherDetailReference>();
                    //var lRepo = _repository.GetRepository<Ledger>().Queryable();
                    var isRefEnabled3 = acrefs.Any(x => x.Id == vd5.AccountId && x.ReferenceFlag);
                    if (isRefEnabled3)
                    {
                        var vdr = new VoucherDetailReference
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd5.Amount,
                            ReferenceNo = sle.VendorReferenceNo,
                            VDRTypeId = 1013,
                            VoucherDetailId = 0,
                            fk_VoucherDetail = vd5,
                            BatchId = batchid,
                            CreatedDOE = doe,
                            CreatedSessionId = sessionid,
                            DueDate = vch.VoucherDate
                        };
                        vd5.VoucherDetailReferences.Add(vdr);
                        vdrs.Add(vdr);
                    }
                }
                #endregion
                #region VD-6=Account5Id
                if (vch.Account5Id > 0 && vch.Amount5 != 0)
                {
                    var vd = new VoucherDetail
                    {
                        OfficeId = vch.OfficeId,
                        AccountId = vch.Account5Id.Value,
                        OrderId = 6,
                        Amount = vch.Amount5,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        BatchId = vch.BatchId,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        CurTypeId = doc.CurTypeId,
                        CurRate = doc.CurRate,
                        ConstCurTypeId = doc.ConstCurTypeId
                    };
                    vch.VoucherDetails.Add(vd);
                    vds.Add(vd);
                    if (vd.VoucherDetailReferences == null)
                        vd.VoucherDetailReferences = new List<VoucherDetailReference>();
                    //var lRepo = _repository.GetRepository<Ledger>().Queryable();
                    var isRefEnabled3 = acrefs.Any(x => x.Id == vd.AccountId && x.ReferenceFlag);
                    if (isRefEnabled3)
                    {
                        var vdr = new VoucherDetailReference
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd.Amount,
                            ReferenceNo = sle.VendorReferenceNo,
                            VDRTypeId = 1013,
                            VoucherDetailId = 0,
                            fk_VoucherDetail = vd,
                            BatchId = batchid,
                            CreatedDOE = doe,
                            CreatedSessionId = sessionid,
                            DueDate = vch.VoucherDate,
                            CurTypeId = doc.CurTypeId,
                            CurRate = doc.CurRate,
                            ConstCurTypeId = doc.ConstCurTypeId
                        };
                        vd.VoucherDetailReferences.Add(vdr);
                        vdrs.Add(vdr);
                    }
                }

                #endregion
                #region VD-7 = Account7Id
                if (vch.Account7Id > 0 && vch.Amount7 != 0)
                {
                    var vd = new VoucherDetail
                    {
                        OfficeId = vch.OfficeId,
                        AccountId = vch.Account7Id.Value,
                        OrderId = 7,
                        Amount = vch.Amount7,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        BatchId = vch.BatchId,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        CurTypeId = doc.CurTypeId,
                        CurRate = doc.CurRate,
                        ConstCurTypeId = doc.ConstCurTypeId
                    };
                    vch.VoucherDetails.Add(vd);
                    vds.Add(vd);
                    if (vd.VoucherDetailReferences == null)
                        vd.VoucherDetailReferences = new List<VoucherDetailReference>();
                    //var lRepo = _repository.GetRepository<Ledger>().Queryable();
                    var isRefEnabled3 = acrefs.Any(x => x.Id == vd.AccountId && x.ReferenceFlag);
                    if (isRefEnabled3)
                    {
                        var vdr = new VoucherDetailReference
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd.Amount,
                            ReferenceNo = sle.VendorReferenceNo,
                            VDRTypeId = 1013,
                            VoucherDetailId = vd.Id,
                            fk_VoucherDetail = vd,
                            BatchId = batchid,
                            CreatedDOE = doe,
                            CreatedSessionId = sessionid,
                            DueDate = vch.VoucherDate,
                            CurTypeId = doc.CurTypeId,
                            CurRate = doc.CurRate,
                            ConstCurTypeId = doc.ConstCurTypeId
                        };
                        vd.VoucherDetailReferences.Add(vdr);
                        vdrs.Add(vdr);
                    }
                }
                #endregion
                #region Validations
                //if (vch.Amount1 + vch.Amount2 +vch.Amount3 + vch.Amount4 + vch.Amount5 + vch.Amount6!= 0 || vch.VoucherDetails.Sum(x => x.Amount) != 0)
                //{
                //    throw new BusinessException(ErrorCode.VCH104);//Credit and Debit Amount mismatch for Voucher
                //}
                if (vch.VoucherDetails.Count <= 1)
                {
                    throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                }
                //if (vch.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) == 0)
                //{
                //    throw new BusinessException(ErrorCode.TADV102);//Atlead one VDR is Required in Advance Transaction
                //}
                if (vch.VoucherDetails.Any(voucherDetail => voucherDetail.VoucherDetailReferences.Count > 0 && voucherDetail.Amount != voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)))
                {
                    throw new BusinessException(ErrorCode.VCH106);//VoucherDetail and VoucherDetailReference Amount Doesn't Tally
                }
                #endregion

                #endregion
                if (doc.TDSAmount > 0)
                {
                    #region TDS Voucher, VDs & VDRs

                    #region Prepare TDSVoucher

                    tdsvoucher.VoucherDate = doc.DocumentDate;
                    tdsvoucher.VoucherDateTime = doc.DocumentDate;
                    tdsvoucher.VoucherTypeId = 92;
                    tdsvoucher.VoucherNo = "TDS-" + doc.DocumentNo;
                    tdsvoucher.Account1Id = doc.TDSLedgerId;
                    tdsvoucher.Amount1 = -doc.TDSAmount;

                    tdsvoucher.Account2Id = doc.PrimaryCreditAccountId;
                    tdsvoucher.Amount2 = doc.TDSAmount;
                    tdsvoucher.OfficeId = doc.OfficeId;
                    tdsvoucher.AccountingRemark = $"Being TDS deducted against bill no {doc.DocumentNo} on amount {doc.PrimaryCreditAmount - doc.RoundOff}";
                    tdsvoucher.ObjectState = ObjectState.Added;
                    vs.Add(tdsvoucher);
                    #endregion
                    #region Prepare TDSVoucherVd and VDR

                    if (tdsvoucher.Account1Id.HasValue && tdsvoucher.Amount1 != 0)
                    {
                        var vd1 = tdsvoucher.VoucherDetails.FirstOrDefault(x => x.OrderId == 1) ?? new VoucherDetail() { };
                        vd1.AccountId = tdsvoucher.Account1Id.Value;
                        vd1.Amount = tdsvoucher.Amount1;
                        vd1.BatchId = tdsvoucher.BatchId;
                        vd1.Particular = "TDS Deducted";
                        vd1.OrderId = 1;
                        vd1.Rate = doc.TDSRate;
                        vd1.Amount1 = vch.VoucherAmount;
                        vd1.Account1Id = doc.PrimaryCreditAccountId;
                        vd1.CurTypeId = doc.CurTypeId;
                        vd1.CurRate = doc.CurRate;
                        vd1.ConstCurTypeId = doc.ConstCurTypeId;

                        var ledger = acrefs.Where(x => x.Id == tdsvoucher.Account1Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();

                        if (ledger == null)
                        {
                            throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification for TDS Voucher  :{tdsvoucher.Account1Id}");
                        }
                        vd1.OfficeId = ledger.OfficeId.Value == 0 ? tdsvoucher.OfficeId : ledger.OfficeId.Value;

                        if (vd1.Id == 0)
                        {
                            vd1.ObjectState = ObjectState.Added;
                            tdsvoucher.VoucherDetails.Add(vd1);
                        }
                        else
                        {
                            vd1.ObjectState = ObjectState.Modified;
                        }
                        vds.Add(vd1);
                    }

                    if (tdsvoucher.Account2Id.HasValue && tdsvoucher.Amount2 != 0)
                    {
                        var a2 = tdsvoucher.VoucherDetails.FirstOrDefault(x => x.OrderId == 2) ?? new VoucherDetail() { };
                        a2.AccountId = tdsvoucher.Account2Id.Value;
                        a2.Amount = tdsvoucher.Amount2;
                        a2.Particular = "Hire TDS Deducted";
                        a2.OrderId = 2;
                        a2.Rate = doc.TDSRate;
                        a2.Amount1 = vch.VoucherAmount;

                        a2.CurTypeId = doc.CurTypeId;
                        a2.CurRate = doc.CurRate;
                        a2.ConstCurTypeId = doc.ConstCurTypeId;

                        var isRefEnabled = acrefs.Any(x => x.Id == a2.AccountId && x.ReferenceFlag);
                        var ledger = acrefs.Where(x => x.Id == tdsvoucher.Account2Id).Select(x => new { x.OfficeId, x.ReferenceFlag }).FirstOrDefault();
                        if (ledger == null)
                        {
                            throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{tdsvoucher.Account2Id}");
                        }
                        a2.OfficeId = ledger.OfficeId.Value == 0 ? tdsvoucher.OfficeId : ledger.OfficeId.Value;

                        if (a2.Id == 0)
                        {
                            a2.ObjectState = ObjectState.Added;
                            tdsvoucher.VoucherDetails.Add(a2);
                        }
                        else
                        {
                            a2.ObjectState = ObjectState.Modified;
                        }
                        vds.Add(a2);
                        a2.VoucherDetailReferences.ForEach(x => x.ObjectState = ObjectState.Deleted);
                        #region Prepare TDSVoucherVDR
                        if (vendorvdr != null)
                        {
                            var vdr1 = new VoucherDetailReference()
                            {
                                Amount = a2.Amount,
                                ObjectState = ObjectState.Added,
                                ReferenceNo = vendorvdr.ReferenceNo,
                                RefId = vendorvdr.Id,
                                fk_ParentReference = vendorvdr,
                                VDRTypeId = 1014,
                                BatchId = a2.BatchId,
                                CurTypeId = doc.CurTypeId,
                                CurRate = doc.CurRate,
                                ConstCurTypeId = doc.ConstCurTypeId

                            };
                            a2.VoucherDetailReferences = new List<VoucherDetailReference>() { vdr1 };
                            vdrs.Add(vdr1);
                        }

                        #endregion
                    }

                    #endregion

                    #endregion
                }
            }
            if (vs.Where(x=>x.VoucherTypeId!= 92).Count() != docs.Count)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Accounting Voucher Count({vs.Count}) and Provided Bill Count({docs.Count}) doest not match");
            }
            if (sparelogextrainfo.Count != docs.Count)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Extra Info Count({vs.Count}) and Provided Bill Count({docs.Count}) doest not match");
            }
            if (labourlist.Count != docs.Sum(x=>x.Labors.Count))
            {
                throw new BusinessException(ErrorCode.GLB106, $"Labour Details (Saving) count({labourlist.Count}) does not match with Provided Labour Count({docs.Sum(x => x.Labors.Count)})");
            }
            if (spareloglist.Count != docs.Sum(x => x.Spares.Count))
            {
                throw new BusinessException(ErrorCode.GLB106, $"Spare Details (Saving) count({spareloglist.Count}) does not match with Provided Spare Count({docs.Sum(x => x.Spares.Count)})");
            }
            //Insert Vouchers
            this._repository.UOW.BulkInsert(vs, transaction);

            //Insert Vouchers Details
            var vbids = vs.Select(x => x.BatchId).ToList();
            var vsbatches = _repository.GetRepository<Voucher>().Queryable().Where(y => vbids.Contains(y.BatchId)).Select(x => new { x.BatchId, x.Id,x.VoucherTypeId,x.VoucherNo }).ToList();
            Parallel.ForEach(vds, vd =>
            {
                vd.VoucherId =  vsbatches?.FirstOrDefault(x => x.BatchId == vd.BatchId&&((vd.Particular?.Contains("TDS Deducted") ?? false)? x.VoucherTypeId == 92/*Not TDS Voucher*/:x.VoucherTypeId!=92/*Not TDS Voucher*/))?.Id ?? 0;
            });
            if (vds.Any(x => x.VoucherId == 0)) throw new BusinessException(ErrorCode.GLB106, "Voucher Integrity Failed!!");
            this._repository.UOW.BulkInsert(vds, transaction);

            // Insert Spare Extra loginfo
            Parallel.ForEach(sparelogextrainfo, ad =>
            {
                ad.VoucherId = vsbatches?.FirstOrDefault(x => x.BatchId == ad.BatchId&&x.VoucherTypeId!=92)?.Id ?? 0;
                ad.TDSVoucherId = vsbatches?.FirstOrDefault(x => x.BatchId == ad.BatchId && x.VoucherTypeId == 92)?.Id;
            });
            this._repository.UOW.BulkInsert(sparelogextrainfo, transaction);
            var vidss = vsbatches.Select(x => (long?)x.Id).Distinct().ToList();

            var seids =
                _repository.GetRepository<SpareLogExtraInfo>()
                    .Queryable()
                    .Where(y => vidss.Contains(y.VoucherId)).Select(x => new { x.VoucherId, x.Id }).ToList();
            //Insert SpareLog
            Parallel.ForEach(spareloglist, ad =>
            {
                ad.VoucherId = vsbatches?.FirstOrDefault(x => x.BatchId == ad.BatchId && x.VoucherTypeId != 92)?.Id ?? 0;
                ad.ExtraInfoId = seids?.FirstOrDefault(x => x.VoucherId == ad.VoucherId)?.Id ?? 0;
            });
            this._repository.UOW.BulkInsert(spareloglist, transaction);

            //Insert LabourLog
            Parallel.ForEach(labourlist, ad =>
            {
                ad.VoucherId = vsbatches?.FirstOrDefault(x => x.BatchId == ad.BatchId && x.VoucherTypeId != 92)?.Id ?? 0;
                ad.ExtraInfoId = seids?.FirstOrDefault(x => x.VoucherId == ad.VoucherId)?.Id ?? 0;
            });
            this._repository.UOW.BulkInsert(labourlist, transaction);

            //Insert Voucher Details reference
            var vids = vds.Select(x => x.VoucherId).Distinct().ToList();
            var vdids =
                _repository.GetRepository<VoucherDetail>()
                    .Queryable()
                    .Where(y => vids.Contains(y.VoucherId)).Select(x => new { x.VoucherId, x.Id,x.OrderId,x.Particular }).ToList();
            Parallel.ForEach(vds, vd =>
            {
                foreach (var vdr in vd.VoucherDetailReferences)
                {
                    vdr.VoucherDetailId = vdids?.FirstOrDefault(x => x.VoucherId == vd.VoucherId && x.OrderId == vd.OrderId)?.Id ?? 0;
                }
            });
            if (vdrs.Any(x => x.VoucherDetailId == 0)) throw new BusinessException(ErrorCode.GLB106, "Voucher Reference Integrity Failed!!");
            this._repository.UOW.BulkInsert(vdrs, transaction);

        }

        public void AmcBatchInsert(List<vwSparePurchaseBill> docs, IDbTransaction transaction)
        {
            if (docs.Any(x => x.Labors == null || x.Labors.Count <= 0)) throw new BusinessException(ErrorCode.GLB106, "One of AMC voucher does not have AMC Details");
            var sparelogextrainfo = new List<SpareLogExtraInfo>();
            var labourlist = new List<RepairLabourLog>();
            var doe = DateTime.Now;
            var sessionid = Helper.SessionId();
            var DT = Helper.ConstCurTypeId;
            foreach (var doc in docs)
            {
                var batchid = Guid.NewGuid().ToString("N");
                doc.BatchId = batchid;
                doc.ConstCurTypeId = DT;
                #region Spare Extra info
                var sle = new SpareLogExtraInfo
                {
                    Id = doc.Id,
                    CurTypeId = doc.CurTypeId,
                    CurRate = doc.CurRate,
                    ConstCurTypeId = doc.ConstCurTypeId,
                    TypeId = doc.TypeId,
                    OfficeId = doc.OfficeId,
                    DocDate = doc.DocumentDate,
                    DocNo = doc.DocumentNo,
                    CrAccountId = doc.PrimaryCreditAccountId,
                    VendorReferenceNo = string.IsNullOrEmpty(doc.VendorReferenceNo) ? doc.DocumentNo : doc.VendorReferenceNo,
                    Remark = doc.Narration,
                    DrAccountId = doc.PrimaryDebitAccountId,
                    CalculateVat = doc.CalVat,
                    ChallanSlipDate=doc.ChallanSlipDate,
                    VoucherTypeId = 83,
                    CGSTACId = doc.CGSTLedgerId,
                    SGSTACId = doc.SGSTLedgerId,
                    IGSTACId = doc.IGSTLedgerId,
                    ViewId = doc.ViewId,
                    CreatedDOE = doe,
                    CreatedSessionId = sessionid,
                    BatchId = doc.BatchId,
                    PageId=doc.PageId,
                    Data=doc.JsonData,
                    TCSAmount=doc.TCSAmount,
                    TCSRate=doc.TCSRate,
                    TCSAccountId=doc.TCSAccountId
                };
                sparelogextrainfo.Add(sle);
                #endregion

                for (var i = 0; i < doc.Labors.Count; i++)
                {
                    var ad = doc.Labors.ElementAt(i);
                    var rll = new RepairLabourLog
                    {
                        Id = ad.Id,
                        ObjectState = ObjectState.Added,
                        VehicleId = ad.VehicleId,
                        HireVehicleId = ad.HireVehicleId,
                        MechanicId = ad.MechanicId,
                        LaborId = ad.LaborId,
                        LaborQty = ad.LaborQty,
                        LaborRate = ad.LaborRate,
                        Amount = ad.Amount,
                        SubTotal = ad.SubTotal,
                        //ServiceTaxAmount = 0,
                        TaxServiceTypeId = ad.GSTServiceTypeId,
                        CGSTAmount = ad.LCCGSTAmount,
                        SGSTAmount = ad.LCSGSTAmount,
                        IGSTAmount = ad.LCIGSTAmount,
                        ODOKm=ad.ODOKm,
                        NetAmount = ad.NetAmount,
                        Remark = ad.Remark,
                        ExtraInfo = sle,
                        CreatedDOE = doe,
                        CreatedSessionId = sessionid,
                        BatchId = batchid,
                        JobCardId=ad.JobCardId,
                        Data = doc.JsonData
                    };
                    labourlist.Add(rll);
                }
            }
            // Insert Spare Extra loginfo (Records will be saved into table and will have ids and batch ids)
            this._repository.UOW.BulkInsert(sparelogextrainfo, transaction);
            // collect all batch ids of inserted records
            var sleBacthIds = sparelogextrainfo.Select(x => x.BatchId).ToList();
            // collect ids of inserted records by matching their Batch ids
            var sleids =
                _repository.GetRepository<SpareLogExtraInfo>()
                    .Queryable()
                    .Where(y => sleBacthIds.Contains(y.BatchId)).Select(x => new { x.BatchId, x.Id }).ToList();
            // Insert LabourLog ()
            // loop ids of spare extra logInfo and set these ids to repair labour log extrainfoId
            foreach (var id in sleids)
            {
                foreach (var repairLabourLog in labourlist.Where(x=>x.BatchId==id.BatchId))
                {
                    repairLabourLog.ExtraInfoId = id.Id;
                }
            }
            this._repository.UOW.BulkInsert(labourlist, transaction);
        }
    }
}
