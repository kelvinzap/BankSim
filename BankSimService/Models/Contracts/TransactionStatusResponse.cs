namespace BankSimService.Models.Contracts
{
    public class TransactionStatusResponse
    {
            public string SourceBankCode { get; set; }
            public string SourceAccountName { get; set; }
            public string SourceBankName { get; set; }
            public string SourceBankAccountNumber { get; set; }
            public decimal Amount { get; set; }
            public string DestinationBankCode { get; set; }
            public string DestinationBankName { get; set; }
            public string DestinationAccountName { get; set; }
            public string DestinationAccountNumber { get; set; }
            public string TraceId { get; set; }
            public string TransactionId { get; set; }
            public string SessionId { get; set; }
            public string Terminal { get; set; }
            public string Description { get; set; }
            public DateTime PaymentDate { get; set; }
              
    }
}
