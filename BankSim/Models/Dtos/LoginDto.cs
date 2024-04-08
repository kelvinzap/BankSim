using System.ComponentModel.DataAnnotations;

namespace BankSim.Models.Dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
