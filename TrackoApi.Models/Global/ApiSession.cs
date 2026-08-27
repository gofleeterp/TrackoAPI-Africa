using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackoApi.Models.Global
{
    public class ApiSession:Base.Entity
    {
        [Key]
        public override long Id { get; set; }
        [Required, Index("IDX_Search_ApiSession",Order =1)]
        public long UserId { get; set; }
        [Required]
        public DateTime StartDateTime { get; set; }
        [Index("IDX_Search_ApiSession", Order = 3)]
        public DateTime? EndDateTime { get; set; }
        [MaxLength(200)]
        public string UserIp { get; set; }
        [MaxLength(200)]
        public string HostName { get; set; }
        [MaxLength(200)]
        public string Origin { get; set; }
        [Required]
        [MaxLength(200),Index("IDX_Search_ApiSession", Order = 2)]
        public string ApplicationId { get; set; }
        [MaxLength(255)]
        public string ConnectionId { get; set; }
        [MaxLength(100)]
        public string AppVersion { get; set; }
        [MaxLength(200)]
        public string OSName { get; set; }
    }

    public class ApiRecordAccessLog:Base.Entity
    {
        [Key]
        public override long Id { get; set; }
        [Required]
        public long UserId { get; set; }
        [Required]
        public long  RecordId { get; set; }
        [MaxLength(200)]
        public string RecordName { get; set; }
        [Required]
        public long ViewId { get; set; }
        public AccessType Type { get; set; }
        public DateTime? TimeStamp { get; set; }
        public long SessionId { get; set; }
        public bool IsMaster { get; set; }
        public string Changes { get; set; }
        public string MachineName { get; set; }
        public string LocalIp { get; set; }
        public string OSUserName { get; set; }
        public long OwnerSessionId{ get; set; }
        public string AuditRemark { get; set; }
    }

    
}
