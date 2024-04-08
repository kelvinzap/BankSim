using System.ComponentModel.DataAnnotations;

namespace BankSim.Models.Database.Tables
{
    public class AccountTb
    {
        [Key]
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public string AccountName { get; set; }
        public decimal MaximumBalance { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
