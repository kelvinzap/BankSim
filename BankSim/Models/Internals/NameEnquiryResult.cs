using BankSim.Models.Contracts;

namespace BankSim.Models.Internals
{
    public class NameEnquiryResult
    {
        public ResponseHeader ResponseHeader { get; set; }
        public NameEnquiryResponse NameEnquiryResponse { get; set; }
    }
}
