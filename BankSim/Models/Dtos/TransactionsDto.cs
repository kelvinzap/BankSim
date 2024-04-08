namespace BankSim.Models.Dtos
{
    public class TransactionsDto
    {
        public List<TransactionDto> Transactions { get; set; }
        public int TotalPages { get; set; }

        public TransactionsDto()
        {
            Transactions = new List<TransactionDto>();
        }
    }
}
