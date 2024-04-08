using BankSim.Models;
using BankSim.Models.Database;
using BankSim.Models.Database.Tables;
using BankSim.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace BankSim.Actions;

public interface ICustomer
{
    Task<CustomersDto> GetAllCustomersAsync(PaginationFilter paginationFilter = null, CustomerFilter customerFilter = null);
    Task<CustomerDto> GetCustomerDetailsAsync(string id);
    Task<CreateCustomerResult> CreateCustomer(CreateCustomerDto dto);
}
public class Customer : ICustomer
{
    private readonly IConfiguration _configuration;

    public Customer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<CreateCustomerResult> CreateCustomer(CreateCustomerDto dto)
    {
        try
        {
            var context = new BankSimDbContext();
            var maxBalance = _configuration.GetValue<decimal>("MaximumAccountBalance");
            //validate account number availability
            var customer = new CustomerTb
            {
              Address = dto.Address,
              BVN = dto.BVN,
              CreationDate = DateTime.Now,
              DateOfBirth = (DateTime) dto.DateOfBirth,
              Email = dto.Email,
              FirstName = dto.FirstName,
              LastName = dto.LastName,
              KYCLevel = dto.KYCLevel,
              MiddleName = dto.MiddleName,
              PhoneNumber = dto.PhoneNumber,
              NIN = dto.NIN              
            };
            
            await context.CustomerTb.AddAsync(customer);
            await context.SaveChangesAsync();

            var account = new AccountTb
            {
                CustomerId = customer.Id,
                AccountNumber = dto.AccountNumber,
                Balance = dto.Balance,
                CreationDate = DateTime.Now,
                IsActive = true,
                Type = dto.AccountType,
                MaximumBalance = maxBalance
            };

            await context.AccountTb.AddAsync(account);

            await context.SaveChangesAsync();

            return new CreateCustomerResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Success"
                }
            };
        }
        catch (Exception ex)
        {
            return new CreateCustomerResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed"
                }
            };
        }
    }

    public async Task<CustomersDto> GetAllCustomersAsync(PaginationFilter paginationFilter = null, CustomerFilter filter = null)
    {
		try
		{
			var context = new BankSimDbContext();

            var customerDto = new List<CustomerDto>();
           
            customerDto =  await context.CustomerTb.OrderByDescending(x => x.CreationDate).Select(x => new CustomerDto
            {
                Id = x.Id,
                Address = x.Address,
                BVN = x.BVN,
                CreationDate = x.CreationDate,
                DateModified = x.DateModified,
                DateOfBirth = x.DateOfBirth,
                Email = x.Email,
                FirstName = x.FirstName,
                KYCLevel = x.KYCLevel,
                LastName = x.LastName,
                MiddleName = x.MiddleName,
                NIN = x.NIN,
                PhoneNumber = x.PhoneNumber
            }).OrderByDescending(x => x.CreationDate).ToListAsync();

            foreach (var customer in customerDto)
            {
                customer.Accounts = await context.AccountTb.AsNoTracking().Where(x => x.CustomerId == customer.Id).Select(x => new AccountDto
                {
                    AccountNumber = x.AccountNumber,
                    Type = x.Type,
                    Balance = x.Balance
                }).ToListAsync();
            }

            if(paginationFilter == null)
            {
                return new CustomersDto
                {
                    Customers = customerDto
                };
            }

            var filteredCustomers = new List<CustomerDto>();

            if (filter != null)
            {
                if (filter.RegistrationDate != null)
                {
                    customerDto = customerDto.Where(x => x.CreationDate >= filter.RegistrationDate).ToList();
                }
                
                if (filter.CustomerName != null)
                {
                    customerDto = customerDto.Where(x => x.FirstName.ToLower().Contains(filter.CustomerName) || x.LastName.ToLower().Contains(filter.CustomerName) 
                    || x.MiddleName.Contains(filter.CustomerName)).ToList();
                }
                
                
                if (!string.IsNullOrEmpty(filter.AccountNumber))
                {                    
                    foreach (var customer in customerDto)
                    {
                        var newCustomer = customer.Accounts.Where(x => x.AccountNumber.ToLower() == filter.AccountNumber.ToLower()).ToList();

                        if(newCustomer != null && newCustomer.Count > 0)
                        {
                            filteredCustomers.Add(customer);
                        }
                    }
                    customerDto = filteredCustomers;
                }
                
                if (!string.IsNullOrEmpty(filter.AccountType))
                {                    
                    foreach (var customer in customerDto)
                    {
                        var newCustomer = customer.Accounts.Where(x => x.Type.ToLower() == filter.AccountType.ToLower()).ToList();

                        if(newCustomer != null && newCustomer.Count > 0)
                        {
                            filteredCustomers.Add(customer);
                        }
                    }
                    customerDto = filteredCustomers;
                }
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            var totalItems = context.CustomerTb.Count();

            int totalPages = totalItems / paginationFilter.PageSize;

            if (totalItems % paginationFilter.PageSize != 0)
            {
                totalPages++;
            }

            customerDto = customerDto.Skip(skip).Take(paginationFilter.PageSize).ToList();
            
            return new CustomersDto
            {
                TotalPages = totalPages,
                Customers = customerDto
            };
        }
		catch (Exception ex)
		{
            return new CustomersDto();
		}
    }

    public async Task<CustomerDto> GetCustomerDetailsAsync(string id)
    {
		try
		{
			var context = new BankSimDbContext();

			var customer = await context.CustomerTb.SingleOrDefaultAsync(x => x.Id == Convert.ToInt32(id));

			if(customer == null)
			{
                return null;
            }

			var customerAccounts = await context.AccountTb.Where(x => x.CustomerId == customer.Id).ToListAsync();
			var accountsDto = customerAccounts.Select(x => new AccountDto
			{
				AccountNumber = x.AccountNumber,
				Balance = x.Balance,
				CreationDate = x.CreationDate,
				CustomerId = x.CustomerId,
				Id = x.Id,
				IsActive = x.IsActive,
				Type = x.Type
			}).ToList();

            var customerDto = new CustomerDto
            {
                Address = customer.Address,
                BVN = customer.BVN,
                Id = Convert.ToInt32(customer.Id),
                CreationDate = customer.CreationDate,
                DateModified = customer.DateModified,
                DateOfBirth = customer.DateOfBirth,
                Email = customer.Email,
                FirstName = customer.FirstName,
                MiddleName = customer.MiddleName,
                LastName = customer.LastName,
                KYCLevel = customer.KYCLevel,
                NIN = customer.NIN,
                PhoneNumber = customer.PhoneNumber
            };

            foreach (var account in accountsDto)
            {
				customerDto.Accounts.Add(account);
            }

            return customerDto;
        }
		catch (Exception ex)
		{

            return null;
		}
    }
}
