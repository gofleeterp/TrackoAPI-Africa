using System.ComponentModel.DataAnnotations;

namespace TrackoAPI.ViewModels.Global
{
    public class BatchVerification
    {
        [Range(1,99999999,ErrorMessage = "Value should be greater than zero")]
        public long ProcId { get; set; }
        [Key]
        public long TransactionId { get; set; }
        [Required(AllowEmptyStrings =false)]
        public string TransactionNumber { get; set; }

        public int TransactionType { get; set; }
        public string JsonData { get; set; }
    }
}
