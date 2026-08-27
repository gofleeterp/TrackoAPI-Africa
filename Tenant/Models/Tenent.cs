using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;
using TrackoAPI.Models.Shared;

namespace Tenant.Models
{
	public class TenantMaster
	{
		public TenantMaster()
		{
			IsActive = true;
			Id = Guid.NewGuid().ToString("N");
			Applications=new List<Application>();
			Apps = new List<TenantApplicationMapping>();
		}
		[Key]
		public string Id { get; set; }
		[MaxLength(100), Index("IX_Tenant_Name", IsUnique = true),Required]
		public string Name { get; set; }
		[MaxLength(300),Index("IX_Tenant_ClientKey",IsUnique = true),Required]
		public string ClientKey { get; set; }
		[MaxLength(100)]
		public string ShortName { get; set; }
		public string Secret { get; set; }
		public bool IsActive { get; set; }
		[Required,IgnoreDataMember]
		public string ConnectionString { get; set; }
		[Required, MinLength(20, ErrorMessage = "Oppsss..It looks like you have entred wrong address.Please correct it.")]
		public string PostalAddress { get; set; }

		/*Commented by sanjay 2023-05-01*/
		//[RegularExpression(@"[A-Z]{5}\d{4}[A-Z]{1}", ErrorMessage = "* Invalid PAN Number"),MaxLength(10),MinLength(10),Index("IDX_Tenant_PAN",IsUnique = true)]
		public string PANNo { get; set; }
		[Required]
		public string PhoneNumber { get; set; }
		[Required,DataType(DataType.EmailAddress)]
		public string EmailAddress { get; set; }
		[MaxLength(200)]
		public string WebAddress { get; set; }
		public virtual ICollection<Application> Applications { get; set; }
		public virtual ICollection<TenantApplicationMapping> Apps { get; set; }
		public LogType LogType { get; set; }
		public string ServerUrl { get; set; }
		[MaxLength(500)]
		public string RemoteBackupPath { get; set; }

		public bool IsSingleUserMode { get; set; } = false;
		public int AccessCode { get; set; }
		public bool IsHostedOnPremise { get; set; } = false;
		public int ConstCurTypeId { get; set; } = 0;
        public DateTime? InstallationDate { get; set; }
    }
	
	public class Application
	{
		public Application()
		{
			IsActive = true;
			Id=Guid.NewGuid().ToString("D");
			Tenants=new List<TenantMaster>();
		}
		[Key]
		public string Id { get; set; }
		[Index("IX_Application_AppName",IsUnique = true),MaxLength(50)]
		public string ApplicationName { get; set; }
		public ApplicationCategory ApplicationType { get; set; }
		public bool IsActive { get; set; }
		public virtual ICollection<TenantMaster> Tenants { get; set; } 
		public string UpdateUrl{get;set;}
	}

	public class TenantApplicationMapping
	{
		public long Id { get; set; }
		public string ApplicationId { get; set; }
		[ForeignKey("ApplicationId")]
		public virtual Application fk_Application { get; set; }
		public string TenantId { get; set; }
		[ForeignKey("TenantId")]
		public virtual TenantMaster fk_Tenant { get; set; }

		public bool IsActive { get; set; }
		public string UpdateUrl { get; set; }
        public string FormatUrl { get; set; }
        public string SetupUrl { get; set; }
        public int NoOfActiveUsers { get; set; }
    }
	
}
