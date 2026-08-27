using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CitiesController : ODataController
    //ODataController
    {
        private readonly ICityMasterService _objCityMasterService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public CitiesController(IUnitOfWorkAsync unitOfWorkAsync, ICityMasterService service)
        {
            _objCityMasterService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/CityMasters
        [HttpGet, EnableQuery]
        public IQueryable<CityMaster> Get()
        {
            return _objCityMasterService.Queryable();
        }

        [HttpGet]
        public string GetCityAbbr([FromODataUri] long key)
        {
            return _objCityMasterService.Queryable().Where(x=>x.Id==key).Select(x=>x.CityAbbr).FirstOrDefault();
        }
        
        [HttpGet]
        public string GetCityName([FromODataUri] long key)
        {
            return _objCityMasterService.Queryable().Where(x => x.Id == key).Select(x => x.CityName).FirstOrDefault();
        }
        // GET: odata/CityMasters(5)
        [EnableQuery]
        public SingleResult<CityMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objCityMasterService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/CityMasters(5)
        public async Task<IHttpActionResult> Put(long key, CityMaster objCityMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objCityMaster.Id)
            {
                return BadRequest();
            }
            objCityMaster.ObjectState = ObjectState.Modified;
            _objCityMasterService.Update(objCityMaster);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objCityMaster);
        }
        // POST: odata/CityMasters
        public async Task<IHttpActionResult> Post(CityMaster objCityMaster)
        {
            objCityMaster.ObjectState = ObjectState.Added;
            _objCityMasterService.Insert(objCityMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CityMasterExists(objCityMaster.CityName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objCityMaster);
        }
        //// PATCH: odata/CityMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CityMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CityMaster objCityMaster = await _objCityMasterService.FindAsync(key);
            if (objCityMaster == null)
            {
                return NotFound();
            }
            var oldValue = new
            {
                objCityMaster.CityAbbr,
                objCityMaster.CityName,
                objCityMaster.Status
            };
            objCityMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objCityMaster);
            try
            {
                if ($"{oldValue.CityName}{oldValue.CityName}" != $"{objCityMaster.CityName}{objCityMaster.CityName}" ||
                    oldValue.Status != objCityMaster.Status)
                {
                    var routes =
                    _unitOfWorkAsync.RepositoryAsync<RouteMaster>()
                        .Queryable()
                        .Where(x => x.FromPlaceId == key || x.ToPlaceId == key || x.WayPoints.Any(y => y.CityId == key))
                        .ToList();
                    routes?.ForEach(x =>
                    {
                        x.Abbr = x.Abbr.Replace(oldValue.CityAbbr, objCityMaster.CityAbbr);
                        x.Name = x.Name.Replace(oldValue.CityName, objCityMaster.CityName);
                        x.Status = objCityMaster.Status;
                        x.ObjectState = ObjectState.Modified;
                    });
                }
                
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objCityMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objCityMaster = await _objCityMasterService.FindAsync(key);
            if (objCityMaster == null)
            {
                return NotFound();
            }
            objCityMaster.ObjectState = ObjectState.Deleted;
            _objCityMasterService.Delete(objCityMaster);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CityMasterExists(string cityName)
        {
            return _objCityMasterService.Query(e => e.CityName == cityName).Select().Any();
        }
        private bool CityMasterExists(long key)
        {
            return _objCityMasterService.Query(e => e.Id == key).Select().Any();
        }
    }
}