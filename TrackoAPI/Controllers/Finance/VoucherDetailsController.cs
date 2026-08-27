using AutoMapper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VoucherDetailsController : ODataController
    //ODataController
    {
        private readonly IVoucherDetailService _service;
        private readonly IVoucherDetailReferenceService _vdrRepo;
        private readonly IMapper _mapper;
        public VoucherDetailsController(IVoucherDetailService service,IVoucherDetailReferenceService vdrRepo, IMapper mapper)
        {
            _service = service;
            _vdrRepo = vdrRepo;
            _mapper = mapper;
        }
        // GET: odata/VoucherDetails
        [HttpGet, EnableQuery]
        public IQueryable<VoucherDetail> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/VoucherDetails(5)
        [EnableQuery]
        public SingleResult<VoucherDetail> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VoucherDetails(5)
        public async Task<IHttpActionResult> Put(long key, VoucherDetail entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            entity.CurTypeId = entity.Voucher.CurTypeId;
            entity.CurRate=entity.Voucher.CurRate;            

            entity.ObjectState = ObjectState.Modified;
            _service.Update(entity);
            
            await Request.GetContext().SaveChangesAsync();

            return Updated(entity);
        }
        // POST: odata/VoucherDetails
        public async Task<IHttpActionResult> Post(VoucherDetail entity)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            };
            try
            {
                var _jsonVDRs = entity.JsonVDRS;
                entity.ObjectState = ObjectState.Added;
                _service.Insert(entity);
                await uow.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(_jsonVDRs) && (entity.VoucherDetailReferences?.Count ?? 0) == 0)
                {
                    var vdrs= JsonConvert.DeserializeObject<List<VoucherDetailReference>>(_jsonVDRs);
                    
                    vdrs?.ForEach(x =>
                    {
                        if (x.Id == 0)
                        {
                            x.ObjectState = ObjectState.Added;
                            x.VoucherDetailId = entity.Id;
                            x.fk_VoucherDetail = entity;
                            _vdrRepo.Insert(x);
                        }
                        else
                        {
                            x.ObjectState = ObjectState.Modified;
                            x.VoucherDetailId = entity.Id;
                            x.fk_VoucherDetail = entity;
                            _vdrRepo.Update(x);
                        }
                    });               
                    await uow.SaveChangesAsync();
                }
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                };
            }
            catch (System.Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                };
                throw;
            }
            
            return Created(entity);
        }
        //// PATCH: odata/VoucherDetails(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VoucherDetail> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            VoucherDetail entity = await _service.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            };            
            try
            {
                var _jsonVDRs = entity.JsonVDRS;                
                entity.ObjectState = ObjectState.Modified;
                patch.Patch(entity);

                await uow.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(_jsonVDRs) && (entity.VoucherDetailReferences?.Count ?? 0) == 0)
                {
                    var vdrs = JsonConvert.DeserializeObject<List<VoucherDetailReference>>(_jsonVDRs);
                    vdrs?.ForEach(x =>
                    {
                        if (x.Id == 0)
                        {
                            x.ObjectState = ObjectState.Added;
                            x.VoucherDetailId = entity.Id;
                            x.fk_VoucherDetail = entity;
                            _vdrRepo.Insert(x);
                        }
                        else
                        {
                            var vdr = new VoucherDetailReference { Id = x.Id };
                            _vdrRepo.Update(vdr);
                            _mapper.Map(x, vdr);
                            vdr.ObjectState = ObjectState.Modified;
                            vdr.VoucherDetailId = entity.Id;
                            vdr.fk_VoucherDetail = entity;
                        }
                    });
                    await uow.SaveChangesAsync();
                }
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                };               
            }
            catch (System.Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                };
                throw;
            }
            return Updated(entity);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var entity = await _service.FindAsync(key);
            if (entity!= null)
            {
                entity.ObjectState = ObjectState.Deleted;
                _service.Delete(entity);
                await Request.GetContext().SaveChangesAsync();
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("POST")]
        [ODataRoute("VoucherDetails({key})/VoucherDetailReferences")]
        public async Task<IHttpActionResult> PostVoucherDetails([FromODataUri]long key, [FromBody] VoucherDetailReference vdr)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var vd =
                await _service.Queryable().FirstAsync(x => x.Id == key);
            if (vd==null)
            {
                return NotFound();
            }

            vdr.AccountId = vd.AccountId;
            vdr.VoucherDetailId = vd.Id;
            if (vdr.VDRTypeId == 1014&&vdr.RefId.GetValueOrDefault()==0)
            {
                if (string.IsNullOrWhiteSpace(vdr.ReferenceNo)) return BadRequest("Parent Reference Not Provided");
                var parentvdrid = _vdrRepo.Queryable().Where(x => x.ReferenceNo == vdr.ReferenceNo && x.AccountId == vdr.AccountId&&x.VDRTypeId==1013).Select(x=>x.Id).ToList();
                if (parentvdrid.Count == 0)
                {
                    return BadRequest($"Provided Parent Reference Not Found Hint:RefNo: {vdr.ReferenceNo},Account: {vdr.AccountId}, Amount: {vdr.Amount}");
                }
                if(parentvdrid.Count>1) return BadRequest("Provided Parent Reference is Ambiguous");
                vdr.RefId = parentvdrid.FirstOrDefault();
            }
            vdr.ObjectState = ObjectState.Added;
            vdr.VoucherDetailId = key;
            _vdrRepo.Insert(vdr);

            await uow.SaveChangesAsync();
            return Created(vdr);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}