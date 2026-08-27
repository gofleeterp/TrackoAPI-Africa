using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Models.Base
{
    internal interface IODataEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
