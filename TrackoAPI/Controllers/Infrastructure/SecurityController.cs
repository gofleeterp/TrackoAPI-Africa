using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TrackoApi.Core;
using TrackoApi.Data;
using TrackoAPI.Infrastructure;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class SecurityApiController : ApiController
    {
        //private readonly ApiUserManager _appUserManager = null;
        //private readonly ApiRoleManager _appRoleManager = null;

        //protected ApiUserManager AppUserManager => _appUserManager ?? Request.GetOwinContext().GetUserManager<ApiUserManager>();

        //protected ApiRoleManager AppRoleManager => _appRoleManager ?? Request.GetOwinContext().GetUserManager<ApiRoleManager>();

        protected ITrackoApiDbContext TrackoApiDbContext = null;
        private readonly IGlobalStore _storage;

        public SecurityApiController(ITrackoApiDbContext trackoApiDbContext,IGlobalStore globalStorage)
        {
            TrackoApiDbContext = trackoApiDbContext;
            _storage = globalStorage;
        }

        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
