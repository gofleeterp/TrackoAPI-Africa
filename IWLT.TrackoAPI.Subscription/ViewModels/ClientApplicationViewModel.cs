using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Tenant.Models;

namespace IWLT.TrackoAPI.Subscription.ViewModels
{
    public class ClientAppInfoViewModel
    {
        public string TenantId { get; set; }
        [MaxLength(100)/*, Index("IX_Tenant_Name", IsUnique = true)*/, Required]
        public string TenantName { get; set; }
        [MaxLength(300),/*Index("IX_Tenant_ClientKey",IsUnique = true),*/Required]
        public string TenantKey { get; set; }
        [MaxLength(100)]
        public string TenantShortName { get; set; }
        public bool IsActive { get; set; }
        public LogType LogType { get; set; }
        public string ServerUrl { get; set; }
        public bool IsSingleUserMode { get; set; } = false;
        public int AccessCode { get; set; }
        public string AppId { get; set; }
        [MaxLength(50)]
        public string AppName { get; set; }
        public ApplicationCategory AppType { get; set; }
        public string UpdateUrl { get; set; }
        public int NoOfActiveUsers { get; set; }
        public string ClientSecret { get; set; }
    }

}
