namespace BankSimService.Models.Contracts
{
    public class NameEnquiryResponse
    {
            public string SessionId { get; set; }
            public string DestinationInstitutionId { get; set; }
            public string AccountId { get; set; }
            public string AccountName { get; set; }
            public string Status { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }
            public string Bvn { get; set; }
            public string KycLevel { get; set; }
            public string AccountType { get; set; }
        
    }
}
