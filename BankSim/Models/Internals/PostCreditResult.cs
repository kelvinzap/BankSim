using BankSim.Models.Contracts;

namespace BankSim.Models.Internals
{
    public class PostCreditResult
    {
        public ResponseHeader ResponseHeader { get; set; }
        public PostCreditResponse PostCreditResponse { get; set; }
    }
}
