namespace BankSim.Models.Internals
{
    public class TransactionFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string SessionId { get; set; }
        public string Institution { get; set; }
        
    }
}
