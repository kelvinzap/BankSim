﻿namespace BankSimService.Models.Contracts
{
    public class TransactionQueryResponse
    {
            public string SessionId { get; set; }
            public string PaymentRef { get; set; }
            public string DestinationInstitutionId { get; set; }
            public string CreditAccount { get; set; }
            public string CreditAccountName { get; set; }
            public string SourceAccountId { get; set; }
            public string SourceAccountName { get; set; }
            public string Narration { get; set; }
            public string Channel { get; set; }
            public string Group { get; set; }
            public string Sector { get; set; }
            public decimal Amount { get; set; }
            public DateTime TransactionDate { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }      
    }
}
