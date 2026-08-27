using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NLog;
using RedisCacheClient;
using RedisCacheClient.UrlShortner;

using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using Repository.Pattern.DataContext;
using Repository.Pattern.Ef6;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.Validation;
using Tenant;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.MessageService;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoApi.Service.Global;
using TrackoAPI.Code.Logics;
using TrackoAPI.Hubs;
using TrackoAPI.Infrastructure.Services;
using TrackoAPI.SignalR.Core;
using Unity;
using Unity.Config;
using Unity.Injection;
using Unity.Lifetime;

namespace TrackoAPI
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        public UnityConfig(IUnityContainer unity)
        {
            RegisterTypes(unity);
        }
        //#region Unity Container
        //private static Lazy<IUnityContainer> container =
        //  new Lazy<IUnityContainer>(() =>
        //  {
        //      var container = new UnityContainer();
        //      container.AddExtension(new Diagnostic());
        //      RegisterTypes(container);
        //      return container;
        //  });

        ///// <summary>
        ///// Configured Unity Container.
        ///// </summary>
        //public static IUnityContainer Container => container.Value;
        //#endregion
        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        //public static IUnityContainer GetConfiguredContainer() => Container.Value;
        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {

            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();
            //var auto_conf = new MapperConfiguration(cfg =>
            //{
            //    cfg.CreateMap<CnChallan, CnChallan>();
            //    cfg.CreateMap<vwBillPaymentLog, CNBillPaymentLog>();
            //    cfg.CreateMap<Voucher, Voucher>();
            //    cfg.CreateMap<CNBillLog, CNBillLog>().Ignore(x => x.Id);
            //});
            //var mapper = new Mapper(auto_conf);
            container.RegisterInstance<IMapper>(AutoMapperConfig.CreateMapper(), InstanceLifetime.Singleton);
            container.RegisterSingleton<StackExchange.Redis.Extensions.Core.ISerializer, StackExchange.Redis.Extensions.Newtonsoft.NewtonsoftSerializer>();
            container.RegisterFactory<ILogger>(l => LogManager.GetLogger("Global"));
            
            if (Helper.RedisCacheFlag)
            {
                var config = new RedisConfiguration()
                {
                    AbortOnConnectFail = true,
                    KeyPrefix = $"_{Helper.APICountryRegion}apiCache",
                    Hosts = new RedisHost[]
                {
                    new RedisHost(){Host = Helper.RedisNetworkAddress, Port = Helper.RedisPort}
                },
                    AllowAdmin = true,
                    //#if DEBUG
                    ConnectTimeout = 6000,
                    SyncTimeout = 6000,
                    //#else
                    //                SyncTimeout=1000,
                    //                ConnectTimeout = 3000,
                    //#endif
                    Database = Helper.RedisDatabase,/*africa*/
                    Ssl = false,
                    Password = Helper.RedisPassword,
                    ServerEnumerationStrategy = new ServerEnumerationStrategy()
                    {
                        Mode = ServerEnumerationStrategy.ModeOptions.All,
                        TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                        UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.IgnoreIfOtherAvailable
                    }
                };
                container.RegisterFactory<RedisConfiguration>(c => config, FactoryLifetime.Singleton);
                container.RegisterSingleton<StackExchange.Redis.Extensions.Core.ICacheClient, StackExchange.Redis.Extensions.Core.StackExchangeRedisCacheClient>(new InjectionConstructor(new ResolvedParameter<ISerializer>(), new ResolvedParameter<RedisConfiguration>()));                
                container.RegisterSingleton<IGlobalStore, GlobalStore>(new InjectionConstructor(new ResolvedParameter<ICacheClient>()));
                container.RegisterFactory<IDatabase>(c =>
                {
                    var cacheClient = c.Resolve<ICacheClient>();
                    return cacheClient.Database;
                });
                container.RegisterType<IUniqueIdGenerator, UniqueIdGenerator>(lifetimeManager: new TransientLifetimeManager());
                container.RegisterType<IUrlRepository, UrlRepository>(lifetimeManager: new TransientLifetimeManager());
                container.RegisterSingleton<IDbBackgroundJobs, DbBackgroundJobs>(new InjectionConstructor(new ResolvedParameter<IGlobalStore>()));

            }
            else
            {

                MemoryCacheOptions memoryCacheOptions = new MemoryCacheOptions
                {
                    //config your cache here
                    //40%
                    CompactionPercentage = 0.40,
                    ExpirationScanFrequency = TimeSpan.FromMinutes(30),
                    SizeLimit = 100000
                };

                //MemoryCache memoryCache = new MemoryCache(Options.Create<MemoryCacheOptions>(memoryCacheOptions));
                container.RegisterSingleton<IMemoryCache, MemoryCache>(new InjectionConstructor(Options.Create<MemoryCacheOptions>(memoryCacheOptions)));
                container.RegisterSingleton<IGlobalStore, GlobalStoreOnPremises>(new InjectionConstructor(new ResolvedParameter<IMemoryCache>()));
            }
            
            //container.RegisterSingleton<RedisConfiguration>(new InjectionFactory(c=>config));
            
            
            
            // TODO: Register your type's mappings here.
            if (!Helper.HostedOnPremise)
            {
                container.RegisterType<ITenantDbContext, TenantDbContext>(new PerWebAPIRequestLifetimeManager());
                container.RegisterType(typeof(ITenantEntitySet<>), typeof(TenantEntitySet<>), new PerWebAPIRequestLifetimeManager());

            }
            //container.RegisterSingleton<ISMSService, SMSService>();
            container.RegisterSingleton<ISMSService, TextLocalSMSService>();
            container.RegisterType<IFuelSystemRepository, FuelSystemRepository>(new Unity.Lifetime.ContainerControlledLifetimeManager());
            
            container.RegisterType<ITrackoApiDbContext, TrackoApiDbContext>(lifetimeManager: new PerWebAPIRequestLifetimeManager());/*new PerRequestLifetimeManager()/*,new InjectionConstructor()*/
            container.RegisterType<IDataContextAsync, DataContext>(lifetimeManager: new PerWebAPIRequestLifetimeManager());
            container.RegisterType<IUnitOfWorkAsync, UnitOfWork>(lifetimeManager: new PerWebAPIRequestLifetimeManager());
            if (Helper.HostedOnPremise)
            {
                container.RegisterType<ISendGridEmailService, SMTPMailService>(lifetimeManager: new TransientLifetimeManager());
            }
            else
            {
                container.RegisterSingleton<ISendGridEmailService, SendGridEmailService>();
            }
            container.RegisterSingleton<IIdentityMessageService, IdentityMailService>();
            
            container.RegisterSingleton<IHangfireJobProcessor, HangfireJobProcessor>();
            container.RegisterSingleton(typeof(TSVS.TSVSClient));
            container.RegisterType<IEntityTable<StationeryBookLog>, EntityTable<StationeryBookLog>>(lifetimeManager: new PerWebAPIRequestLifetimeManager(),new InjectionConstructor(new ResolvedParameter<ITrackoApiDbContext>()));
            container.RegisterType<IEntityTable<TrackoApi.Models.Base.Rule>, EntityTable<TrackoApi.Models.Base.Rule>>(lifetimeManager: new PerWebAPIRequestLifetimeManager(), new InjectionConstructor(new ResolvedParameter<ITrackoApiDbContext>()));
            container.RegisterType(typeof(IRepositoryAsync<>), typeof(Repository<>), new PerWebAPIRequestLifetimeManager());
            //container.RegisterType<ModelValidator, UnityModelValidator>(new PerRequestLifetimeManager());
            //DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory((providers, attribute) => {return new DataAnnotationsModelValidator(new UnityModelValidator(providers,attribute));});
            //(metadata, context, attribute) => container.Resolve<ModelValidator>(new ParameterOverrides() { { "metadata", metadata }, { "context", context }, { "attribute", attribute } }));
            //Call External Unity Configuration Library
            // DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory((providers, attribute) => container.Resolve<ModelValidator>(new ParameterOverrides() { { "providers", providers },{ "attribute", attribute } }));
            new TrackoApi.Unity.Configure(container);
        }

        private static ModelValidator Factory(IEnumerable<ModelValidatorProvider> validatorproviders, ValidationAttribute attribute)
        {
            throw new NotImplementedException();
        }
    }
    public class SignalRUnityDependencyResolver : DefaultDependencyResolver
    {
        private IUnityContainer _container;

        public SignalRUnityDependencyResolver(IUnityContainer container)
        {
            _container = container;
            if (_container != null)
            {
                _container.RegisterFactory<ClientHub>(CreateClientHub, FactoryLifetime.Transient);
                //container.RegisterType<ClientHub>(new InjectionFactory(CreateClientHub));
                _container.RegisterType<IClientHub, ClientHub>();
            }
        }

        public override object GetService(Type serviceType)
        {
            if (_container.IsRegistered(serviceType))
            {
                try { return _container.Resolve(serviceType); } catch { return base.GetService(serviceType); }
            }
            return base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            if (_container.IsRegistered(serviceType))
            {
                try { return _container.ResolveAll(serviceType); } catch { return base.GetServices(serviceType); }
            }
            
            return base.GetServices(serviceType);
        }

        private static object CreateClientHub(IUnityContainer p)
        {
            var myHub = new ClientHub(p.Resolve<IGlobalStore>());
            return myHub;
        }
    }
}