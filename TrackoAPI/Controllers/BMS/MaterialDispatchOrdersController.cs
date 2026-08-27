using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class MaterialDispatchOrdersController : ODataController
    //ODataController
    {
        private readonly IMaterialDispatchOrderService _repo;

        public MaterialDispatchOrdersController(IMaterialDispatchOrderService service)
        {
            _repo = service;
        }
        // GET: odata/MaterialDispatchOrders
        [HttpGet, EnableQuery]
        public IQueryable<MaterialDispatchOrder> Get() => _repo.Queryable();

        // GET: odata/MaterialDispatchOrders(5)
        [EnableQuery]
        public SingleResult<MaterialDispatchOrder> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/MaterialDispatchOrders(5)
        public async Task<IHttpActionResult> Put(long key, MaterialDispatchOrder objMaterialDispatchOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objMaterialDispatchOrder.Id)
            {
                return BadRequest();
            }
            objMaterialDispatchOrder.ObjectState = ObjectState.Modified;
            _repo.Update(objMaterialDispatchOrder);

            try
            {
              await Request.GetContext().SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialDispatchOrderExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialDispatchOrder);
        }
        // POST: odata/MaterialDispatchOrders
        public async Task<IHttpActionResult> Post(MaterialDispatchOrder objMaterialDispatchOrder)
        {
            objMaterialDispatchOrder.ObjectState = ObjectState.Added;
            _repo.Insert(objMaterialDispatchOrder);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MaterialDispatchOrderExists(objMaterialDispatchOrder.OrderNo, objMaterialDispatchOrder.VendorId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name or Code should be unique");
                }
                throw;
            }
            return Created(objMaterialDispatchOrder);
        }
        //// PATCH: odata/MaterialDispatchOrders(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<MaterialDispatchOrder> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MaterialDispatchOrder objMaterialDispatchOrder = await _repo.FindAsync(key);
            if (objMaterialDispatchOrder == null)
            {
                return NotFound();
            }
            objMaterialDispatchOrder.ObjectState = ObjectState.Modified;
            patch.Patch(objMaterialDispatchOrder);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialDispatchOrderExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialDispatchOrder);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objMaterialDispatchOrder = await _repo.FindAsync(key);
            if (objMaterialDispatchOrder == null)
            {
                return NotFound();
            }
            objMaterialDispatchOrder.ObjectState = ObjectState.Deleted;
            _repo.Delete(objMaterialDispatchOrder);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var mdo = _repo.Queryable().SingleOrDefault(p => p.Id == key);
            if (mdo == null)
            {
                return NotFound();
            }
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                
                case "fk_Dispatch":
                    var tlRepo = Request.GetContext()
                        .RepositoryAsync<VehicleMovementLog>();
                    if (!await tlRepo.Queryable().AnyAsync(x => x.Id == id))
                    {
                        return BadRequest("Invalid Dispatch Defined");
                    }
                    mdo.DispatchId = null;
                    mdo.fk_Dispatch = null;
                    mdo.ObjectState = ObjectState.Modified;
                    _repo.Update(mdo);
                    break;
                case "fk_ChallanId":
                    var ChalRepo = Request.GetContext()
                        .RepositoryAsync<ChallanMaster>();
                    if (!await ChalRepo.Queryable().AnyAsync(x => x.Id == id))
                    {
                        return BadRequest("Invalid Challan Defined");
                    }
                    mdo.ChallanId = null;
                    mdo.fk_ChallanId = null;
                    mdo.ObjectState = ObjectState.Modified;
                    _repo.Update(mdo);
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction();
            }
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
       string navigationProperty, [FromBody] Uri link)
        {
            var mdo = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (mdo == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_Dispatch":
                    var tlRepo = Request.GetContext()
                       .RepositoryAsync<VehicleMovementLog>();
                    if (!await tlRepo.Queryable().AnyAsync(x => x.Id == id))
                    {
                        return BadRequest("Invalid Dispatch Defined");
                    }
                    mdo.DispatchId = id;
                    mdo.ObjectState = ObjectState.Modified;
                    break;

                case "fk_ChallanId":
                    var ChlRepo = Request.GetContext()
                       .RepositoryAsync<ChallanMaster>();
                    if (!await ChlRepo.Queryable().AnyAsync(x => x.Id == id))
                    {
                        return BadRequest("Invalid Challan Defined");
                    }
                    mdo.ChallanId = id;
                    mdo.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing&&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MaterialDispatchOrderExists(string orderNo,long? vendorCode) => _repo.Query(e => (e.OrderNo == orderNo)|| (e.VendorId== vendorCode)).Select().Any();
        private bool MaterialDispatchOrderExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}