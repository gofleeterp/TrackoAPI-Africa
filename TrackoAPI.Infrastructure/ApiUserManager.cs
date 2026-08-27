using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Threading.Tasks;
using TrackoApi.Core;
using TrackoApi.Data;
using TrackoApi.MessageService;
using TrackoApi.Models.Global;

namespace TrackoAPI.Infrastructure
{
    public class ApiUserManager: UserManager<ApiUser,long>
    {
        private readonly IGlobalStore _gs;

        public ApiUserManager(IUserStore<ApiUser,long> store, IIdentityMessageService emailService,ISMSService smsService,IGlobalStore globalStorage)
            : base(store)
        {
            Setup(emailService,smsService);
            _gs = globalStorage;
        }
        public override Task<ApiUser> FindByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return base.FindByEmailAsync(email);
        }

        private void Setup(IIdentityMessageService emailService,ISMSService smsService)
        {
            this.UserValidator = new ApiUserValidator(this)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = false,
                EmailIsOptional = true
            };
            PasswordValidator = new PasswordValidator
            {
                RequiredLength = 5,
                RequireNonLetterOrDigit = true,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };
            EmailService = emailService;
            SmsService = smsService;
            //var dataProtectionProvider = options.DataProtectionProvider;
            //if (dataProtectionProvider != null)
            //{
            //    appUserManager.UserTokenProvider = new DataProtectorTokenProvider<ApiUser, long>(dataProtectionProvider.Create("ASP.NET Identity"))
            //    {
            //        //Code for email confirmation and reset password life time
            //        TokenLifespan = TimeSpan.FromHours(6)
            //    };
            //}
        }
        public static ApiUserManager Create(IdentityFactoryOptions<ApiUserManager> options, IOwinContext context, IIdentityMessageService emailService,ISMSService smsService, IGlobalStore globalStorage)
        {
            var appDbContext = new TrackoApiDbContext(globalStorage);
            var appUserManager = new ApiUserManager(new UserStore<ApiUser,ApiRole,long,ApiUserLogin,ApiUserRole,ApiUserClaim>(appDbContext), emailService,smsService, globalStorage);

            // Configure validation logic for usernames
            appUserManager.UserValidator = new UserValidator<ApiUser,long>(appUserManager)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = false
            };

            // Configure validation logic for passwords
            appUserManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = false,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            appUserManager.EmailService = emailService;
            appUserManager.SmsService = smsService;
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                appUserManager.UserTokenProvider = new DataProtectorTokenProvider<ApiUser,long>(dataProtectionProvider.Create("ASP.NET Identity"))
                {
                    //Code for email confirmation and reset password life time
                    TokenLifespan = TimeSpan.FromHours(6)
                };
            }
            return appUserManager;
        }
    }
}
