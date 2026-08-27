using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Global;

namespace TrackoAPI.Infrastructure
{
    public class ExtendedClaimsProvider
    {
        public static IEnumerable<Claim> GetClaims(ApiUser user)
        {

            List<Claim> claims = new List<Claim>();

            //var daysInWork = (DateTime.Now.Date - user.JoinDate).TotalDays;

            //if (daysInWork > 90)
            //{
            //    claims.Add(CreateClaim("FTE", "1"));

            //}
            //else
            //{
            //    claims.Add(CreateClaim("FTE", "0"));
            //}

            return claims;
        }

        public static Claim CreateClaim(string type, string value) => new Claim(type, value, ClaimValueTypes.String);
    }
}
