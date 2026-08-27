using Repository.Pattern.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.WebUtilities.Helper;
using IsolationLevel = System.Data.IsolationLevel;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehiclesController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<Ledger> _ledger;
        private readonly IVehicleMasterService _vehicleService;
        public VehiclesController(IVehicleMasterService service, IRepositoryAsync<Ledger> ledgers)
        {
            _vehicleService = service;
            _ledger = ledgers;
        }

        [HttpPost]
        public async Task<IHttpActionResult> BulkPostVehicle(ODataActionParameters parameters)
        {
            var batchId = Guid.NewGuid().ToString("N");
            var ivehicles = parameters["vehicles"] as IEnumerator<VehicleMaster>;
            if (ivehicles == null) return BadRequest("No Records found to upload");
            var vehicles = ivehicles.ToList();
            var uow = Request.GetContext();
            var cdoe = DateTime.Now;
            var csid = Helper.SessionId();

            //var showVehicleInAccount = uow.Context.GetApiConfig<int>("ShowVehicleInAccounts");
            long defaultGroup = 49;
            long x = uow.Context.GetApiConfig<long>("DefaultVehicleAccountGroupId");
            defaultGroup = x > 0 ? x : defaultGroup;

            var accountlist = vehicles.Select(entity => new Ledger()
            {
                Alias = entity.VehicleNo,
                AccountName = string.IsNullOrWhiteSpace(entity.AccountDetail.AccountName) ? entity.VehicleRegNo : entity.AccountDetail.AccountName,
                FleetAcName = entity.VehicleNo,
                BookingAcName = entity.VehicleNo,
                GroupId = defaultGroup != 0 ? defaultGroup : entity.AccountDetail.GroupId,
                OfficeId = entity.OfficeId,
                IsAccountImpact = true,
                Id = entity.Id,
                ObjectState = ObjectState.Added,
                AccountRoleId = 1130,
                CreatedDOE=cdoe,
                CreatedSessionId=csid,
                BatchId= batchId
            }).ToList();
            //Parallel.ForEach(vehicles.AsParallel(), entity =>
            //{
            //    var ledger = new Ledger()
            //    {
            //        Alias = entity.VehicleNo,
            //        AccountName = string.IsNullOrWhiteSpace(entity.AccountDetail.AccountName) ? entity.VehicleRegNo : entity.AccountDetail.AccountName,
            //        FleetAcName = entity.VehicleNo,
            //        BookingAcName = entity.VehicleNo,
            //        GroupId = defaultGroup != 0 ? defaultGroup : entity.AccountDetail.GroupId,
            //        OfficeId = entity.OfficeId,
            //        IsAccountImpact = showVehicleInAccount == 1,
            //        Id = entity.Id,
            //        ObjectState = ObjectState.Added,
            //        AccountRoleId = 1130,
            //        BatchId = batchId
            //    };
            //    ledger.IsAccountImpact = showVehicleInAccount == 1;
            //    ledger.BatchId = batchId;
            //    ledger.CreatedDOE = DateTime.Now;
            //    ledger.CreatedSessionId = Helper.SessionId();
            //    accountlist.Add(ledger);
            //    entity.CreatedDOE = DateTime.Now;
            //    entity.CreatedSessionId = Helper.SessionId();
            //    entity.BatchId = batchId;
            //});
            try
            {
                var transaction = uow.Context.Database.CurrentTransaction ??
                                     uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                var tran = transaction.UnderlyingTransaction;//is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction;
                uow.BulkInsert(accountlist, tran);
                var list = await
                    uow.RepositoryAsync<Ledger>()
                        .Queryable().Where(x => x.BatchId == batchId).Select(x => new { x.FleetAcName, x.Id }).ToListAsync();
                Parallel.ForEach(vehicles.AsParallel(), entity =>
                {
                    entity.Id = list?.FirstOrDefault(x => x.FleetAcName == entity.VehicleNo)?.Id ?? 0;
                    entity.CreatedDOE = cdoe;
                    entity.CreatedSessionId = csid;
                    entity.BatchId = batchId;
                });
                uow.BulkInsert(vehicles, tran);
                if (!Request.IsBatchRequest())
                {
                    transaction.Commit();
                    transaction.Dispose();
                    uow.Dispose();
                }
                var item = new vwBatch { BatchId = batchId, BatchSize = vehicles.Count };
                return Ok(item);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateLink([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var _uow = Request.GetContext();
            VehicleMaster vehicle = await _vehicleService.FindAsync(key);
            if (vehicle == null)
            {
                return NotFound();
            }
            long navigationkey = 0;
            switch (navigationProperty)
            {
                case "Aliases":
                    navigationkey = Request.GetKeyValue<long>(link);
                    MasterAlias alias = await _uow.RepositoryAsync<MasterAlias>().FindAsync(navigationkey);
                    if (alias == null)
                    {
                        return NotFound();
                    }
                    alias.RelatedId = vehicle.Id;
                    alias.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);

                case "Dues":
                    navigationkey = Request.GetKeyValue<long>(link);
                    VehicleDueMapping due = await _uow.RepositoryAsync<VehicleDueMapping>().FindAsync(navigationkey);
                    if (due == null)
                    {
                        return NotFound();
                    }
                    vehicle.Dues.Add(due);
                    due.ObjectState = ObjectState.Modified;
                    vehicle.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);

                default:
                    return NotFound();
            }
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var _uow = Request.GetContext();
            VehicleMaster vehicle = await _vehicleService.FindAsync(key);
            if (vehicle == null)
            {
                return NotFound();
            }
            long navigationkey = 0;
            switch (navigationProperty)
            {
                case "Dues":
                    navigationkey = Request.GetKeyFromUri<long>(link);
                    VehicleDueMapping due = await _uow.RepositoryAsync<VehicleDueMapping>().FindAsync(navigationkey);
                    if (due == null)
                    {
                        return NotFound();
                    }
                    vehicle.Dues.Add(due);
                    due.ObjectState = ObjectState.Modified;
                    vehicle.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);

                case "Aliases":
                    navigationkey = Request.GetKeyFromUri<long>(link);
                    MasterAlias alias = await _uow.RepositoryAsync<MasterAlias>().FindAsync(navigationkey);
                    if (alias == null)
                    {
                        return NotFound();
                    }
                    alias.RelatedId = vehicle.Id;
                    alias.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);

                default:
                    return NotFound();
            }
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var vehicle = await _vehicleService.FindAsync(key);
            if (vehicle == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            vehicle.ObjectState = ObjectState.Deleted;
            try
            {
                _vehicleService.Delete(vehicle);
                await Request.GetContext().SaveChangesAsync();
                await _ledger.MapLedgerToDefaultRoleClass(vehicle.Id, 1130, null);
                if (vehicle.fk_VehicleLedger?.GroupId > 0) await _ledger.MapLedgerToDefaultGroupClass(vehicle.Id, null, vehicle.fk_VehicleLedger?.GroupId);
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

        // GET: odata/VehicleMasters
        [HttpGet, EnableQuery]
        public IQueryable<VehicleMaster> Get()
        {
            return _vehicleService.Queryable();
        }

        // GET: odata/VehicleMasters(5)
        [EnableQuery]
        public SingleResult<VehicleMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_vehicleService.Queryable().Where(t => t.Id == key));
        }

        [HttpGet, Route("AllVehicles")]
        public IQueryable<vwOwnHireVehicle> GetAllOwnHireVehicles()
        {
            return _ledger.SelectQuery<vwOwnHireVehicle>("EXEC [dbo].[Proc_1031_SearchOwnHireVehicle]@parameter1,@parameter2",
                new object[] { new SqlParameter("@parameter1", ""), new SqlParameter("@parameter2", 10000) });
        }

        [HttpGet, EnableQuery]
        public IQueryable<vwOwnHireVehicle> GetOwnHireVehicle(ODataQueryOptions<vwOwnHireVehicle> option)
        {
            var config = _vehicleService.GetConfigValue<int>("ShowAttachedVehicleAsOwnVehicle");
            var own = _vehicleService.Queryable();
            if (config == 0)
            {
                own = own.Where(x => !x.IsHireVehicle);
            }

            var ov = own.Select(x => new vwOwnHireVehicle
            {
                Id = x.Id,
                VehicleNo = x.VehicleNo,
                RegistrationNo = x.VehicleRegNo,
                OwnerId = x.OwnerPartyId,
                Owner = x.fk_VehicleOwner == null ? null : x.fk_VehicleOwner.FleetAcName,
                Type = "O"
            });

            var hire = Request.GetContext().RepositoryAsync<HireVehicle>().Queryable().Select(x => new vwOwnHireVehicle
            {
                Id = x.Id,
                VehicleNo = x.VehicleNo,
                RegistrationNo = x.RegistrationNo,
                OwnerId = x.HirePartyId,
                Owner = x.fk_HireParty == null ? null : x.fk_HireParty.FleetAcName,
                Type = "H"
            });

            var query = ov.Union(hire);
            option.ApplyTo(query);
            return query;
        }

        [HttpGet]
        public IQueryable<vwOwnHireVehicle> GetOwnHireVehicleNew([FromODataUri] string searchTerm,
            [FromODataUri] int count)
        {
            return _ledger.SelectQuery<vwOwnHireVehicle>("EXEC [dbo].[Proc_1031_SearchOwnHireVehicle]@parameter1,@parameter2",
                new object[] { new SqlParameter("@parameter1", searchTerm), new SqlParameter("@parameter2", count) });
        }
        //// PATCH: odata/VehicleMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleMaster> patch)
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
            VehicleMaster vehicle = await _vehicleService.FindAsync(key);
            if (vehicle == null)
            {
                return NotFound();
            }
            vehicle.ObjectState = ObjectState.Modified;
            var oldgroupid = vehicle.fk_VehicleLedger?.GroupId;
            try
            {
                patch.Patch(vehicle);
                _vehicleService.Patch(vehicle);
                await Request.GetContext().SaveChangesAsync();
                await _ledger.MapLedgerToDefaultRoleClass(vehicle.Id, 1130, null);
                if (vehicle.fk_VehicleLedger?.GroupId != oldgroupid) await _ledger.MapLedgerToDefaultGroupClass(vehicle.Id, vehicle.fk_VehicleLedger?.GroupId, oldgroupid);
            }
            catch (BusinessException)
            {
                if (!Request.IsBatchRequest())
                    uow.Rollback();
                throw;
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                    uow.Rollback();
                if (VehicleMasterExists(key))
                {
                    // return Conflict();
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            if (!Request.IsBatchRequest())
                uow.Commit();

            return Updated(vehicle);
        }

        [HttpPatch, ODataRoute("Vehicles({key})/Aliases")]
        public async Task<IHttpActionResult> PatchAliases([FromODataUri] long key, [FromODataUri] long key1, Delta<MasterAlias> delta)
        {
            var uom = Request.GetContext();
            var repo = uom.RepositoryAsync<MasterAlias>();
            var alias = repo.Find(key1);
            if (alias == null) return NotFound();
            delta.Patch(alias);
            if (alias.RelatedTypeId == 0)
            {
                alias.RelatedTypeId = 1073;
            }
            alias.RelatedId = key;
            alias.ObjectState = ObjectState.Modified;
            repo.Update(alias);
            await uom.SaveChangesAsync();
            return Updated(alias);
        }

        [HttpPatch, ODataRoute("Vehicles({key})/Dues")]
        public async Task<IHttpActionResult> PatchDues([FromODataUri] long key, [FromODataUri] long key1, Delta<VehicleDueMapping> delta)
        {
            var uom = Request.GetContext();
            var repo = uom.RepositoryAsync<VehicleDueMapping>();
            var due = repo.Find(key1);
            if (due == null) return NotFound();
            delta.Patch(due);
            due.VehicleId = key;
            due.ObjectState = ObjectState.Modified;
            repo.Update(due);
            await uom.SaveChangesAsync();
            return Updated(due);
        }

        // POST: odata/VehicleMasters
        public async Task<IHttpActionResult> Post(VehicleMaster vehicle)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                long defaultGroup = 49;
                long x = uow.Context.GetApiConfig<long>("DefaultVehicleAccountGroupId");
                defaultGroup = x > 0 ? x : defaultGroup;

                vehicle.fk_VehicleLedger.GroupId = defaultGroup;
            }catch  { }
            vehicle.ObjectState = ObjectState.Added;
            _vehicleService.Insert(vehicle);
            try
            {
                await Request.GetContext().SaveChangesAsync();
                await _ledger.MapLedgerToDefaultRoleClass(vehicle.Id, 1130, null);
                if (vehicle.fk_VehicleLedger?.GroupId > 0) await _ledger.MapLedgerToDefaultGroupClass(vehicle.Id, vehicle.fk_VehicleLedger?.GroupId, null);
            }
            catch (BusinessException)
            {
                if (!Request.IsBatchRequest())
                    uow.Rollback();
                throw;
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                    uow.Rollback();
                if (VehicleMasterExists(vehicle.VehicleNo, vehicle.VehicleRegNo))
                {
                    // return Conflict();
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            if (!Request.IsBatchRequest())
                uow.Commit();
            return Created(vehicle);
        }

        [HttpPost, ODataRoute("Vehicles({key})/Aliases")]
        public async Task<IHttpActionResult> PostAliases([FromODataUri] long key, MasterAlias alias)
        {
            if (alias.RelatedTypeId == 0)
            {
                alias.RelatedTypeId = 1073;
            }
            if (alias.ExtAppId == 0)
            {
                alias.ExtAppId = 1134;
            }
            var uom = Request.GetContext();
            alias.RelatedId = key;
            alias.ObjectState = ObjectState.Added;
            uom.RepositoryAsync<MasterAlias>().Insert(alias);
            await uom.SaveChangesAsync();
            return Created(alias);
        }

        [HttpPost, ODataRoute("Vehicles({key})/Dues")]
        public async Task<IHttpActionResult> PostDues([FromODataUri] long key, VehicleDueMapping due)
        {
            var uom = Request.GetContext();
            due.VehicleId = key;
            due.ObjectState = ObjectState.Added;
            due.Status = MasterStatus.Active;
            uom.RepositoryAsync<VehicleDueMapping>().Insert(due);
            await uom.SaveChangesAsync();
            return Created(due);
        }

        // PUT: odata/VehicleMasters(5)
        public async Task<IHttpActionResult> Put(long key, VehicleMaster vehicle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != vehicle.Id)
            {
                return BadRequest();
            }
            var uow = Request.GetContext();
            try
            {
                long defaultGroup = 49;
                long x = uow.Context.GetApiConfig<long>("DefaultVehicleAccountGroupId");
                defaultGroup = x > 0 ? x : defaultGroup;

                vehicle.fk_VehicleLedger.GroupId = defaultGroup;
            }
            catch { }
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            vehicle.ObjectState = ObjectState.Modified;
            _vehicleService.Update(vehicle);

            try
            {
                await Request.GetContext().SaveChangesAsync();
                await _ledger.MapLedgerToDefaultRoleClass(vehicle.Id, 1130, null);
                if (vehicle.fk_VehicleLedger?.GroupId > 0) await _ledger.MapLedgerToDefaultGroupClass(vehicle.Id, vehicle.fk_VehicleLedger?.GroupId, null);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (!VehicleMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(vehicle);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private bool VehicleMasterExists(string vehiclecode, string vehicleno)
        {
            return _vehicleService.Query(e => e.VehicleNo == vehiclecode || e.VehicleRegNo == vehicleno).Select().Any();
        }

        private bool VehicleMasterExists(long key)
        {
            return _vehicleService.Query(e => e.Id == key).Select().Any();
        }
    }
}