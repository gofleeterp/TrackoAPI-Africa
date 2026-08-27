using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IWLT.TrackoAPI.Subscription.Helpers
{
    public class TransactionScopeODataBatchHandler : DefaultODataBatchHandler
    {
        #region Properties

        public IsolationLevel BatchDbIsolationLevel { get; set; } = IsolationLevel.RepeatableRead;

        #endregion

        #region ExecuteRequestMessagesAsync

        public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(
          IEnumerable<ODataBatchRequestItem> requests,
          RequestDelegate handler)
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));

            IDbTransaction transaction = null;

            try
            {
                Task RequestDelegate(HttpContext httpContext)
                {
                    if (transaction == null)
                    {
                        var dbTransactionFactory = httpContext.RequestServices.GetService<IDbTransactionFactory>();

                        transaction = dbTransactionFactory?.BeginTransaction(BatchDbIsolationLevel);
                    }

                    return handler(httpContext);
                }

                var responses = await base.ExecuteRequestMessagesAsync(requests, RequestDelegate);

                var wasSuccessful = responses.OfType<ChangeSetResponseItem>()
                  .Select(r => r.Contexts.All(c => c.Response.IsSuccessStatusCode()))
                  .All(c => c);

                if (wasSuccessful)
                {
                    if (transaction != null)
                    {
                        transaction.Commit();

                        OnCommitted();
                    }
                }
                else
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();

                        OnRollbacked();
                    }
                }

                return responses;
            }
            finally
            {
                using (transaction)
                {
                }
            }
        }

        #endregion

        #region OnRollbacked

        protected virtual void OnRollbacked()
        {
        }

        #endregion

        #region OnCommited

        protected virtual void OnCommitted()
        {
        }

        #endregion
    }
}
