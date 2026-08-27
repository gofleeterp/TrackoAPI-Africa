using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.OData.Batch;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.WebApi;
using Repository.Pattern.Core.UnitOfWork;

namespace TrackoApi.OData.Helper
{
    public class ODataBatchHandlerSingleTransaction : DefaultODataBatchHandler
    {
        private readonly IUnityContainer _unity;
        private IUnitOfWorkAsync _uow;
        public ODataBatchHandlerSingleTransaction(HttpServer httpServer) : base(httpServer)
        {
            Server = httpServer;
            var uhdr = (UnityHierarchicalDependencyResolver)Server.Configuration.DependencyResolver;
            _unity= ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer)));
        }

        public HttpServer Server { get; private set; }

        public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(
            IEnumerable<ODataBatchRequestItem> requests, CancellationToken cancellationToken)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }
            //return base.ExecuteRequestMessagesAsync(requests, cancellationToken);

            IList<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            try
            {
                foreach (ODataBatchRequestItem request in requests)
                {
                    var operation = request as OperationRequestItem;
                    if (operation != null)
                    {
                        responses.Add(await operation.SendRequestAsync(Invoker, cancellationToken));
                    }
                    else
                    {
                        await ExecuteChangeSet((ChangeSetRequestItem)request, responses, cancellationToken);
                    }
                }
            }
            catch
            {
                foreach (ODataBatchResponseItem response in responses)
                {
                    response?.Dispose();
                }
                throw;
            }
            return responses;
        }

        private async Task ExecuteChangeSet(ChangeSetRequestItem changeSet, IList<ODataBatchResponseItem> responses,
            CancellationToken cancellationToken)
        {
            _uow = _unity.Resolve<IUnitOfWorkAsync>();
            using (_uow)
            {
                _uow.BeginTransaction(IsolationLevel.Serializable);
                foreach (HttpRequestMessage request in changeSet.Requests)
                {
                    request.SetContext(_uow);
                }
                var changeSetResponse = (ChangeSetResponseItem)await changeSet.SendRequestAsync(Invoker, cancellationToken);
                responses.Add(changeSetResponse);
                if (changeSetResponse.Responses.All(x => x.IsSuccessStatusCode))
                {
                    _uow.Commit();
                }
                else
                {
                    _uow.Rollback();
                }
            }
        }
    }
}
public static class HttpRequestMessageExtensions
{
    private const string DbContext = "Batch_DbContext";

    public static void SetContext(this HttpRequestMessage request, IUnitOfWorkAsync context)
    {
        try
        {
            request.Properties[DbContext] = context;
        }
        catch (Exception)
        {
            Debugger.Break();
            throw;
        }
        
    }

    public static IUnitOfWorkAsync GetContext(this HttpRequestMessage request)
    {
        try
        {
            object trackoApiContext;
            if (request.Properties.TryGetValue(DbContext, out trackoApiContext))
            {
                return (IUnitOfWorkAsync)trackoApiContext;
            }
            var uhdr = (UnityHierarchicalDependencyResolver)request.GetConfiguration().DependencyResolver;
            var _uow = ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer))).Resolve<IUnitOfWorkAsync>();
            SetContext(request, _uow);
            //request.RegisterForDispose(unity);
            return _uow;
        }
        catch (Exception)
        {
            Debugger.Break();
            throw;
        }
    }
}
