using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoAPI.Models.Shared;

namespace TrackoAPI.ViewModels.Global
{
    public class UserResourceResult
    {
        public long UserId { get; set; }
        public long ObjectId { get; set; }
        public string ObjectName { get; set; }
        public AclType EntityType { get; set; }
        public bool Read { get; set; }
        public bool Create { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
    }

    

}
