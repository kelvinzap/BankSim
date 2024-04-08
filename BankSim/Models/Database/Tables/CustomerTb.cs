using System.ComponentModel.DataAnnotations;

namespace BankSim.Models.Database.Tables
{
    public class CustomerTb
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string BVN { get; set; }
        public string NIN { get; set; }
        public string KYCLevel { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
