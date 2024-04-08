namespace BankSim.Models.Internals
{
    public class CustomerFilter
    {
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string CustomerName { get; set; }
        public DateTime? RegistrationDate { get; set; }
    }
}
