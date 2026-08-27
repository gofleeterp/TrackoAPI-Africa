using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models
{
    [Table("ThirdPartyToken")]
    public class ThirdPartyToken
    {
        public ThirdPartyToken()
        {

        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), MaxLength(100)]
        public string Token { get; set; }
        [MaxLength(200)]
        public string Appidentity { get; set; }
        public string TenantId { get; set; }
        /// <summary>
        /// Calling Interval in Minutes
        /// </summary>
        public int Interval { get; set; } = 20;

        public DateTime? ExpiryDate { get; set; }
        /// <summary>
        /// Controller=Action keyValue pair comma seperated
        /// e.g {{integration,simpleeventpost},{integration,getvtsstatus}} OR put * to bypass this filter
        /// </summary>
        public string AllowedPath { get; set; }
        public string JsonMetaData { get; set; }
        public bool IsDeactivated { get; set; }
        public DateTime? LastCalledTime { get; set; }
        public bool IsValidCall(out string ErrorMessage, string controller = null, string action = null)
        {
            ErrorMessage = "";
            if (LastCalledTime != null && (LastCalledTime.Value.AddMinutes(Interval) > DateTime.Now) || (DateTime.Now.Subtract(LastCalledTime.Value).TotalMinutes < Interval))
            {
                ErrorMessage = $"Next Call would be allowed after {LastCalledTime.Value.AddMinutes(Interval):dd-MMM-yyyy HH:mm:ss}. left minutes {DateTime.Now.Subtract(LastCalledTime.Value).TotalMinutes}";
                return false;
            }
            if (ExpiryDate != null && ExpiryDate < DateTime.Now)
            {
                ErrorMessage = $"Token has been Expired";
                return false;
            }
            if (IsDeactivated)
            {
                ErrorMessage = $"Token has been revoked";
                return false;
            }
            if (string.IsNullOrWhiteSpace(AllowedPath))
            {
                AllowedPath = "*";
            }
            if (AllowedPath != "*")
            {
                if (string.IsNullOrWhiteSpace(controller))
                {
                    ErrorMessage = $"Unauthorized access of EndPoint {controller}. ErrorCode:304";
                    return false;
                }
                try
                {
                    var dics = JsonConvert.DeserializeObject<Dictionary<string, string>>(AllowedPath);
                    if (dics != null)
                    {
                        if (!dics.ContainsKey(controller))
                        {
                            ErrorMessage = $"Unauthorized access of EndPoint {controller}. ErrorCode:314";
                            return false;
                        }
                        if (!string.IsNullOrWhiteSpace(action) && action != "*")
                        {
                            if (dics.TryGetValue(controller, out string actionName) && action.Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                ErrorMessage = $"Unauthorized access of EndPoint {controller}/{action}. ErrorCode:319";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessage = $"Unauthorized access of EndPoint {controller}/{action}. ErrorCode:326";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.GetBaseException().Message;
                    return false;
                }
            }
            if (string.IsNullOrWhiteSpace(TenantId))
            {
                ErrorMessage = "Token is not configured";
                return false;
            }
            return true;
        }

    }
}
