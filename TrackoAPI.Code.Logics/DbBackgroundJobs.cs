using Hangfire;
using Hangfire.Server;
using Repository.Pattern.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;

namespace TrackoAPI.Code.Logics
{
    public interface IDbBackgroundJobs
    {
        [Queue("fifo_event_stockmerge"), DisableConcurrentExecution(60), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        void MergeCNStock(PerformContext context, string tenantId, long cnid, long officeId, long logId, long existingLogId);
    }
    
}
