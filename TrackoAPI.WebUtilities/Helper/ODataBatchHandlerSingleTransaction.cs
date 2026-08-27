using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Batch;
using Repository.Pattern.Core.UnitOfWork;

using Tenant.Models;

using TrackoAPI.Infrastructure;
using Unity;
using Unity.AspNet.WebApi;

namespace TrackoAPI.WebUtilities.Helper
{
    public class ODataBatchHandlerSingleTransaction : DefaultODataBatchHandler
    {
        private readonly IUnityContainer _unity;
        private IUnitOfWorkAsync _uow;
        private IAuthRepository _auth;

        public ODataBatchHandlerSingleTransaction(HttpServer httpServer) : base(httpServer)
        {
            Server = httpServer;
            var uhdr = (UnityHierarchicalDependencyResolver)Server.Configuration.DependencyResolver;
            _unity= ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer)));
        }
        public override void ValidateRequest(HttpRequestMessage request)
        {
            base.ValidateRequest(request);
        }
        public override Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.ProcessBatchAsync(request, cancellationToken);
        }
        public override Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.ParseBatchRequestsAsync(request, cancellationToken);
        }
        public override Task<HttpResponseMessage> CreateResponseMessageAsync(IEnumerable<ODataBatchResponseItem> responses, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.CreateResponseMessageAsync(responses, request, cancellationToken);
        }
        public override Uri GetBaseUri(HttpRequestMessage request)
        {
            return base.GetBaseUri(request);
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
                _uow = _unity.Resolve<IUnitOfWorkAsync>();
                _auth = _unity.Resolve<IAuthRepository>();
                
                foreach (ODataBatchRequestItem request in requests)
                {
                    if (request is OperationRequestItem operation)
                    {
                        operation.Request.SetContext(_uow);
                        operation.Request.SetSecurityContext(_auth);
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
            
            using (_uow)
            {
                //_uow.BeginTransaction(IsolationLevel.ReadCommitted);
                
                
                //foreach (HttpRequestMessage request in changeSet.Requests)
                //{
                //    request.SetContext(_uow);
                //    request.SetSecurityContext(_auth);
                //}
                //var changeSetResponse = (ChangeSetResponseItem)await changeSet.SendRequestAsync(Invoker, cancellationToken);
                //responses.Add(changeSetResponse);
                //if (changeSetResponse.Responses.All(x => x.IsSuccessStatusCode))
                //{
                //    _uow.Commit();
                //}
                //else
                //{
                //    _uow.Rollback();
                //}
                using (var transaction = _uow.ODataBatchBeginTransaction(IsolationLevel.ReadCommitted))
                {
                    foreach (HttpRequestMessage request in changeSet.Requests)
                    {
                        request.SetContext(_uow);
                        request.SetSecurityContext(_auth);
                    }
                    var changeSetResponse = (ChangeSetResponseItem)await changeSet.SendRequestAsync(Invoker, cancellationToken);
                    responses.Add(changeSetResponse);
                    if (changeSetResponse.Responses.All(x => x.IsSuccessStatusCode))
                    {
                       transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
            }
        }
    }
    public class ODataBatchHandlerSingleTransactionForTenant : DefaultODataBatchHandler
    {
        private readonly IUnityContainer _unity;
        private ITenantDbContext _uow;

        public ODataBatchHandlerSingleTransactionForTenant(HttpServer httpServer) : base(httpServer)
        {
            Server = httpServer;
            var uhdr = (UnityHierarchicalDependencyResolver)Server.Configuration.DependencyResolver;
            _unity = ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer)));
        }
        public override void ValidateRequest(HttpRequestMessage request)
        {
            base.ValidateRequest(request);
        }
        public override Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.ProcessBatchAsync(request, cancellationToken);
        }
        public override Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.ParseBatchRequestsAsync(request, cancellationToken);
        }
        public override Task<HttpResponseMessage> CreateResponseMessageAsync(IEnumerable<ODataBatchResponseItem> responses, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.CreateResponseMessageAsync(responses, request, cancellationToken);
        }
        public override Uri GetBaseUri(HttpRequestMessage request)
        {
            return base.GetBaseUri(request);
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
                _uow = _unity.Resolve<ITenantDbContext>();

                foreach (ODataBatchRequestItem request in requests)
                {
                    if (request is OperationRequestItem operation)
                    {
                        operation.Request.SetTenantDb(_uow);
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

            using (_uow)
            {
                using (var transaction = _uow.ODataBatchBeginTransaction(IsolationLevel.ReadCommitted))
                {
                    foreach (HttpRequestMessage request in changeSet.Requests)
                    {
                        request.SetTenantDb(_uow);
                    }
                    var changeSetResponse = (ChangeSetResponseItem)await changeSet.SendRequestAsync(Invoker, cancellationToken);
                    responses.Add(changeSetResponse);
                    if (changeSetResponse.Responses.All(x => x.IsSuccessStatusCode))
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
            }
        }
    }
}