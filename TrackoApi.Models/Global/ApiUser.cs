using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Web.OData.Builder;

using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    public class ApiUser:IdentityUser<long,ApiUserLogin,ApiUserRole,ApiUserClaim>, IAuditableInfraEntity
    {
        public ApiUser(string user):this()
        {
            UserName = user;
        }

        public ApiUser()
        {
            ResourceAccessLogs = new List<ApiResourceAccessLog>();
            TeamMembers = new List<ApiUser>();
            IpUserMappings = new List<IpUserMapping>();
            Connections = new List<UserConnection>();
            Groups = new List<ConversationGroup>();
           // this.Id = Guid.NewGuid().ToString();
        }
        [Index("IDX_Search_ApiUser"),MaxLength(100)]
        public override string UserName { get; set; }
        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string MiddleName { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? JoinDate { get; set; }
        public long? OfficeId { get; set; }
        //[ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        public long? AddressId { get; set; }
        //[ForeignKey("AddressId")]
        public virtual PostalAddress fk_Address { get; set; }
        public virtual List<ApiResourceAccessLog> ResourceAccessLogs { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;
        [ForeignKey("ReportingManagerId")]
        public virtual ApiUser fk_ReportingManager { get; set; }
        public long? ReportingManagerId { get; set; }
        public virtual List<ApiUser> TeamMembers { get; set; }
        public bool IsSuspended { get; set; }
        public UserType TypeId { get; set; }=UserType.User;
        public virtual List<IpUserMapping> IpUserMappings { get; set; }
        public bool IsRoamingUser { get; set; }
        public long CreatedSessionId { get; set; } = 0;
        public DateTime CreatedDOE { get; set; }
        public long? ModifiedSessionId { get; set; }
        public DateTime? ModifiedDOE { get; set; }
        [MaxLength(100)]
        public string SecuredByTenantId { get; set; }
        //[MinLength(10),MaxLength(10)]//TODO:Index("IDX_ApiDevice_Unique", IsUnique = true)
        //public string MobileNo { get; set; }

        public virtual List<UserConnection> Connections { get; set; }
        public virtual List<ConversationGroup> Groups { get; set; }
        public long? DefaultCashAccountId { get; set; }
        public long? DefaultPumpAccountId { get; set; }

        public long? DefaultStoreAccountId { get; set; }
        public long? DefaultFleetManagerId { get; set; }
        public override ICollection<ApiUserRole> Roles => base.Roles;
        [IgnoreDataMember]
        public override string PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }
        [IgnoreDataMember]
        public override string SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }
    }
    [Table("ApiDevices")]
    public class ApiDevice : Entity
    {[MaxLength(200), Index("IDX_Search_ApiDevice", Order = 1)]
        public string DeviceIdentity { get; set; }
        [MaxLength(200)]
        public string ComputerName { get; set; }
        [MaxLength(200)]
        public string LocalHostIp { get; set; }
        [MaxLength(200)]
        public string PublicHostIp { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; }
        [MaxLength(200)]
        public string OTP { get; set; }
        public bool IsVerified { get; set; }
        [MaxLength(100)]
        public string UserName { get; set; }
        [MaxLength(200)]
        public string ISP { get; set; }
        [MaxLength(200)]
        public string Location { get; set; }
        public DeviceOS DeviceOS { get; set; }
        public int PIN { get; set; }
        [MaxLength(200)]
        public string ApplicationId { get; set; }
    }

    public enum DeviceOS
    {
        WindowsPCOS = 0,
        Android = 1,
        Mac=2,
        Linux=3,
        WindowsPhoneOS=4,
        iOS=5
    }
    [Table("ApiIpUserMappings")]
    public class IpUserMapping:AuditableEntity
    {
        [MaxLength(200)]
        public string IPAddress { get; set; }
        public long UserId { get; set; }
    }
    public enum UserType
    {
        SuperAdmin=100,
        Admin=200,
        User=0,
        Agent=1,
        Client=2,
        Vendor=3,
        ExternalAuditor=4
    }
    public class ApiRole : IdentityRole<long,ApiUserRole>, IAuditableInfraEntity
    {
        public ApiRole()
        {
            AccessList = new List<ApiRolePermission>();
        }
        public virtual List<ApiRolePermission> AccessList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;
        public long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApiUser fk_PrivateUser { get; set; }

        public long CreatedSessionId { get; set; } = 0;
        public DateTime CreatedDOE { get; set; }
        public long? ModifiedSessionId { get; set; }
        public DateTime? ModifiedDOE { get; set; }
        [MaxLength(100)]
        public string SecuredByTenantId { get; set; }
    }

    public class ApiUserClaim : IdentityUserClaim<long>
    {
        public ApiUserClaim() { }
    }

    public class ApiUserLogin : IdentityUserLogin<long>
    {
        public ApiUserLogin() { }
    }

    public class ApiUserRole : IdentityUserRole<long>, IAuditableInfraEntity
    {
        public ApiUserRole() { }
        [ForeignKey("fk_User")]
        public override long UserId { get; set; }
        
        public virtual ApiUser fk_User { get; set; }
        [ForeignKey("fk_Role")]
        public override long RoleId { get; set; }
        public virtual ApiRole fk_Role { get; set; }
        public long CreatedSessionId { get; set; } = 0;
        public DateTime CreatedDOE { get; set; }
        public long? ModifiedSessionId { get; set; }
        public DateTime? ModifiedDOE { get; set; }
        [MaxLength(100)]
        public string SecuredByTenantId { get; set; }
    }

    public class ApiRolePermission:AuditableEntity
    {
        public ApiRolePermission()
        {
           
        }
        [Index("IX_RoleAcl_Bridge_RoleId", 1, IsUnique = true)]
        public long ApiObjectId { get; set; }
        [Index("IX_RoleAcl_Bridge_RoleId",2,IsUnique = true),ForeignKey("ApiRole")]
        public long ApiRoleId { get; set; }
        public virtual ApiRole ApiRole { get; set; }
        [MaxLength(200)]
        public string ObjectName { get; set; }
        [Required]
        public int Permission { get; set; }
        [Column("EntityTypeId"),Index("IX_RoleAcl_Bridge_RoleId", 3, IsUnique = true)]
        public AclType EntityType { get; set; }
        public long? EntitySubTypeId { get; set; }
    }
    
    [Table("ApiRAL")]
    public class ApiResourceAccessLog:Entity
    {
        
        [Index("IX_ApiResourceAccessLog_ResourceId_UserId",1,IsUnique = true)]
        public long ResourceId { get; set; }
        [MaxLength(200)]
        public string ResourceName { get; set; }
        [Index("IX_ApiResourceAccessLog_ResourceId_UserId", 2, IsUnique = true)]
        public long UserId { get; set; }
        public DateTime LastAccessDateTime { get; set; }
        [Column("ResourceTypeId")]
        [Index("IX_ApiResourceAccessLog_ResourceId_UserId", 3, IsUnique = true)]
        public AclType ResourceType { get; set; }
        [MaxLength(200)]
        public string ApplicationId { get; set; }
        public int Count { get; set; }
        
        public string DefaultData { get; set; }
    }
    [Table("ApiUserConnection")]
    public class UserConnection
    {
        [Key,MaxLength(255)]
        public string ConnectionId { get; set; }
        [MaxLength(255)]
        public string UserAgent { get; set; }
        public bool Connected { get; set; }
        public long UserId { get; set; }
        [ForeignKey("Userid")]
        public virtual ApiUser fk_User{ get; set; }
    }
    [Table("ApiConversationGroup")]
    public class ConversationGroup
    {
        public ConversationGroup()
        {
            Users = new List<ApiUser>();
        }
        [Key]
        public long Id { get; set; }
        [MaxLength(255)]
        public string GroupName { get; set; }

        //public bool IsTemporary { get; set; } = true;
        public virtual List<ApiUser> Users { get; set; }
    }
    [Table("tApiPubSubStore")]
    public class ApiPubSubStore
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// Sender User id
        /// </summary>
        public long SenderId { get; set; } = 0;
        /// <summary>
        /// Receiver User id
        /// </summary>
        public long ReceiverId { get; set; } = 0;
        /// <summary>
        /// ApiView Id from where the Record was updated
        /// </summary>
        public long RecordTypeId { get; set; } = 0;
        [MaxLength(50)]
        public string RecordKey { get; set; }
        [MaxLength(200)]
        public string Subject { get; set; }
        [DataType(DataType.Text)]
        public string Message { get; set; }
        /// <summary>
        /// 0:Added,1:Modified,3:Deleted
        /// </summary>
        public int ActionType { get; set; } = 0;
        public ApiPubSubStore Clone()
        {
            return (ApiPubSubStore)this.MemberwiseClone();
        }
        public string EventName { get; set; }
    }
    
   
}
