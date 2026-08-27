using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackoAPI.ViewModels
{
    public class RegisterTenant
    {
        public RegisterTenant()
        {
            Applications=new List<ApiApplication>();
        }
        [MaxLength(200),Required]
        public string SecretPhrase { get; set; }
        [MaxLength(300)]
        public string DatabaseConnectionString { get; set; }
        [MaxLength(150),Required]
        public string TenantName { get; set; }
        [RegularExpression(@"[A-Z]{5}\d{4}[A-Z]{1}", ErrorMessage = "* Invalid PAN Number")]
        public string PAN { get; set; }
        [MaxLength(20),Required]
        public string AdminUserName { get; set; }
        [Compare("ConfirmedPassword"),DataType(DataType.Password),MaxLength(200)]
        public string Password { get; set; }
        [DataType(DataType.Password),MaxLength(200)]
        public string  ConfirmedPassword { get; set; }
        [Required,MinLength(20,ErrorMessage = "Oppsss..It looks like you have entred wrong postal address.Please correct it."),MaxLength(200)]
        public string PostalAddress { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required, DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        public int AccessCode { get; set; }
        public virtual List<ApiApplication> Applications { get; set; }
        [MaxLength(225), Required]
        public string ServerUrl { get; set; }
        [MaxLength(225), Required]
        public string SetupUrl { get; set; }
        [MaxLength(225), Required]
        public string FormatUrl { get; set; }
    }

    public class ApiApplication
    {
        public ApiApplication()
        {
            RefreshTokenLifeTime = 8;
            AllowedOrigin = "*";
            NoOfActiveUsers = 5;
        }

        public int NoOfActiveUsers { get; set; }

        public string ApplicationId { get; set; }
        public string AllowedOrigin { get; set; }
        public int RefreshTokenLifeTime { get; set; }
    }

    public class TenantResult
    {
        public string ClientKey { get; set; }
        public string ClientSecret { get; set; }
        public string ApplicationId { get; set; }
    }
  

    public class vwApiDevice:IValidatableObject
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ISP { get; set; }
        public string Location { get; set; }
        public string DeviceIdentity { get; set; }
        public string ComputerName { get; set; }
        public string LocalHostIp { get; set; }
        public string PublicHostIp { get; set; }
        public string Remark { get; set; }
        public string ClientId { get; set; }
        public string ApplicationId { get; set; }
        /// <summary>
        /// WindowsPCOS=0,
        /// Android=1,
        /// Mac=2,
        /// Linux=3,
        /// WindowsPhoneOS=4,
        /// iOS=5
        /// Web=6
        /// </summary>
        public int? DeviceOSId { get; set; } = 0;

        [Display(Name = "New PIN")]
        public int Pin { get; set; } = 0000;

        [Compare("Pin", ErrorMessage = "New PIN Does not match with confirmed pin")]
        public int ConfirmPin { get; set; } = 0000;

        #region Implementation of IValidatableObject

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var mobiledevices=new List<int>(){1,4,5};
            if (mobiledevices.Contains(this.DeviceOSId.GetValueOrDefault(0))&&(Pin!=ConfirmPin||ConfirmPin.ToString().Length!=4||ConfirmPin==0000))
            {
                yield return new ValidationResult($"Invalid PIN {this.ConfirmPin}.PIN should not be 0000 and New PIN and Confirm PIN should match and PIN Length should be 4 digit");
            }
        }

        #endregion
    }

}
