using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using TrackoApi.Models.Global;

namespace TrackoAPI.Infrastructure
{
    public class ApiUserValidator: UserValidator<ApiUser, long>
    {
        public ApiUserValidator(ApiUserManager manager) : base(manager)
        {
            RequireUniqueEmail = false;
            Manager = manager;
        }

        public bool EmailIsOptional{ get; set; }
        private ApiUserManager Manager { get; set; }
        public override async Task<IdentityResult> ValidateAsync(ApiUser item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            var errors = new List<string>();
            await ValidateUserName(item, errors);
            if (RequireUniqueEmail)
            {
                await ValidateEmail(item, errors);
            }
            if (errors.Count > 0)
            {
                return IdentityResult.Failed(errors.ToArray());
            }
            return IdentityResult.Success;
            //return base.ValidateAsync(item);
        }
        private async Task ValidateUserName(ApiUser user, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add(String.Format(CultureInfo.CurrentCulture, "Invalid Length", "UserName"));
            }
            else if (AllowOnlyAlphanumericUserNames && !Regex.IsMatch(user.UserName, @"^[A-Za-z0-9@_\.]+$"))
            {
                // If any characters are not letters or digits, its an illegal user name
                errors.Add(String.Format(CultureInfo.CurrentCulture, "User Name should be alfa numeric", user.UserName));
            }
            else
            {
                var owner = await Manager.FindByNameAsync(user.UserName);
                if (owner != null && !EqualityComparer<long>.Default.Equals(owner.Id, user.Id))
                {
                    errors.Add(String.Format(CultureInfo.CurrentCulture, $"UserName {user.UserName} already Exists.", user.UserName));
                }
            }
        }

        // make sure email is not empty, valid, and unique
        private async Task ValidateEmail(ApiUser user, List<string> errors)
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    errors.Add(String.Format(CultureInfo.CurrentCulture, "Invalid Length", "Email"));
                    return;
                }
                try
                {
                    var m = new MailAddress(user.Email);
                }
                catch (FormatException)
                {
                    errors.Add(String.Format(CultureInfo.CurrentCulture, "InvalidEmail Formait", "Email"));
                    return;
                }
                if (RequireUniqueEmail)
                {
                    var owner = await Manager.FindByEmailAsync(user.Email);
                    if (owner != null && !EqualityComparer<long>.Default.Equals(owner.Id, user.Id))
                    {
                        errors.Add(String.Format(CultureInfo.CurrentCulture, "Email is associated with other user",
                            "Email"));
                    }
                }
            }
            if (!EmailIsOptional)
            {
                errors.Add(String.Format(CultureInfo.CurrentCulture, "Email is Required",
                            "Email"));
            }
        }
    }
}
