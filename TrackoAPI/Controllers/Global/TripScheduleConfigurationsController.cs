using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

//using HibernatingRhinos.Profiler.Appender.ProfiledDataAccess;
using IsolationLevel = System.Data.IsolationLevel;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TripScheduleConfigurationsController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<TripScheduleConfiguration> _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public TripScheduleConfigurationsController(IUnitOfWorkAsync unitOfWorkAsync, IRepositoryAsync<TripScheduleConfiguration> service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }

        [HttpPost]
        public IHttpActionResult BulkPost(ODataActionParameters parameters)
        {
            var uow = Request.GetContext();
            var transaction = uow.Context.Database.CurrentTransaction ??
                              uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            var tran = transaction.UnderlyingTransaction;//is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction;
            try
            {
                if (!(parameters["logs"] is IEnumerator<TripScheduleConfiguration> tripschedule)) return BadRequest("No Contract log found to upload");
                var ts = tripschedule.ToList();
                ConcurrentBag<ValidationResult> list = new ConcurrentBag<ValidationResult>();
                Parallel.ForEach(ts.AsParallel(), entity =>
                {
                    entity.ObjectState = ObjectState.Added;
                    entity.CreatedDOE = DateTime.Now;
                    entity.CreatedSessionId = Helper.SessionId();
                    foreach (var validationResult in entity.ValidateLogic())
                    {
                        list.Add(validationResult);
                    }
                });
                if (list.Any())
                {
                    int count = 0;
                    foreach (ValidationResult result in list)
                    {
                        count++;
                        ModelState.AddModelError(result.MemberNames?.JoinStrings(",") ?? count.ToString(), result.ErrorMessage);
                    }
                }

                if (ModelState.IsValid)
                {
                    uow.BulkInsert(ts, tran);
                    tran.Commit();
                    return Ok();
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw;
            }
            finally
            {
                tran.Dispose();
                uow.Dispose();
            }
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTripScheduleConfiguration = await _repo.FindAsync(key).ConfigureAwait(false);
            if (objTripScheduleConfiguration == null)
            {
                return NotFound();
            }
            objTripScheduleConfiguration.ObjectState = ObjectState.Deleted;
            _repo.Delete(objTripScheduleConfiguration);
            await _unitOfWorkAsync.SaveChangesAsync().ConfigureAwait(false);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/TripScheduleConfigurations
        [HttpGet, EnableQuery]
        public IQueryable<TripScheduleConfiguration> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/TripScheduleConfigurations(5)
        [EnableQuery]
        public SingleResult<TripScheduleConfiguration> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        //// PATCH: odata/TripScheduleConfigurations(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TripScheduleConfiguration> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TripScheduleConfiguration objTripScheduleConfiguration = await _repo.FindAsync(key).ConfigureAwait(false);
            if (objTripScheduleConfiguration == null)
            {
                return NotFound();
            }
            patch.Patch(objTripScheduleConfiguration);
            objTripScheduleConfiguration.ObjectState = ObjectState.Modified;
            await _unitOfWorkAsync.SaveChangesAsync().ConfigureAwait(false);

            return Updated(objTripScheduleConfiguration);
        }

        // POST: odata/TripScheduleConfigurations
        public async Task<IHttpActionResult> Post(TripScheduleConfiguration objTripScheduleConfiguration)
        {
            objTripScheduleConfiguration.ObjectState = ObjectState.Added;
            _repo.Insert(objTripScheduleConfiguration);
            await _unitOfWorkAsync.SaveChangesAsync().ConfigureAwait(false);
            return Created(objTripScheduleConfiguration);
        }

        // PUT: odata/TripScheduleConfigurations(5)
        public async Task<IHttpActionResult> Put(long key, TripScheduleConfiguration objTripScheduleConfiguration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != objTripScheduleConfiguration.Id)
            {
                return BadRequest();
            }
            objTripScheduleConfiguration.ObjectState = ObjectState.Modified;
            _repo.Update(objTripScheduleConfiguration);
            await _unitOfWorkAsync.SaveChangesAsync().ConfigureAwait(false);

            return Updated(objTripScheduleConfiguration);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}