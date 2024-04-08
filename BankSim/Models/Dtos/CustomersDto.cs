using BankSim.Pages;

namespace BankSim.Models.Dtos
{
    public class CustomersDto
    {
        public List<CustomerDto> Customers { get; set; }
        public int TotalPages { get; set; }

        public CustomersDto()
        {
            Customers = new List<CustomerDto>();
        }
    }
}
