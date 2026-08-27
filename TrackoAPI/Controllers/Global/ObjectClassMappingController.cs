using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Models.Shared;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ObjectClassMappingController : ODataController
    {
        private readonly IObjectClassMapService _ObjectClassMapService;

        public ObjectClassMappingController(IObjectClassMapService service)
        {
            _ObjectClassMapService = service;
        }
        // GET: odata/ObjectClassMap
        [HttpGet,EnableQuery]
        public IQueryable<ObjectClassMap> Get()
        {
            return _ObjectClassMapService.Queryable();
        }

        [HttpGet]
        public IQueryable<ObjectClassMap> GetObjectMappings([FromODataUri] string keys, [FromODataUri] int count,[FromODataUri]string searchTerm="")
        {
            if (string.IsNullOrWhiteSpace(keys))
            {
                return null;
            }
            var ids = keys.Split(',').Select(long.Parse);
            var uow = Request.GetContext();
            // var cls = _objectClassService.Queryable().Include(x => x.Category).Include(x=>x.ObjectMappings).Where(x => x.Id == key).SelectMany();
            var cls =
                uow.Repository<ObjectClassMap>().Queryable().Include(x => x.fk_Category).Include(x => x.fk_Class).Where(x => ids.Contains(x.ClassId));
            var roleTypeId = cls.Select(x => x.fk_Category.RoleTypeId).FirstOrDefault();
            switch (roleTypeId)
            {
                case 1145:
                    var ac1145 =(from a in uow.Repository<LedgerRole>().Queryable()
                                  join b in cls on a.RoleId equals b.fk_Category.RoleId
                                  where !a.fk_Ledger.IsDefaulter && b.ObjectId == a.LedgerId &&(a.fk_Ledger.AccountName.Contains(searchTerm)|| a.fk_Ledger.Alias.Contains(searchTerm))
                                  let c = new
                                  {
                                      Id = b.Id,
                                      ObjectId = a.LedgerId,
                                      ClassId = b.ClassId,
                                      CategoryId = b.CategoryId,
                                      ObjectName = a.fk_Ledger.AccountName
                                  }
                                  select c).Take(count).ToList();
                    var result = ac1145.Select(x => new ObjectClassMap()
                    {
                        Id = x.Id,
                        CategoryId = x.CategoryId,
                        ClassId = x.ClassId,
                        ObjectId = x.ObjectId,
                        ObjectName = x.ObjectName
                    }).AsQueryable();
                    return result;
                case 1146:
                    var ac1146 = (from a in uow.Repository<Ledger>().Queryable()
                                  join b in cls on a.GroupId equals b.fk_Category.RoleId
                                  where !a.IsDefaulter && b.ObjectId == a.Id&& (a.AccountName.Contains(searchTerm) || a.Alias.Contains(searchTerm))
                                  let c = new
                                  {
                                      Id = b.Id,
                                      ObjectId = a.Id,
                                      ClassId = b.ClassId,
                                      CategoryId = b.CategoryId,
                                      ObjectName = a.AccountName
                                  }
                                  select c).Take(count).ToList();
                    return ac1146.Select(x => new ObjectClassMap()
                    {
                        Id = x.Id,
                        CategoryId = x.CategoryId,
                        ClassId = x.ClassId,
                        ObjectId = x.ObjectId,
                        ObjectName = x.ObjectName,
                    }).AsQueryable();
                case 1292:
                    var offices = (from a in uow.Repository<OfficeMaster>().Queryable()
                                   join b in cls on 1292 equals b.fk_Category.RoleId
                                   where a.Status == MasterStatus.Active && b.ObjectId == a.Id && (a.OfficeAbbr.Contains(searchTerm) || a.OfficeName.Contains(searchTerm))
                                   let c = new
                                   {
                                       Id = b.Id,
                                       ObjectId = a.Id,
                                       ClassId = b.ClassId,
                                       CategoryId = b.CategoryId,
                                       ObjectName = a.OfficeName
                                   }
                                   select c).Take(count).ToList();
                    return offices.Select(x => new ObjectClassMap()
                    {
                        Id = x.Id,
                        CategoryId = x.CategoryId,
                        ClassId = x.ClassId,
                        ObjectId = x.ObjectId,
                        ObjectName = x.ObjectName,
                    }).AsQueryable();
                default:
                    return new List<ObjectClassMap>().AsQueryable();
            }
        }
        // GET: odata/ObjectClassMap(5)
        [EnableQuery]
        public SingleResult<ObjectClassMap> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_ObjectClassMapService.Queryable().Where(t => t.Id == key));
        }
        //PUT: odata/ObjectClassMaps(5)
       public async Task<IHttpActionResult> Put(long key, ObjectClassMap ObjectClassMap)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != ObjectClassMap.Id)
            {
                return BadRequest();
            }
            ObjectClassMap.ObjectState=ObjectState.Modified;
            _ObjectClassMapService.Update(ObjectClassMap);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObjectClassMapExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(ObjectClassMap);
        }
        // POST: odata/ObjectClassMaps
        public async Task<IHttpActionResult> Post(ObjectClassMap ObjectClassMap)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ObjectClassMap.ObjectState = ObjectState.Added;
            _ObjectClassMapService.Insert(ObjectClassMap);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ObjectClassMapExists(ObjectClassMap.CategoryId,ObjectClassMap.ObjectId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }

            return Created(ObjectClassMap);
        }
        
        //// PATCH: odata/ObjectClassMaps(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ObjectClassMap> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectClassMap objectClassMap = await _ObjectClassMapService.FindAsync(key);

            if (objectClassMap == null)
            {
                return NotFound();
            }
            objectClassMap.ObjectState=ObjectState.Modified;
            patch.Patch(objectClassMap);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObjectClassMapExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objectClassMap);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            ObjectClassMap ObjectClassMap = await _ObjectClassMapService.FindAsync(key);

            if (ObjectClassMap == null)
            {
                return NotFound();
            }
            ObjectClassMap.ObjectState=ObjectState.Deleted;
            _ObjectClassMapService.Delete(ObjectClassMap);
            await Request.GetContext().SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }
        //// GET: odata/ObjectClassMap(5)/Classes
        //[EnableQuery]
        //public IQueryable<ObjectClassMap> GetClasses([FromODataUri] long key)
        //{
        //    return _ObjectClassMapService.Queryable().Where(m => m.Id == key).SelectMany(m => m.ObjectClassMapes);
        //}
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

        private bool ObjectClassMapExists(long categoryId,long objectId)
        {
            return _ObjectClassMapService.Query(e => e.CategoryId == categoryId && e.ObjectId==objectId).Select().Any();
        }
        private bool ObjectClassMapExists(long id)
        {
            return _ObjectClassMapService.Query(e => e.Id == id).Select().Any();
        }
    }
}