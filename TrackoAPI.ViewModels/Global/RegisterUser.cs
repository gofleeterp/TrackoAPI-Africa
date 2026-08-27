using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Routing.Constraints;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoAPI.ViewModels
{
    public class RegisterUser:IValidatableObject
    {
        public RegisterUser()
        {
           Roles=new List<long>();
           fk_Address=new vwPostalAddress();
            UserType = 0;
        }
        public long Id { get; set; }
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public long? OfficeId { get; set; }

        public string OfficeName { get; set; }
        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string MiddleName { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
        public virtual vwPostalAddress fk_Address{ get; set; }
        public long? AddressId { get; set; }
        public List<long> Roles { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? BirthDate { get; set; }
        public long? ReportingManagerId { get; set; }
        public string ReportingManager { get; set; }
        public bool IsRoaming { get; set; }
        public string IpAddress { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Id == 0 && string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                yield return new ValidationResult("The Password must be at least 4 characters long.",new []{ "ConfirmPassword" });
            }
        }

        public int UserType { get; set; } = 0;
        public long? DefaultCashAccountId { get; set; }
        public long? DefaultPumpAccountId { get; set; }

        public long? DefaultStoreAccountId { get; set; }
        public long? DefaultFleetManagerId { get; set; }
    }
    public class vwUserName
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
    }
    public class vwRole
    {
        public long Id { get; set; }
        public string RoleName { get; set; }
    }
    public class vwApiRolePermission
    {
        public long Id { get; set; }
       public long ApiObjectId { get; set; }
        public long ApiRoleId { get; set; }
        public string ObjectName { get; set; }
       public int Permission { get; set; }
        public AclType EntityType { get; set; }
        public long? EntitySubTypeId { get; set; }
       
    }

    public class ChangePassword
    {
        /// <summary>
        /// Gets or sets the old password.
        /// </summary>
        /// <value>The old password.</value>
        public string OldPassword { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
