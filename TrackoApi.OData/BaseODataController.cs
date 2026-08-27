using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core;
using Unity;

namespace TrackoApi.OData
{
    public class BaseODataController<TModel>:ODataController
        where TModel : class
    {
        private readonly IUnityContainer _uc;
        private readonly IGlobalStore _gs;
        private readonly IRepositoryAsync<TModel> _repo;

        public BaseODataController(IUnityContainer container, IGlobalStore gs)
        {
            _uc = container;
            _gs = gs;
            _repo= _uc.Resolve<IRepositoryAsync<TModel>>();

        }
		[EnableQuery]
		public IHttpActionResult Get()
		{
			var dbSet = _repo.Queryable();

			return Ok(dbSet);
		}

		[EnableQuery,HttpGet]
		public async Task<IHttpActionResult> Get(long key)
		{
			var entity = await _repo.FindAsync(key).ConfigureAwait(true);
			if (entity == null)
			{
				return NotFound();
			}

			return Ok(entity);
		}

		[EnableQuery,HttpPost]
		public async Task<IHttpActionResult> Post([FromBody] TModel entity)
		{
			_repo.Insert(entity);
			await _repo.UOW.SaveChangesAsync().ConfigureAwait(false);

			return Created(entity);
		}

		[EnableQuery,HttpPut]
		public async Task<IHttpActionResult> Put(long key, [FromBody] Delta<TModel> entity)
		{
			var originalEntity = await _repo.FindAsync(key).ConfigureAwait(true);
			if (originalEntity == null)
			{
				return NotFound();
			}

			entity.Put(originalEntity);
			await _repo.UOW.SaveChangesAsync().ConfigureAwait(false);

			return Updated(entity);
		}

		[EnableQuery,HttpPatch]
		public async Task<IHttpActionResult> Patch(long key, [FromBody] Delta<TModel> entity)
		{
			var originalEntity = await _repo.FindAsync(key).ConfigureAwait(true);
			if (originalEntity == null)
			{
				return NotFound();
			}

			entity.Patch(originalEntity);
			await _repo.UOW.SaveChangesAsync().ConfigureAwait(false);

			return Updated(entity);
		}

		[EnableQuery,HttpDelete]
		public async Task<IHttpActionResult> Delete(long key)
		{
			
			var entity = await _repo.FindAsync(key).ConfigureAwait(true);
			if (entity == null)
			{
				return NotFound();
			}
			_repo.Delete(entity);
			await _repo.UOW.SaveChangesAsync().ConfigureAwait(false);

			return Ok();
		}
	}
	
}