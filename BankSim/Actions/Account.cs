using BankSim.Models;
using BankSim.Models.Contracts;
using BankSim.Models.Database;
using BankSim.Models.Database.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Immutable;

namespace BankSim.Actions;

public interface IAccount
{
    Task<TransferResult> DebitAccount(TransferDto dto);
    Task<AccountDto> GetAccountWithAccountNumber(string accountNumber);
    Task<IEnumerable<AccountDto>> GetCustomerAccounts(string id);
    Task<string> GetCreditAccountName(string accountId, string destinationBankCode);
    Task<TransactionsDto> GetAccountTransactions(string id, PaginationFilter paginationFilter = null, TransactionFilter filter = null);
    Task<AccountDto> GetAccountDetails(string id);
    Task<ResponseHeader> CreateAccount(AccountDto dto);
    Task<ResponseHeader> FundAccount(FundAccountDto dto);
    Task<AccountAvailableResult> CheckAccountNumberAvailability(string accountNumber);
    
}
public class Account : IAccount
{
    private readonly IConfiguration configuration;
    private readonly ICipClient cipClient;
    private readonly TransactionStatus transactionStatus;

    public Account(IConfiguration configuration, ICipClient cipClient, TransactionStatus transactionStatus)
    {
        this.configuration = configuration;
        this.cipClient = cipClient;
        this.transactionStatus = transactionStatus;
    }


    public Account()
    {

    }
    public async Task<AccountAvailableResult> CheckAccountNumberAvailability(string accountNumber)
    {
        try
        {
            var context = new BankSimDbContext();

            var accountExists = await context.AccountTb.SingleOrDefaultAsync(x => x.AccountNumber == accountNumber);

            var response = new AccountAvailableResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00"
                },
                IsAvailable = accountExists == null
            };

            return response;
        }
        catch (Exception ex)
        {
            return new AccountAvailableResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "01"
                }
            };
        }
    }

    public async Task<ResponseHeader> CreateAccount(AccountDto dto)
    {
        try
        {

            var context = new BankSimDbContext();
            var maxBalance = configuration.GetValue<decimal>("MaximumAccountBalance");

            //validate account number availability
            var account = new AccountTb
            {
                CustomerId = dto.CustomerId,
                AccountNumber = dto.AccountNumber,
                Balance = dto.Balance,
                CreationDate = DateTime.Now,
                IsActive = true,
                Type = dto.Type,
                MaximumBalance = maxBalance
            };

            await context.AccountTb.AddAsync(account);
            await context.SaveChangesAsync();

            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = "Success"
            };
        }
        catch (Exception ex)
        {
            return new ResponseHeader
            {
                ResponseCode = "01",
                ResponseMessage = "Success"
            };
        }
    }

    public async Task<TransferResult> DebitAccount(TransferDto dto)
    {
        try
        {            
            var bankCode = configuration.GetValue<string>("BankCode");
            var context = new BankSimDbContext();

            var debitAccount = await context.AccountTb.SingleOrDefaultAsync(x => x.AccountNumber == dto.DebitAccount);

            if (debitAccount == null)
            {
                return new TransferResult
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed"
                    }
                };
            }

            var customer = await context.CustomerTb.SingleOrDefaultAsync(x => x.Id == debitAccount.CustomerId);

            var sessionId = bankCode + Utilities.GenerateSessionId();

            var transactionExists = await context.TransactionTb.SingleOrDefaultAsync(x => x.SessionId == sessionId);

            if (transactionExists != null)
            {
                return new TransferResult
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed"
                    }
                };
            }

            if (debitAccount.Balance < Convert.ToDecimal(dto.Amount))
            {
                return new TransferResult
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed"
                    }
                };
            }

            debitAccount.Balance -= Convert.ToDecimal(dto.Amount);
            debitAccount.DateModified = DateTime.Now;

            var transaction = new TransactionTb
            {
                Amount = Convert.ToDecimal(dto.Amount),
                Channel = "WEB",
                ChargeAmount = 0,
                CreditAccountName = dto.CreditAccountName,
                DebitAccountName = $"{customer.LastName} {customer.FirstName} {customer.MiddleName}",
                DestinationAccount = dto.CreditAccount,
                DestinationBankCode = dto.DestinationBankCode,
                EntryDate = DateTime.Now,
                Narration = dto.Description,
                ResponseCode = "09",
                ResponseMessage = "Pending",
                SessionId = sessionId,
                Group = "Personal",
                Sector = "NGN",
                TransactionId = CoralPay.Miscellaneous.UniqueNumber(),
                TransactionType = "DEBIT",
                SourceAccount = dto.DebitAccount,
                SourceBankCode = bankCode
            };

            await context.TransactionTb.AddAsync(transaction);
            await context.SaveChangesAsync();

            var postCreditRequest = new PostCreditRequest
            {
                Amount = transaction.Amount,
                Channel = transaction.Channel,
                CreditAccount = transaction.DestinationAccount,
                CreditAccountName = transaction.CreditAccountName,
                DestinationInstitutionId = transaction.DestinationBankCode,
                Group = transaction.Group,
                Narration = transaction.Narration,
                PaymentRef = transaction.TransactionId,
                Sector = transaction.Sector,
                SessionId = transaction.SessionId,
                SourceAccountId = transaction.SourceAccount,
                SourceAccountName = transaction.DebitAccountName
            };

            var result = await cipClient.PostCredit(postCreditRequest);

            var tranQueryRequest = new TransactionQueryRequest
            {
                SessionId = transaction.SessionId
            };

            if (result.ResponseHeader == null || result.ResponseHeader.ResponseCode == "01" || result.PostCreditResponse == null                
                || result.PostCreditResponse.ResponseCode == "09")
            {                
                Task.Run(() => transactionStatus.UpdateTransactionStatus(tranQueryRequest));

                return new TransferResult
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "09",
                        ResponseMessage = "Pending"
                    }
                };
            }

          
            if (result.PostCreditResponse.ResponseCode == "00")
            {
                transaction.ResponseCode = "00";
                transaction.ResponseMessage = result.PostCreditResponse.ResponseMessage;
                await context.SaveChangesAsync();
                return new TransferResult
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    TransactionDto = new TransactionDto
                    {
                        Id = Convert.ToInt32(transaction.Id),
                        SourceAccount = transaction.SourceAccount,
                        DestinationAccount = transaction.DestinationAccount,
                        Amount = transaction.Amount,
                        Channel = transaction.Channel,
                        ChargeAmount = transaction.ChargeAmount,
                        CreditAccountName = transaction.CreditAccountName,
                        DebitAccountName = transaction.DebitAccountName,
                        DestinationBankCode = transaction.DestinationBankCode,
                        EntryDate = transaction.EntryDate,
                        Narration = transaction.Narration,
                        ResponseCode = transaction.ResponseCode,
                        ResponseMessage = transaction.ResponseMessage,
                        SessionId = transaction.SessionId,
                        SourceBankCode = transaction.SourceBankCode,
                        TransactionId = transaction.TransactionId,
                        TransactionType = transaction.TransactionType,
                        MainTransactionId = transaction.MainTransactionId
                    }
                };
            }

            transaction.ResponseCode = result.PostCreditResponse.ResponseCode;
            transaction.ResponseMessage = result.PostCreditResponse.ResponseMessage;

            await context.SaveChangesAsync();

            await Utilities.PerformReversal(transaction); //reversal

            return new TransferResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed"
                }
            };

        
        }
        catch (Exception ex)
        {
            return new TransferResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "06",
                    ResponseMessage = "Error"
                }
            };
        }
    }

    public async Task<ResponseHeader> FundAccount(FundAccountDto dto)
    {
        try
        {
            var context = new BankSimDbContext();

            var account = await context.AccountTb.SingleOrDefaultAsync(x => x.Id == Convert.ToInt32(dto.AccountId));
            
            if (account == null) 
            {
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed"
                };
            }

            account.Balance += dto.Amount;
            await context.SaveChangesAsync();

            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = "Successful"
            };
        }
        catch (Exception ex)
        {
            return new ResponseHeader
            {
                ResponseCode = "01",
                ResponseMessage = "Failed"
            };
        }
    }

    public async Task<AccountDto> GetAccountWithAccountNumber(string accountNumber)
    {
        try
        {
            var context = new BankSimDbContext();

            var account = await context.AccountTb.SingleOrDefaultAsync(x => x.AccountNumber == accountNumber);

            if (account == null)
            {
                return new AccountDto();
            }


            var accountDto = new AccountDto
            {
                AccountNumber = account.AccountNumber,
                Id = account.Id,
                CustomerId = account.CustomerId,
                Balance = account.Balance,
                CreationDate = account.CreationDate,
                IsActive = account.IsActive,
                Type = account.Type
            };

            return accountDto;
        }
        catch (Exception ex)
        {
            return new AccountDto();
        }
    }

    public async Task<AccountDto> GetAccountDetails(string id)
    {
        try
        {
            var context = new BankSimDbContext();

            var account = await context.AccountTb.SingleOrDefaultAsync(x => x.Id == Convert.ToInt32(id));

            if (account == null)
            {
                return null;
            }

            var customer = await context.CustomerTb.SingleOrDefaultAsync(x => x.Id == account.CustomerId);

            var accountDto = new AccountDto
            {
                AccountNumber = account.AccountNumber,
                Id = account.Id,
                CustomerId = account.CustomerId,
                Balance = account.Balance,
                CreationDate = account.CreationDate,
                IsActive = account.IsActive,
                Type = account.Type,
                AccountName = $"{customer.LastName} {customer.FirstName} {customer.MiddleName}"
            };


            return accountDto;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<TransactionsDto> GetAccountTransactions(string id, PaginationFilter paginationFilter = null, TransactionFilter filter = null)
    {
        try
        {
            var context = new BankSimDbContext();            
            var transactionDto = new List<TransactionDto>();            
            var finalTransactionDto = new List<TransactionDto>();

            var account = await context.AccountTb.SingleOrDefaultAsync(x => x.Id == Convert.ToInt32(id));

            if (account == null)
            {
                return new TransactionsDto();
            }
            
            var allTransactions = await context.TransactionTb.AsNoTracking().Where(x => x.DestinationAccount == account.AccountNumber || x.SourceAccount == account.AccountNumber)
               .OrderByDescending(x => x.EntryDate).ToListAsync();

          
            if (paginationFilter == null)
            { 
                transactionDto = await context.TransactionTb.Where(x => x.DestinationAccount == account.AccountNumber || x.SourceAccount == account.AccountNumber).
                    Select(x => new TransactionDto
                {
                    Id = Convert.ToInt32(x.Id),
                    SourceAccount = x.SourceAccount,
                    DestinationAccount = x.DestinationAccount,
                    Amount = x.Amount,
                    Channel = x.Channel,
                    ChargeAmount = x.ChargeAmount,
                    CreditAccountName = x.CreditAccountName,
                    DebitAccountName = x.DebitAccountName,
                    DestinationBankCode = x.DestinationBankCode,
                    EntryDate = x.EntryDate,
                    Narration = x.Narration,
                    ResponseCode = x.ResponseCode,
                    ResponseMessage = x.ResponseMessage,
                    SessionId = x.SessionId,
                    SourceBankCode = x.SourceBankCode,
                    TransactionId = x.TransactionId,
                    TransactionType = x.TransactionType,
                    MainTransactionId = x.MainTransactionId
                }).OrderByDescending(x => x.EntryDate).ToListAsync();

                return new TransactionsDto
                {
                    Transactions = transactionDto
                };
            }


            if (filter != null)
            {
                if (filter.StartDate != null)
                {
                    allTransactions = allTransactions.Where(x => x.EntryDate >= filter.StartDate).ToList();
                }

                if (filter.EndDate != null)
                {
                    allTransactions = allTransactions.Where(x => x.EntryDate <= filter.EndDate).ToList();
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    allTransactions = allTransactions.Where(x => x.ResponseCode == filter.Status).ToList();
                }


                if (!string.IsNullOrEmpty(filter.SessionId))
                {
                    allTransactions = allTransactions.Where(x => x.SessionId.Contains(filter.SessionId)).ToList();
                }

                if (!string.IsNullOrEmpty(filter.Institution))
                {
                    allTransactions = allTransactions.Where(x => x.DestinationBankCode.Contains(filter.Institution) ||
                    (x.SourceBankCode != null && x.SourceBankCode.Contains(filter.Institution))).ToList();
                }               
            }


            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            var totalItems = allTransactions.Count();

            int totalPages = totalItems / paginationFilter.PageSize;

            if (totalItems % paginationFilter.PageSize != 0)
            {
                totalPages++;
            }


            transactionDto = allTransactions
                .Skip(skip).Take(paginationFilter.PageSize).Select(x => new TransactionDto
                {
                    Id = Convert.ToInt32(x.Id),
                    SourceAccount = x.SourceAccount,
                    DestinationAccount = x.DestinationAccount,
                    Amount = x.Amount,
                    Channel = x.Channel,
                    ChargeAmount = x.ChargeAmount,
                    CreditAccountName = x.CreditAccountName,
                    DebitAccountName = x.DebitAccountName,
                    DestinationBankCode = x.DestinationBankCode,
                    EntryDate = x.EntryDate,
                    Narration = x.Narration,
                    ResponseCode = x.ResponseCode,
                    ResponseMessage = x.ResponseMessage,
                    SessionId = x.SessionId,
                    SourceBankCode = x.SourceBankCode,
                    TransactionId = x.TransactionId,
                    TransactionType = x.TransactionType,
                    MainTransactionId = x.MainTransactionId
                }).ToList();
         

            return new TransactionsDto
            {
                TotalPages = totalPages,
                Transactions = transactionDto
            };
        }
        catch (Exception ex)
        {
            return new TransactionsDto();
        }


    }

    public async Task<string> GetCreditAccountName(string accountId, string destinationBankCode)
    {
        try
        {
            var bankCode = configuration.GetValue<string>("BankCode");

            var request = new NameEnquiryRequest
            {
                AccountId = accountId,
                DestinationInstitutionId = destinationBankCode,
                SessionId = bankCode + Utilities.GenerateSessionId()
            };

            var result = await cipClient.NameEnquiry(request);

            if (result.ResponseHeader == null || result.ResponseHeader.ResponseCode == "01" || result.NameEnquiryResponse == null)
            {
                return string.Empty;
            }

            if (result.NameEnquiryResponse.ResponseCode == "00")
            {
                return result.NameEnquiryResponse.AccountName;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            return string.Empty;
        }   
    }

    public async Task<IEnumerable<AccountDto>> GetCustomerAccounts(string id)
    {
        try
        {
            var context = new BankSimDbContext();

            return await context.AccountTb.Where(x => x.CustomerId.ToString() == id && x.IsActive).Select(a => new AccountDto
            {
                AccountNumber = a.AccountNumber,
                Id = a.Id,
                CustomerId = a.CustomerId,
                Balance = a.Balance,
                CreationDate = a.CreationDate,
                IsActive = a.IsActive,
                Type = a.Type
            }).ToListAsync();

        }
        catch (Exception ex)
        {
            return Enumerable.Empty<AccountDto>();
        }
    }

}
