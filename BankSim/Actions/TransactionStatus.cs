using BankSim.Models.Contracts;
using BankSim.Models.Database;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BankSim.Actions
{
    public class TransactionStatus
    {
        private readonly ICipClient cipClient;

        public TransactionStatus(ICipClient cipClient)
        {
            this.cipClient = cipClient;
        }

        public async Task UpdateTransactionStatus(TransactionQueryRequest request)
        {
            try
            {
                var reqJson = JsonConvert.SerializeObject(request);
                Utilities.WriteLog("TransactionStatus:UpdateTransactionStatus", $"REQUEST PLAIN: {reqJson}");

                var status = "09";
                var numberOfRequests = 1;

                while (status == "09" && numberOfRequests <= 6)
                {
                    var response = await cipClient.TransactionQuery(request);

                    Utilities.WriteLog("TransactionStatus:UpdateTransactionStatus", $"SENT REQUEST NO.{numberOfRequests}");
                    
                    if (response.ResponseHeader.ResponseCode == "00" && response.TransactionQueryResponse != null)
                    {                    
                        var context = new BankSimDbContext();
                        var transaction = await context.TransactionTb.SingleOrDefaultAsync(x => x.SessionId == request.SessionId);
                        
                        if(response.ResponseHeader.ResponseCode == "25" && transaction != null)
                        {
                            transaction.ResponseCode = response.TransactionQueryResponse.ResponseCode;
                            transaction.ResponseMessage = response.TransactionQueryResponse.ResponseMessage;
                            await context.SaveChangesAsync();

                            await Utilities.PerformReversal(transaction);
                            status = transaction.ResponseCode;
                            numberOfRequests++;
                            await Task.Delay(TimeSpan.FromSeconds(30));
                            continue;
                        }
                        
                        if(response.ResponseHeader.ResponseCode == "00" && transaction != null)
                        {
                            transaction.ResponseCode = response.TransactionQueryResponse.ResponseCode;
                            transaction.ResponseMessage = response.TransactionQueryResponse.ResponseMessage;
                            await context.SaveChangesAsync();

                          
                            status = transaction.ResponseCode;
                            numberOfRequests++;
                            await Task.Delay(TimeSpan.FromSeconds(30));
                            continue;
                        }


                        if(response.TransactionQueryResponse.SessionId == request.SessionId && transaction != null)
                        {
                            transaction.ResponseCode = response.TransactionQueryResponse.ResponseCode;
                            transaction.ResponseMessage = response.TransactionQueryResponse.ResponseMessage;
                            await context.SaveChangesAsync();
                                
                            if(transaction.ResponseCode != "09")
                            {
                                await Utilities.PerformReversal(transaction);
                                status = transaction.ResponseCode;                                
                            }

                            numberOfRequests++;
                            await Task.Delay(TimeSpan.FromSeconds(30));
                            continue;
                        }
                        numberOfRequests++;
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                    else
                    {
                        numberOfRequests++;
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.WriteLog("TransactionStatus.UpdateTransactionStatus", $"Err: {ex.Message}");
            }
        }
    }
}
