//using HibernatingRhinos.Profiler.Appender.ProfiledDataAccess;
using Repository.Pattern.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.WebUtilities.Helper;
using IsolationLevel = System.Data.IsolationLevel;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DriversController : ODataController
    //ODataController
    {
        private readonly IDriverMasterService _driverService;

        private readonly IRepositoryAsync<Ledger> _ledgerRepo;
        //private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public DriversController(IDriverMasterService service, IRepositoryAsync<Ledger> ledgerRepo)
        {
            _driverService = service;
            _ledgerRepo = ledgerRepo;
        }

        [HttpPost]
        public async Task<IHttpActionResult> BulkPostDriver(ODataActionParameters parameters)
        {
            var batchId = Guid.NewGuid().ToString("N");
            var idrivers = parameters["drivers"] as IEnumerator<DriverMaster>;
            if (idrivers == null) return BadRequest("No Records found to upload");
            var drivers = idrivers.ToList();
            var uow = Request.GetContext();
            var cdoe = DateTime.Now;
            var csid = Helper.SessionId();


            var showDriverInAccount = uow.Context.GetApiConfig<int>("ShowDriverInAccounts");
            long defaultGroup = uow.Context.GetApiConfig<long>("DefaultDriverAccountGroupId");
            var accountlist = drivers.Select(entity => new Ledger()
            {
                Alias = entity.DriverCode,
                AccountName = string.IsNullOrWhiteSpace(entity.AccountDetail.AccountName) ? entity.DriverName : entity.AccountDetail.AccountName,
                FleetAcName = entity.DriverName,
                BookingAcName = entity.DriverName,
                GroupId = defaultGroup != 0 ? defaultGroup : entity.AccountDetail.GroupId,
                InvoicePrintingName = string.IsNullOrWhiteSpace(entity.NameOnLicence) ? entity.AccountDetail?.AccountName ?? entity.DriverName : entity.NameOnLicence,
                OfficeId = entity.OfficeId,
                Id = entity.Id,
                ObjectState = ObjectState.Added,
                AccountRoleId = 1085,
                IsAccountImpact = showDriverInAccount == 1,
                BatchId = batchId,
                CreatedDOE = cdoe,
                CreatedSessionId = csid
            }).ToList();
            var transaction = uow.Context.Database.CurrentTransaction ??
                                  uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                //#if !DEBUG
                uow.BulkInsert(accountlist, transaction.UnderlyingTransaction);
                //#elif DEBUG
                //uow.BulkInsert(accountlist, transaction.UnderlyingTransaction is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction);
                //#endif

                var list =
                    uow.RepositoryAsync<Ledger>()
                        .Queryable().Where(x => x.BatchId == batchId).Select(x => new { x.FleetAcName, x.Id }).ToList();
                Parallel.ForEach(drivers.AsParallel(), entity =>
                {
                    entity.Id = list?.FirstOrDefault(x => x.FleetAcName == entity.DriverName)?.Id ?? 0;
                    entity.CreatedDOE =cdoe;
                    entity.CreatedSessionId = csid;
                    entity.BatchId = batchId;
                    entity.NameOnLicence = string.IsNullOrWhiteSpace(entity.NameOnLicence) ? entity.DriverName : entity.NameOnLicence;
                });
                //foreach (var entity in drivers)
                //{
                //    entity.Id = list?.FirstOrDefault(x => x.FleetAcName == entity.DriverName)?.Id ?? 0;
                //}
                //#if !DEBUG
                uow.BulkInsert(drivers, transaction.UnderlyingTransaction);
                //#elif DEBUG
                //uow.BulkInsert(drivers, transaction.UnderlyingTransaction is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction);
                //#endif

                if (!Request.IsBatchRequest())
                {
                    transaction.Commit();
                    transaction.Dispose();
                }
                var item = new vwBatch { BatchId = batchId, BatchSize = drivers.Count };
                return Ok(item);
            }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }
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
            DriverMaster driver = await _driverService.FindAsync(key);
            if (driver == null)
            {
                return NotFound();
            }
            long navigationkey = 0;
            switch (navigationProperty)
            {
                case "fk_Ledger":
                    navigationkey = Request.GetKeyValue<long>(link);
                    Ledger account = await _uow.RepositoryAsync<Ledger>().FindAsync(navigationkey);
                    if (account == null)
                    {
                        return NotFound();
                    }
                    driver.fk_Ledger = account;
                    driver.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);

                case "fk_CurrentAddress":
                    navigationkey = Request.GetKeyValue<long>(link);
                    PostalAddress curAddress = await _uow.RepositoryAsync<PostalAddress>().FindAsync(navigationkey);
                    if (curAddress == null)
                    {
                        return NotFound();
                    }
                    driver.fk_CurrentAddress = curAddress;
                    driver.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);

                case "fk_PermanentAddress":
                    navigationkey = Request.GetKeyValue<long>(link);
                    PostalAddress parAddress = await _uow.RepositoryAsync<PostalAddress>().FindAsync(navigationkey);
                    if (parAddress == null)
                    {
                        return NotFound();
                    }
                    driver.fk_PermanentAddress = parAddress;
                    driver.ObjectState = ObjectState.Modified;
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
            DriverMaster driver = await _driverService.FindAsync(key);
            if (driver == null)
            {
                return NotFound();
            }
            long navigationkey = 0;
            switch (navigationProperty)
            {
                case "fk_Ledger":
                    navigationkey = Request.GetKeyFromUri<long>(link);
                    Ledger account = await _uow.RepositoryAsync<Ledger>().FindAsync(navigationkey);
                    if (account == null)
                    {
                        return NotFound();
                    }
                    driver.fk_Ledger = account;
                    driver.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);

                case "fk_CurrentAddress":
                    navigationkey = Request.GetKeyFromUri<long>(link);
                    PostalAddress curAddress = await _uow.RepositoryAsync<PostalAddress>().FindAsync(navigationkey);
                    if (curAddress == null)
                    {
                        return NotFound();
                    }
                    driver.fk_CurrentAddress = curAddress;
                    driver.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);

                case "fk_PermanentAddress":
                    navigationkey = Request.GetKeyFromUri<long>(link);
                    PostalAddress parAddress = await _uow.RepositoryAsync<PostalAddress>().FindAsync(navigationkey);
                    if (parAddress == null)
                    {
                        return NotFound();
                    }
                    driver.fk_PermanentAddress = parAddress;
                    driver.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);

                default:
                    return NotFound();
            }
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var driver = await _driverService.Queryable().Include(x => x.fk_Ledger).FirstOrDefaultAsync(x => x.Id == key);
            if (driver == null)
            {
                return NotFound();
            }
            driver.ObjectState = ObjectState.Deleted;
            if (driver.fk_Ledger != null)
            {
                driver.fk_Ledger.ObjectState = ObjectState.Deleted;
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                _driverService.Delete(driver);
                await Request.GetContext().SaveChangesAsync();
                await _ledgerRepo.MapLedgerToDefaultRoleClass(driver.Id, null, 1085);
                await _ledgerRepo.MapLedgerToDefaultGroupClass(driver.Id, null, driver.fk_Ledger?.GroupId);
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

        // GET: odata/DriverMasters
        [HttpGet, EnableQuery]
        public IQueryable<DriverMaster> Get()
        {
            return _driverService.Queryable();
        }

        // GET: odata/DriverMasters(5)
        [EnableQuery]
        public SingleResult<DriverMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_driverService.Queryable().Where(t => t.Id == key));
        }

        // GET: odata/DriverMasters(5)/fk_CurrentAddress
        [ODataRoute("Drivers({key})/fk_CurrentAddress")]
        public SingleResult<PostalAddress> GetCurrentAddress([FromODataUri] long key)
        {
            return SingleResult.Create(_driverService.Queryable().Where(t => t.Id == key).Select(x => x.fk_CurrentAddress));
        }

        // GET: odata/DriverMasters(5)/fk_CurrentAddress
        [ODataRoute("Drivers({key})/fk_PermanentAddress")]
        public SingleResult<PostalAddress> GetPermanentAddress([FromODataUri] long key)
        {
            return SingleResult.Create(_driverService.Queryable().Where(t => t.Id == key).Select(x => x.fk_PermanentAddress));
        }

        //// PATCH: odata/DriverMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DriverMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DriverMaster driver = await _driverService.Queryable().Include(x => x.fk_Ledger).FirstOrDefaultAsync(x => x.Id == key);
            if (driver == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            var oldgroupid = driver.fk_Ledger?.GroupId;
            driver.ObjectState = ObjectState.Modified;
            patch.Patch(driver);
            if (LicenseExists(driver.LicenceNo, driver.Id)) return BadRequest($"License Number {driver.LicenceNo} is Duplicate");
            try
            {
                _driverService.Update(driver);
                await Request.GetContext().SaveChangesAsync();
                await _ledgerRepo.MapLedgerToDefaultRoleClass(driver.Id, 1085, null);
                if (driver.fk_Ledger?.GroupId != oldgroupid) await _ledgerRepo.MapLedgerToDefaultGroupClass(driver.Id, driver.fk_Ledger?.GroupId, oldgroupid);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (!DriverMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(driver);
        }

        [HttpPatch, ODataRoute("Drivers({key})/Guarantors")]
        public async Task<IHttpActionResult> PatchGuarantors([FromODataUri] long key, [FromODataUri] long guarantorKey, Delta<DriverGuarantor> entity)
        {
            var _uow = Request.GetContext();
            DriverGuarantor guarantor = await _uow.RepositoryAsync<DriverGuarantor>().FindAsync(guarantorKey);
            DriverMaster driver = await _driverService.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (driver == null || guarantor == null)
            {
                return NotFound();
            }
            entity.Patch(guarantor);
            guarantor.fk_Driver = driver;
            guarantor.DriverId = driver.Id;
            guarantor.ObjectState = ObjectState.Modified;
            _uow.RepositoryAsync<DriverGuarantor>().Update(guarantor);
            await _uow.SaveChangesAsync();
            return Updated(guarantor);
        }

        [HttpPatch, ODataRoute("Drivers({key})/Relatives")]
        public async Task<IHttpActionResult> PatchRelatives([FromODataUri] long key, [FromODataUri] long guarantorKey, Delta<DriverRelative> entity)
        {
            var _uow = Request.GetContext();
            DriverRelative log = await _uow.RepositoryAsync<DriverRelative>().FindAsync(guarantorKey);
            DriverMaster driver = await _driverService.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (driver == null || log == null)
            {
                return NotFound();
            }
            entity.Patch(log);
            log.fk_Driver = driver;
            log.DriverId = driver.Id;
            log.ObjectState = ObjectState.Modified;
            _uow.RepositoryAsync<DriverRelative>().Update(log);
            await _uow.SaveChangesAsync();
            return Updated(log);
        }

        [HttpPatch, ODataRoute("Drivers({key})/TrainingLogs")]
        public async Task<IHttpActionResult> PatchTrainingLogs([FromODataUri] long key, [FromODataUri] long guarantorKey, Delta<DriverTrainingLog> entity)
        {
            var _uow = Request.GetContext();
            DriverTrainingLog log = await _uow.RepositoryAsync<DriverTrainingLog>().FindAsync(guarantorKey);
            DriverMaster driver = await _driverService.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (driver == null || log == null)
            {
                return NotFound();
            }
            entity.Patch(log);
            log.fk_Driver = driver;
            log.DriverId = driver.Id;
            log.ObjectState = ObjectState.Modified;
            _uow.RepositoryAsync<DriverTrainingLog>().Update(log);
            await _uow.SaveChangesAsync();
            return Updated(log);
        }

        // POST: odata/DriverMasters
        public async Task<IHttpActionResult> Post(DriverMaster driver)
        {
            if (LicenseExists(driver.LicenceNo, driver.Id)) return BadRequest($"License Number {driver.LicenceNo} is Duplicate");
            driver.ObjectState = ObjectState.Added;
            _driverService.Insert(driver);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                await Request.GetContext().SaveChangesAsync();
                await _ledgerRepo.MapLedgerToDefaultRoleClass(driver.Id, 1085, null);
                if (driver.fk_Ledger.GroupId > 0) await _ledgerRepo.MapLedgerToDefaultGroupClass(driver.Id, driver.fk_Ledger.GroupId, null);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (DriverMasterExists(driver.DriverName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                    //return Conflict();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Created(driver);
        }

        [HttpPost, ODataRoute("Drivers({key})/Guarantors")]
        public async Task<IHttpActionResult> PostGuarantors([FromODataUri] long key, DriverGuarantor entity)
        {
            var _uow = Request.GetContext();
            DriverMaster driver = await _driverService.FindAsync(key);
            if (driver == null)
            {
                return NotFound();
            }
            entity.fk_Driver = driver;
            entity.DriverId = driver.Id;
            entity.ObjectState = ObjectState.Added;
            _uow.RepositoryAsync<DriverGuarantor>().Insert(entity);
            await _uow.SaveChangesAsync();
            return Created(entity);
        }

        [HttpPost, ODataRoute("Drivers({key})/Relatives")]
        public async Task<IHttpActionResult> PostRelatives([FromODataUri] long key, DriverRelative entity)
        {
            var _uow = Request.GetContext();
            DriverMaster driver = await _driverService.FindAsync(key);
            if (driver == null)
            {
                return NotFound();
            }
            entity.DriverId = driver.Id;
            entity.fk_Driver = driver;
            entity.ObjectState = ObjectState.Added;
            _uow.RepositoryAsync<DriverRelative>().Insert(entity);
            await _uow.SaveChangesAsync();
            return Created(entity);
        }

        [HttpPost, ODataRoute("Drivers({key})/TrainingLogs")]
        public async Task<IHttpActionResult> PostTrainingLogs([FromODataUri] long key, DriverTrainingLog entity)
        {
            var _uow = Request.GetContext();
            DriverMaster driver = await _driverService.FindAsync(key);
            if (driver == null)
            {
                return NotFound();
            }
            entity.fk_Driver = driver;
            entity.DriverId = driver.Id;
            entity.ObjectState = ObjectState.Added;
            _uow.RepositoryAsync<DriverTrainingLog>().Insert(entity);
            await _uow.SaveChangesAsync();
            return Created(entity);
        }

        // PUT: odata/DriverMasters(5)
        public async Task<IHttpActionResult> Put(long key, DriverMaster driver)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (LicenseExists(driver.LicenceNo, driver.Id)) return BadRequest($"License Number {driver.LicenceNo} is Duplicate");
            if (key != driver.Id)
            {
                return BadRequest();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            driver.ObjectState = ObjectState.Modified;

            _driverService.Update(driver);

            try
            {
                await Request.GetContext().SaveChangesAsync();
                if (driver.fk_Ledger.GroupId > 0) await _ledgerRepo.MapLedgerToDefaultGroupClass(driver.Id, driver.fk_Ledger.GroupId, null);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (!DriverMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(driver);
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

        private bool DriverMasterExists(string driverName)
        {
            return _driverService.Query(e => e.DriverName == driverName).Select().Any();
        }

        private bool DriverMasterExists(long key)
        {
            return _driverService.Query(e => e.Id == key).Select().Any();
        }

        private bool LicenseExists(string licenseNo, long? DriverId)
        {
            if (string.IsNullOrWhiteSpace(licenseNo)) return false;
            return _driverService.Query(e => e.LicenceNo == licenseNo && e.Id != DriverId).Select().Any();
        }
    }
}