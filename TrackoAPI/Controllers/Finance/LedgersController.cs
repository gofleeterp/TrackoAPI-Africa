using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData;

//using HibernatingRhinos.Profiler.Appender.ProfiledDataAccess;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;
using IsolationLevel = System.Data.IsolationLevel;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class LedgersController : ODataController
    //ODataController
    {
        private readonly ILedgerService _ledgerService;
        //private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public LedgersController(ILedgerService service)
        {
            _ledgerService = service;
            //_unitOfWorkAsync = unitOfWorkAsync;
        }

        [HttpPost]
        public async Task<IHttpActionResult> BulkPostLedger(ODataActionParameters parameters)
        {
            var doe = DateTime.Now;
            var ssid = Helper.SessionId();

            var iledgers = parameters["ledgers"] as IEnumerator<Ledger>;
            if (iledgers == null) return BadRequest("No Records found to upload");
            var ledgers = iledgers.ToList();
            var vouchers = new List<Voucher>();
            var vdlist = new List<VoucherDetail>();
            var vdrlist = new List<VoucherDetailReference>();
            var uow = Request.GetContext();
            var addresslist = new List<PostalAddress>();
            foreach (var entity in ledgers)
            {
                try
                {
                    var batchId = Guid.NewGuid().ToString("N");
                    if (!string.IsNullOrWhiteSpace(entity.Data) && entity.Data.Length > 3)
                    {
                        var address = JsonConvert.DeserializeObject<PostalAddress>(entity.Data);
                        if (address != null)
                        {
                            address.CreatedDOE = doe;
                            address.CreatedSessionId = ssid;
                            address.BatchId = batchId;
                            entity.fk_Address = address;

                            entity.fk_Address.ObjectState = ObjectState.Added;
                            addresslist.Add(entity.fk_Address);
                        }
                    }

                    entity.CreatedDOE = doe;
                    entity.CreatedSessionId = ssid;
                    entity.BatchId = batchId;

                    Validate(entity);

                    #region Opening Voucher

                    if (entity.GroupId.GetValueOrDefault() > 0)
                    {
                        var voucher = new Voucher
                        {
                            OfficeId = entity.OfficeId ?? 0,
                            VoucherDate = entity.EffectiveDate ?? DateTime.Now,
                            VoucherDateTime = entity.EffectiveDate ?? DateTime.Now,
                            VoucherTypeId = 19,
                            VoucherAmount = entity.OpeningBalance,
                            Amount1 = entity.OpeningBalance,
                            Amount2 = 0,
                            Amount3 = 0,
                            Amount4 = 0,
                            Amount5 = 0,
                            Amount6 = 0,
                            UserRemark = null,
                            CreatedDOE = doe,
                            CreatedSessionId = ssid,
                            IsAccepted = false,
                            IsAudited = true,
                            IsAccountsVisiblity = true,
                            Amount7 = 0,
                            Amount8 = 0,
                            Amount9 = 0,
                            Amount10 = 0,
                            BatchId = batchId,
                        };
                        vouchers.Add(voucher);

                        var vd = new VoucherDetail
                        {
                            OfficeId = entity.OfficeId ?? 0,
                            OrderId = 1,
                            Amount = voucher.VoucherAmount,
                            CreatedDOE = doe,
                            CreatedSessionId = ssid,
                            BatchId = batchId,
                        };
                        vdlist.Add(vd);
                        if (entity.ReferenceFlag)
                        {
                            var vdr = new VoucherDetailReference
                            {
                                VDRTypeId = 1013,
                                Amount = vd.Amount,
                                DueDate = entity.EffectiveDate ?? DateTime.Now,
                                CreatedDOE = doe,
                                CreatedSessionId = ssid,
                                BatchId = batchId,
                            };
                            vdrlist.Add(vdr);
                        }
                    }

                    #endregion Opening Voucher
                }
                catch (Exception e)
                {
                    throw;
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var batches = ledgers.Select(y => y.BatchId).ToList();
            try
            {
                //using (var transaction = new TransactionScope())
                //{
                var transaction = uow.Context.Database.CurrentTransaction ??
                                     uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                //var tran = transaction.UnderlyingTransaction is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction;
                var tran = transaction.UnderlyingTransaction;
                if (addresslist.Any())
                {
                    uow.BulkInsert(addresslist, tran);
                }
                var adids = await uow.RepositoryAsync<PostalAddress>().Queryable().Where(x => batches.Contains(x.BatchId)).Select(x => new { x.BatchId, x.Id }).ToListAsync();
                foreach (var l in ledgers)
                {
                    l.AddressId = adids.FirstOrDefault(x => x.BatchId == l.BatchId)?.Id;
                }
                uow.BulkInsert(ledgers, tran);

                if (vouchers.Any())
                {
                    var acids = await _ledgerService.Queryable().Where(x => batches.Contains(x.BatchId)).Select(x => new { x.BatchId, x.Id }).ToListAsync();
                    foreach (var v in vouchers)
                    {
                        v.Account1Id = acids.FirstOrDefault(x => x.BatchId == v.BatchId)?.Id;
                        v.VoucherNo = "OP" + v.Account1Id;
                    }
                    uow.BulkInsert(vouchers, tran);
                    var vchid = await uow.RepositoryAsync<Voucher>().Queryable().Where(x => batches.Contains(x.BatchId)).Select(x => new { x.Account1Id, x.Id }).ToListAsync();
                    foreach (var vd in vdlist)
                    {
                        vd.AccountId = acids.FirstOrDefault(x => x.BatchId == vd.BatchId)?.Id ?? 0;
                        vd.VoucherId = vchid.FirstOrDefault(x => x.Account1Id == vd.AccountId)?.Id ?? 0;
                    }
                    uow.BulkInsert(vdlist, tran);
                    if (vdrlist.Any())
                    {
                        var vdrid = await uow.RepositoryAsync<VoucherDetail>().Queryable().Where(x => batches.Contains(x.BatchId)).Select(x => new { x.AccountId, x.Id }).ToListAsync();
                        foreach (var vdr in vdrlist)
                        {
                            vdr.AccountId = acids.FirstOrDefault(x => x.BatchId == vdr.BatchId)?.Id ?? 0;
                            vdr.ReferenceNo = "Openingbalance" + vdr.AccountId;
                            vdr.VoucherDetailId = vdrid.FirstOrDefault(x => x.AccountId == vdr.AccountId)?.Id ?? 0;
                        }
                        uow.BulkInsert(vdrlist, tran);
                    }
                }

                if (!Request.IsBatchRequest())
                {
                    transaction.Commit();
                    transaction.Dispose();
                    uow.Dispose();
                }

                //    transaction.Complete();
                // }
                var item = new vwBatch { BatchId = batches.JoinStrings("^"), BatchSize = ledgers.Count };
                return Ok(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<List<long>> CheckReferenceFlags(ODataActionParameters paramter)
        {
            var ids = paramter["ids"] as IEnumerator<long>;
            var list = ids.ToList().Distinct().ToList();
            return await _ledgerService.Queryable().Where(x => list.Contains(x.Id) && x.ReferenceFlag).Select(x => x.Id)
                .ToListAsync();
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateLink([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();

            Ledger ledger = await _ledgerService.FindAsync(key);
            if (ledger == null)
            {
                return NotFound();
            }
            long navigationkey = 0;
            switch (navigationProperty)
            {
                case "fk_Address":
                    navigationkey = Request.GetKeyValue<long>(link);
                    PostalAddress curAddress = await uow.RepositoryAsync<PostalAddress>().FindAsync(navigationkey);
                    if (curAddress == null)
                    {
                        return NotFound();
                    }
                    ledger.fk_Address = curAddress;
                    ledger.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return NotFound();
            }
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                await uow.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
                string navigationProperty, [FromBody] Uri link)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();

            Ledger ledger = await _ledgerService.FindAsync(key);
            if (ledger == null)
            {
                return NotFound();
            }
            switch (navigationProperty)
            {
                case "fk_Address":
                    var navigationkey = Request.GetKeyFromUri<long>(link);
                    PostalAddress curAddress = await uow.RepositoryAsync<PostalAddress>().FindAsync(navigationkey);
                    if (curAddress == null)
                    {
                        return NotFound();
                    }
                    ledger.fk_Address = curAddress;
                    ledger.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return NotFound();
            }
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                await uow.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            var ledger = await _ledgerService.FindAsync(key);
            if (ledger == null)
            {
                return NotFound();
            }
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            ledger.ObjectState = ObjectState.Deleted;
            var oldgroup = ledger.GroupId;
            var oldRole = ledger.AccountRoleId;
            _ledgerService.Delete(ledger);
            try
            {
                await Request.GetContext().SaveChangesAsync();
                if (oldRole > 0) await _ledgerService.MapLedgerToDefaultRoleClass(key, null, oldRole);
                if (oldgroup > 0) await _ledgerService.MapLedgerToDefaultGroupClass(key, null, oldgroup);
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw new BusinessException(ErrorCode.GLB108, $"The Used Account Cannot be deleted");
            }
        }

        // GET: odata/Ledgers
        [HttpGet, EnableQuery]
        public IQueryable<Ledger> Get()
        {
            return _ledgerService.Queryable();
        }

        // GET: odata/Ledgers(5)
        [EnableQuery]
        public SingleResult<Ledger> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_ledgerService.Queryable().Where(t => t.Id == key));
        }

        [HttpGet]
        public bool GetReferenceFlag([FromODataUri] long key)
        {
            return _ledgerService.Queryable().Where(x => x.Id == key).Select(x => x.ReferenceFlag).FirstOrDefault();
        }

        //// PATCH: odata/Ledgers(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<Ledger> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            patch.TryGetPropertyValue("JsonDataList", out var jsonDataList);
            Ledger ledger = await _ledgerService.FindAsync(key);
            if (ledger == null)
            {
                return NotFound();
            }
            var oldgroup = ledger.GroupId;
            var oldRole = ledger.AccountRoleId;

            ledger.ObjectState = ObjectState.Modified;            
            patch.Patch(ledger);
            if (jsonDataList is List<JsonDataEntity> dataview && dataview.Any())
            {
                foreach (var entity in dataview)
                {
                    ledger.DeleteAndAdd(entity);
                }
            }
            try
            {
                await Request.GetContext().SaveChangesAsync();
                if (ledger.AccountRoleId != oldRole) await _ledgerService.MapLedgerToDefaultRoleClass(ledger.Id, ledger.AccountRoleId, oldRole);
                if (ledger.GroupId != oldgroup) await _ledgerService.MapLedgerToDefaultGroupClass(ledger.Id, ledger.GroupId, oldgroup);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (LedgerExists(ledger.AccountName, ledger.Id))
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Ledger Name \"{ledger.AccountName}\" already exists");
                }
                else if (LedgerCodeExists(ledger.Alias, ledger.Id))
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Ledger Code \"{ledger.Alias}\" already exists");
                }
                if (!LedgerExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Ok();
        }

        // POST: odata/Ledgers
        public async Task<IHttpActionResult> Post(Ledger ledger)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            ledger.ObjectState = ObjectState.Added;
            _ledgerService.Insert(ledger);
            try
            {
                await uow.SaveChangesAsync();
                if (ledger.AccountRoleId > 0) await _ledgerService.MapLedgerToDefaultRoleClass(ledger.Id, ledger.AccountRoleId, null);
                if (ledger.GroupId > 0) await _ledgerService.MapLedgerToDefaultGroupClass(ledger.Id, ledger.GroupId, null);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (LedgerExists(ledger.AccountName,ledger.Id))
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Ledger Name \"{ledger.AccountName}\" already exists");
                }else if (LedgerCodeExists(ledger.Alias, ledger.Id))
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Ledger Code \"{ledger.Alias}\" already exists");
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Created(ledger);
        }

        // PUT: odata/Ledgers(5)
        public async Task<IHttpActionResult> Put(long key, Ledger ledger)
        {
            var uow = Request.GetContext();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            if (key != ledger.Id)
            {
                return BadRequest();
            }
            ledger.ObjectState = ObjectState.Modified;
            var oldgroup = ledger.GroupId;
            var oldRole = ledger.AccountRoleId;
            _ledgerService.Update(ledger);

            try
            {
                await Request.GetContext().SaveChangesAsync();
                if (ledger.AccountRoleId > 0 || oldRole > 0) await _ledgerService.MapLedgerToDefaultRoleClass(ledger.Id, ledger.AccountRoleId, oldRole);
                if (ledger.GroupId > 0 || oldgroup > 0) await _ledgerService.MapLedgerToDefaultGroupClass(ledger.Id, ledger.GroupId, oldgroup);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (LedgerExists(ledger.AccountName, ledger.Id))
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Ledger Name \"{ledger.AccountName}\" already exists");
                }
                else if (LedgerCodeExists(ledger.Alias, ledger.Id))
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Ledger Code \"{ledger.Alias}\" already exists");
                }
                if (!LedgerExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(ledger);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LedgerExists(string name, long id)
        {
            return _ledgerService.Query(e => e.AccountName == name &&e.Id!=id).Select().Any();
        }
        private bool LedgerCodeExists(string alias, long id)
        {
            return _ledgerService.Query(e => e.Alias == alias && e.Id != id).Select().Any();
        }

        private bool LedgerExists(long key)
        {
            return _ledgerService.Query(e => e.Id == key).Select().Any();
        }
        private void Validate(object model, Type type)
        {
            var validator = Configuration.Services.GetBodyModelValidator();
            var metadataProvider = Configuration.Services.GetModelMetadataProvider();

            HttpActionContext actionContext = new HttpActionContext(ControllerContext, Request.GetActionDescriptor());

            if (!validator.Validate(model, type, metadataProvider, actionContext, String.Empty))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState));
            }
        }
    }
}