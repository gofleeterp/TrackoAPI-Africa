using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Ef6.DbInterceptors;

namespace Repository.Pattern.Ef6
{
    public class ApiDbConfiguration: DbConfiguration
    {
        public ApiDbConfiguration()
        {
            //SetDefaultTransactionHandler(()=>new System.Data.Entity.Infrastructure.CommitFailureHandler());
            //SetExecutionStrategy(SqlProviderServices.ProviderInvariantName, () => new SqlAzureExecutionStrategy(2, TimeSpan.FromMilliseconds(300)));
            
        }

        public void Start()
        {
           //base.AddInterceptor(new SessionInfoInterceptor());
        }        
    }
}
