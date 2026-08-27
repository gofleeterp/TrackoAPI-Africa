using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using TrackoApi.Models.Base;

namespace TrackoApi.MessageService
{
    public interface ISMSService: IIdentityMessageService
    {
        Task<SMSResult> SendAsync(string message, string sender = "IWLT", long userId = 0, string tenantId = null, params string[] recivers);
        Task<SMSResult> SendAsync(SMSTemplate sms,long userId= 0, string tenantId = null);
    }
}