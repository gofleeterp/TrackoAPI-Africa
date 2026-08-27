using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("ApiAppClient")]
    public class ApiAppClient:Entity
    {
        public ApiAppClient(){
            IsActive = true;
            RefreshTokenLifeTime = 1;
            AllowedOrigin = "*";
        }
        [Key]
        public override long Id { get; set; }
        [MaxLength(300), Required, Index("IDX_Search_ApiAppClient", Order = 2)]
        public string ClientKey { get; set; }
        [Required, Index("IDX_Search_ApiAppClient", Order = 3), MaxLength(500)]
        public string Secret { get; set; }
        [Required,MaxLength(100), Index("IX_ClientApi_AppId", IsUnique = true),Index("IDX_Search_ApiAppClient",Order =1)]
        public string ApplicationId { get; set; }
        [MaxLength(200)]
        public string AllowedOrigin { get; set; }
        [Range(1, 24, ErrorMessage = "Refresh token hour can be between 1 to 24 only")]
        public long RefreshTokenLifeTime { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(100)]
        public string MinimumSupportedVersion { get; set; }
        [MaxLength(100)]
        public string MaximumSupportedVersion { get; set; }
    }
    
    

    [Table("ApiRefreshToken")]
    public class ApiRefreshToken
    {
        public ApiRefreshToken() { }
        [Key]
        public string Id { get; set; }
        [Required,MaxLength(50),Index("IX_RefToken_Sub_Key", 1, IsUnique = true)]
        public string Subject { get; set; }
        [Required, MaxLength(50),Index("IX_RefToken_Sub_Key", 2, IsUnique = true)]
        public string ClientKey { get; set; }

        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        [Required]
        public string ProtectedTicket { get; set; }
    }
}
