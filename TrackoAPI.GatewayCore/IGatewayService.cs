using System;
using System.Collections.Generic;
using System.Text;

namespace TrackoAPI.GatewayCore
{
    public interface IGatewayService
    {
        string GetConnectionByTenantId(string tenantId);
    }
}
