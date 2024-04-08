using System.ComponentModel.DataAnnotations;

namespace BankSimService.Models.Contracts
{
    public class CreateAccountRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string AccountName { get; set; }
        public string TransactionId { get; set; }
    }
}
