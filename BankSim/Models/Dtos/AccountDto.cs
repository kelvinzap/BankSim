using System.ComponentModel.DataAnnotations;

namespace BankSim.Models.Dtos
{
    public class AccountDto
    {
        public int Id { get; set; }
        [Required]
        [MinLength(10, ErrorMessage = "Account Number must be 10 digits")]
        [MaxLength(10, ErrorMessage = "Account number must be 10 digits")]
        public string AccountNumber { get; set; }
        public int CustomerId { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDate { get; set; }
        public string AccountName { get; set; }
    }
}
