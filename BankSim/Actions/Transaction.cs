using BankSim.Models.Database;
using BankSim.Models.Database.Tables;
using BankSim.Models.Internals;
using Microsoft.EntityFrameworkCore;

namespace BankSim.Actions;

public interface ITransaction
{
    Task<TransactionsDto> GetAllPagedTransactions(PaginationFilter paginationFilter = null, TransactionFilter filter = null);
    Task<TransactionDto> GetTransaction(string id);
    
}

public class Transaction : ITransaction
{
    public async Task<TransactionsDto> GetAllPagedTransactions(PaginationFilter paginationFilter = null, TransactionFilter filter = null)
    {
		try
		{
            var context = new BankSimDbContext();            
            var transactionDto = new List<TransactionDto>();            
            var finalTransactionDto = new List<TransactionDto>();


            var allTransactions = await context.TransactionTb.AsNoTracking().OrderByDescending(x => x.EntryDate).ToListAsync();
         
            if(paginationFilter == null)
            {
               
                transactionDto = allTransactions.Select(x => new TransactionDto
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
                    MainTransactionId = x.MainTransactionId,                    
                }).ToList();
            
                return new TransactionsDto
                {
                    Transactions = transactionDto
                };
            }

            

            if (filter != null)
            {
                if(filter.StartDate != null)
                {
                    allTransactions = allTransactions.Where(x => x.EntryDate >= filter.StartDate).ToList();
                }
                
                if(filter.EndDate != null)
                {
                    allTransactions = allTransactions.Where(x => x.EntryDate <= filter.EndDate).ToList();
                }
                
                if(!string.IsNullOrEmpty(filter.Status))
                {
                    allTransactions = allTransactions.Where(x => x.ResponseCode  == filter.Status).ToList();
                }
                
                
                if(!string.IsNullOrEmpty(filter.SessionId))
                {
                    allTransactions = allTransactions.Where(x => x.SessionId.Contains(filter.SessionId)).ToList();
                } 

                if(!string.IsNullOrEmpty(filter.Institution))
                {
                    allTransactions = allTransactions.Where(x => x.DestinationBankCode.Contains(filter.Institution) || 
                    (x.SourceBankCode != null && x.SourceBankCode.Contains(filter.Institution)) ).ToList();
                }

            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;
            
            var totalItems = allTransactions.Count();

            int totalPages = totalItems / paginationFilter.PageSize;

            if (totalItems % paginationFilter.PageSize != 0)
            {
                totalPages++;
            }


            transactionDto = allTransactions.Skip(skip).Take(paginationFilter.PageSize)
                .Select(x => new TransactionDto
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


    public async Task<TransactionDto> GetTransaction(string id)
    {
        try
        {
            var context = new BankSimDbContext();            

            var transaction = await context.TransactionTb.SingleOrDefaultAsync(x => x.Id == Convert.ToInt32(id));            

            if (transaction == null)
            {
                return new TransactionDto();
            }

                     
            var transactionDto = new TransactionDto
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
                MainTransactionId = transaction.MainTransactionId,
                Group = transaction.Group,
                Sector = transaction.Sector              
            };
        
            return transactionDto;
        }
        catch (Exception ex)
        {

            return new TransactionDto();
        }
    }
}
