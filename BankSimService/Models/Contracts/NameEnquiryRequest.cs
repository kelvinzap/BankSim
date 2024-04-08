namespace BankSimService.Models.Contracts
{
    public class NameEnquiryRequest
    {
            public string SessionId { get; set; }
            public string DestinationInstitutionId { get; set; }
            public string AccountId { get; set; }

    }
}
