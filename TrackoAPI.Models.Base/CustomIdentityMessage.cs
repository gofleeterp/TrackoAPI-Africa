//using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Core.Helpers;

namespace TrackoApi.Models.Base
{
    public class SendGridEmailViewModel
    {
        public SendGridEmailViewModel()
        {
            From = new EmailAddressModel("team@gofleet.co.in", "GoFleet Africa");
            Tos = new List<EmailAddressModel>();
            Ccs = new List<EmailAddressModel>();
            Bccs = new List<EmailAddressModel>();
            Attachments = new List<AttachmentDetail>();
            ReplyTo = new EmailAddressModel("support@gofleet.co.in", "GoFleet Africa");
            CustomArgs = new Dictionary<string, string>();
        }
        public EmailAddressModel From { get; set; }
        public List<EmailAddressModel> Tos { get; set; }
        public List<EmailAddressModel> Ccs { get; set; }
        public List<EmailAddressModel> Bccs { get; set; }
        public List<AttachmentDetail> Attachments { get; set; }
        public EmailAddressModel ReplyTo { get; set; }
        public string Subject { get; set; }
        public string PlanTextBody { get; set; }
        public string HtmlBody { get; set; }
        public Dictionary<string, string> CustomArgs { get; set; }        
    }

    public class EmailAddressModel
    {
        public EmailAddressModel()
        {

        }
        public EmailAddressModel(string emailAddress) : this(emailAddress, null)
        {
        }
        public EmailAddressModel(string emailAddress, string name)
        {
            EmaillAddress = emailAddress;
            Name = name;
        }
        [Required]
        public string EmaillAddress { get; set; }
        public string Name { get; set; }
    }

    public class AttachmentDetail
    {

        /// <summary>
        /// Gets or sets the Base64 encoded content of the attachment.
        /// </summary>
        [JsonProperty]
        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the mime type of the content you are attaching. For example, application/pdf or image/jpeg.
        /// </summary>
        [JsonProperty]
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the filename of the attachment.
        /// </summary>
        [JsonProperty]
        public string Filename
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content-disposition of the attachment specifying how you would like the attachment to be displayed. For example, "inline" results in the attached file being displayed automatically within the message while "attachment" results in the attached file requiring some action to be taken before it is displayed (e.g. opening or downloading the file). Defaults to "attachment". Can be either "attachment" or "inline".
        /// </summary>
        [JsonProperty]
        public string Disposition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a unique id that you specify for the attachment. This is used when the disposition is set to "inline" and the attachment is an image, allowing the file to be displayed within the body of your email. Ex: <img src="cid:ii_139db99fdb5c3704"></img>
        /// </summary>
        [JsonProperty]
        public string ContentId
        {
            get;
            set;
        }

    }
    public class EmailServiceException : Exception
    {
        public string Body { get; private set; }
        public EmailResponse Response { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public EmailServiceException(HttpStatusCode statusCode, string body) : base(statusCode.ToString())
        {
            Body = body;
            StatusCode = statusCode;
            Response = JsonConvert.DeserializeObject<EmailResponse>(body);
        }

        public EmailServiceException(HttpStatusCode statusCode, string body, Exception innerException) : base(statusCode.ToString(), innerException)
        {
            Body = body;
            StatusCode = statusCode;
            Response = JsonConvert.DeserializeObject<EmailResponse>(body);
        }
    }
    public class EmailErrorMessage
    {
        public string Message { get; set; }
        public string Help { get; set; }
    }

    public class EmailResponse
    {
        public override string ToString()
        {
            return $"HttpStatus:{Status} | \n{Errors?.Select(x => $"{x.Message}=>{x.Help}").JoinStrings("\n")}";
        }
        private HttpStatusCode _status;

        public EmailResponse()
        {
            Status = HttpStatusCode.Accepted;
            Errors = new List<EmailErrorMessage>();
        }
        public DateTime DateSent { get; set; }
        public string UniqueMessageId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode Status
        {
            get => _status; set
            {
                _status = value;
                setMessage(value);
            }
        }
        public bool IsSuccessful
        {
            get
            {
                var status = $"{((int)Status)}";
                return status.StartsWith("2");
            }
        }
        public List<EmailErrorMessage> Errors { get; set; }
        private void setMessage(HttpStatusCode status)
        {
            switch (status)
            {
                case HttpStatusCode.OK:
                    Message = "Your message is valid, but it is not queued to be delivered";
                    break;
                case HttpStatusCode.Created:
                case HttpStatusCode.NonAuthoritativeInformation:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.ResetContent:
                case HttpStatusCode.PartialContent:
                    Message = "The request that you made is valid and successful.";
                    break;
                case HttpStatusCode.Accepted:
                    Message = "Your message is both valid, and queued to be delivered.";
                    break;
                case HttpStatusCode.BadRequest:
                    Message = "There was a problem with your request.";
                    break;
                case HttpStatusCode.Unauthorized:
                    Message = "You do not have authorization to make the request.";
                    break;
                case HttpStatusCode.PaymentRequired:
                    break;
                case HttpStatusCode.Forbidden:
                    break;
                case HttpStatusCode.NotFound:
                    Message = "The resource you tried to locate could not be found or does not exist.";
                    break;
                case HttpStatusCode.MethodNotAllowed:
                    Message = "METHOD NOT ALLOWED";
                    break;
                case HttpStatusCode.RequestEntityTooLarge:
                    Message = "The JSON payload you have included in your request is too large.";
                    break;
                case HttpStatusCode.UnsupportedMediaType:
                    Message = "UNSUPPORTED MEDIA TYPE";
                    break;
                case HttpStatusCode.InternalServerError:
                    Message = "An error occurred when SendGrid attempted to processes it.";
                    break;
                case HttpStatusCode.NotImplemented:
                    break;
                case HttpStatusCode.BadGateway:
                    break;
                case HttpStatusCode.ServiceUnavailable:
                    Message = "The SendGrid v3 Web API is not available.";
                    break;
                default:
                    Message = $"{status}";
                    break;
            }
        }
    }
    public class TextLocalResponce
    {
        public string status { get; set; }
        public decimal Balance { get; set; }
        public string batch_id { get; set; }
        public decimal cost { get; set; }
        public int num_messages { get; set; }
        public List<TextLocalSubRes> messages { get; set; }

    }
    public class TextLocalSubRes
    {
        public string id { get; set; }
        public long recipient { get; set; }

    }
    public class SMSResult
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public HttpStatusCode Status { get; set; }

        public static string GetStatusMessage(int statusCode)
        {
            switch (statusCode)
            {
                case 200: return "success";
                case 101: return "Missing mobile no.";
                case 102: return "Missing message";
                case 103: return "Missing sender ID";
                case 105: return "Missing password";
                case 106: return "Missing Authentication Key";
                case 107: return "Missing Route";
                case 202: return "Invalid mobile number. You must have entered either less than 10 digits or there is an alphabetic character in the mobile number field in API.";
                case 203: return "Invalid sender ID. Your sender ID must be 6 characters, alphabetic.";
                case 207: return "Invalid authentication key. Crosscheck your authentication key from your account’s API section.";
                case 208: return "IP is blacklisted. We are getting SMS submission requests other than your whitelisted IP list.";
                case 301:
                case 402:
                    return "Insufficient balance to send SMS";
                case 302: return "Expired user account. You need to contact your account manager.";
                case 303: return "Banned user account";
                case 306: return "This route is currently unavailable. You can send SMS from this route only between 9 AM - 9 PM.";
                case 307: return "Incorrect scheduled time";
                case 308: return "Campaign name cannot be greater than 32 characters";
                case 309: return "Selected group(s) does not belong to you";
                case 310: return "SMS is too long. System paused this request automatically.";
                case 311: return "Request discarded because same request was generated twice within 10 seconds";
                default:
                    return "unkown response code";
            }
        }
    }

    public class SMSTemplate
    {
        public SMSTemplate()
        {
            Sender = "IWLT";
            Route = "4";
            Country = "91";
            SMS = new List<SMSViewModel>();
        }
        public string Sender { get; set; }
        public string Route { get; set; }
        public string Country { get; set; }
        public string Campaign { get; set; }
        public List<SMSViewModel> SMS { get; set; }
    }
    public class SMSViewModel
    {
        public SMSViewModel()
        {
            Message = "Blank SMS";
            To = new List<string>();
        }
        public SMSViewModel(string message)
        {
            Message = message;
            To = new List<string>();
        }
        [MaxLength(160, ErrorMessage = "Maximum length for SMS is 160 characters")]
        public string Message { get; set; }
        public List<string> To { get; set; }
        [JsonIgnore]
        public string Callback { get; set; }
    }
}
