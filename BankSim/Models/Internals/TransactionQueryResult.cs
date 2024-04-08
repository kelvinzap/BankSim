using BankSim.Models.Contracts;

namespace BankSim.Models.Internals
{
    public class TransactionQueryResult
    {
        public ResponseHeader ResponseHeader { get; set; }
        public TransactionQueryResponse TransactionQueryResponse { get; set; }
    }
}
