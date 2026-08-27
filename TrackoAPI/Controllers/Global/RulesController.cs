using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers.Global
{
    [AuthorizeEx]
    public class RulesController : ODataController
    {
        private readonly IUnitOfWorkAsync _uow;

        private readonly IRepositoryAsync<Rule> _ruleRepo;

        public RulesController(IUnitOfWorkAsync uow, IRepositoryAsync<Rule> ruleRepo)
        {
            _ruleRepo = ruleRepo;
            _uow = uow;
        }
        [HttpGet, EnableQuery]
        public IQueryable<Rule> Get()
        {
            return _ruleRepo.Queryable();
        }


        // GET api/<controller>/5
        [EnableQuery]
        public SingleResult<Rule> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_ruleRepo.Queryable().Where(t => t.Id == key));
        }

        // POST api/<controller>
        public async Task<IHttpActionResult> Put(long key, Rule rule)
        {
            try
            {
                _ruleRepo.Update(rule);
                rule.ObjectState = ObjectState.Modified;
                await _uow.SaveChangesAsync();
                return Updated(rule);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        // PUT api/<controller>/5
        public async Task<IHttpActionResult> Post(Rule rule)
        {
            _ruleRepo.Insert(rule);
            rule.ObjectState = ObjectState.Added;
            await _uow.SaveChangesAsync();
            return Created(rule);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<Rule> patch)
        {
            try
            {
                var existing = await _ruleRepo.FindAsync(key);
                var ruleKey = existing.RuleKey;
                patch.Patch(existing);
                existing.RuleKey = ruleKey;
                existing.ObjectState = ObjectState.Modified;
                await _uow.SaveChangesAsync();
                return Updated(existing);
            }
            catch (Exception e)
            {
                throw;
            }
        }
        // DELETE api/<controller>/5
        public async Task<IHttpActionResult> Delete(long key)
        {
            var existing = await _ruleRepo.FindAsync(key);
            if (existing == null) return NotFound();
            existing.ObjectState = ObjectState.Deleted;
            await _uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}