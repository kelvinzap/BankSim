using BankSimService.Actions;
using BankSimService.Models;
using BankSimService.Models.Contracts;
using BankSimService.Models.Database;
using BankSimService.Models.Internals;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BankSimService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]
    public class TransactionStatusController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public TransactionStatusController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody])
        {
            try {
                var respJson = "";
                var response = new CreateAccountResponse();
                var context = new BankSimDbContext();
                var maxBalance = configuration.GetValue<decimal>("MaximumAccountBalance");
                var bankCode = configuration.GetValue<string>("BankCode");
                var bankName = configuration.GetValue<string>("BankName");
                var sessionId = CoralPay.Miscellaneous.UniqueNumber();
                var accountNumber = "";

                var transactionInDatabase = await context.TransactionTb.SingleOrDefaultAsync(x => x.TransactionId == request.TransactionId);

                if (accountTraceIdExists != null)
                {
                    response = new TransactionStatusResponse
                    {
                        ResponseHeader = new ResponseHeader
                        {
                            ResponseCode = "01",
                            ResponseMessage = "Transaction Id mismatch"
                        },
                        TransactionId = request.TransactionId
                    };

                    respJson = JsonConvert.SerializeObject(response);
                    Utilities.WriteLog("CreateDynamicAccountController", $"RESPONSE: {respJson}");
                    return Ok(response);
                }

                var accountNumberExists = true;

                while (accountNumberExists)
                {
                    var newAccountNumber = Utilities.RandomDigits(10);
                    var accountExists = context.AccountTb.SingleOrDefault(x => x.AccountNumber == newAccountNumber);
                    if (accountExists == null)
                    {
                        accountNumber = newAccountNumber;
                        accountNumberExists = false;
                    }
                }
                var reqJson = JsonConvert.SerializeObject(request);
                Utilities.WriteLog("CreateDynamicAccountController", $"REQUEST: {reqJson}");


                var account = new AccountTb
                {
                    AccountNumber = accountNumber,
                    Balance = 0m,
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    Type = AccountTypes.Virtual.ToString(),
                    MaximumBalance = maxBalance,
                    AccountName = request.AccountName,
                    SessionId = sessionId,
                    TraceId = request.TransactionId
                };

                await context.AccountTb.AddAsync(account);
                await context.SaveChangesAsync();

                response = new CreateAccountResponse
                {
                    Description = request.Description,
                    DestinationAccountName = account.AccountName,
                    DestinationAccountNumber = account.AccountNumber,
                    DestinationBankCode = bankCode,
                    DestinationBankName = bankName,
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Account created successfully"
                    },
                    SessionId = sessionId,
                    TransactionId = request.TransactionId
                };

                respJson = JsonConvert.SerializeObject(response);
                Utilities.WriteLog("CreateDynamicAccountController", $"RESPONSE: {respJson}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Utilities.WriteLog("CreateDynamicAccountController", $"ERR: {ex.Message}");

                return Ok(new CreateAccountResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    },
                    TransactionId = request.TransactionId
                });
            }
        }
    }
}
