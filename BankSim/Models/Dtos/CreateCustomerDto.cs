using System.ComponentModel.DataAnnotations;

namespace BankSim.Models.Dtos
{
    public class CreateCustomerDto
    {
        [Required]                
        [Display(Name = "Firstname")]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string MiddleName { get; set; }
        [Required]
        [Display(Name = "Date of birth")]

        public DateTime? DateOfBirth { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^(090|091|080|081|070|071)\d{8}$", ErrorMessage = "Invalid phone number")] //regex expression to validate phone numbers
        public string PhoneNumber { get; set; }
        [Required]
        [MinLength(10, ErrorMessage = "BVN must be 10 digits")]
        [MaxLength(10, ErrorMessage = "BVN must be 10 digits")]
        
        public string BVN { get; set; }
        [Required]
        [MinLength(10, ErrorMessage = "NIN must be 10 digits")]
        [MaxLength(10, ErrorMessage = "NIN must be 10 digits")]        
        public string NIN { get; set; }
        [Required]
        [Display(Name = "KYC Level")]

        public string KYCLevel { get; set; }
        [Required]
        [MinLength(10, ErrorMessage = "Account Number must be 10 digits")]
        [MaxLength(10, ErrorMessage = "Account Number must be 10 digits")]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }
        [Required]
        [Display(Name = "Account Type")]
        public string AccountType { get; set; }
        [Required]
        public decimal Balance { get; set; }
    }
}
