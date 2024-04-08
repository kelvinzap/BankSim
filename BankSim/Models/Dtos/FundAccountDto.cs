using System.ComponentModel.DataAnnotations;

namespace BankSim.Models.Dtos
{
    public class FundAccountDto
    {
        public string AccountId { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}
