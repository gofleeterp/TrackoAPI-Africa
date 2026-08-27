
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.GSTN.Models.EWB
{
    public class GSPAuthToken
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }/*token_type:bearer*/
        public long ExpiresIn { get; set; }/*expires_in*/
        public string Scope { get; set; }
        public string JTI { get; set; }

    }
}
