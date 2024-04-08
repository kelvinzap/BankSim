using System.ComponentModel.DataAnnotations;

namespace BankSimService.Models.Contracts
{
    public class CreateAccountResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public string DestinationBankCode { get; set; }
        public string DestinationBankName { get; set; }
        public string Description { get; set; }
        public string DestinationAccountName { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string TransactionId { get; set; }
        public string SessionId { get; set; } //from bank for create account
        public string Logo { get; set; }
    }
}
