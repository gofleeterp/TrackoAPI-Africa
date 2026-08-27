using System.Threading.Tasks;
using System.Web.Http;
using TrackoAPI.Infrastructure;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [RoutePrefix("api/RefreshTokens")]
    [AuthorizeEx]
    public class RefreshTokensController : ApiController
    {
        private readonly AuthRepository _repo = null;

        public RefreshTokensController(IAuthRepository repo)
        {
            _repo = (AuthRepository) repo;
        }

       [Route("GetAll")]
        public IHttpActionResult Get()
        {
            return Ok(_repo.GetAllRefreshTokens());
        }

        //[Authorize(Users = "Admin")]
        [AllowAnonymous]
        [Route("Revoke")]
        public async Task<IHttpActionResult> Delete(string tokenId)
        {
            var result = await _repo.RemoveRefreshToken(tokenId);
            if (result)
            {
                return Ok();
            }
            return BadRequest("Token Id does not exist");

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
