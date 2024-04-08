using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BankSim.Models.Dtos
{
    public class TransferDto
    {
        [Required]
        [Display(Name = "Destination Bank")]
        public string DestinationBankCode { get; set; }
        [Required]
        [MinLength(10, ErrorMessage = "Account Number must be 10 digits")]
        [MaxLength(10, ErrorMessage = "Account Number must be 10 digits")]
        [Display(Name = "Account Number")]
        public string CreditAccount { get; set; }
        
        [Required]
        [Display(Name = "Account")]

        public string DebitAccount { get; set; }
        public string CreditAccountName { get; set; }        
        [Required]
        [Display(Name = "Narration")]

        public string Description { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}
