using BankSimService.Actions;
using BankSimService.Models.Contracts;
using BankSimService.Models.Database;
using BankSimService.Models.Database.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BankSimService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostCreditController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public PostCreditController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            try
            {
                var respJson = "";
                var respEnc = "";
                var bankCode = configuration.GetValue<string>("BankCode");
                var requestDecrypted = Utilities.Decrypt(value);
                var cipPublicKeyPath = configuration.GetValue<string>("CIP:CipPublicKeyPath");
                var cipPublicKey = System.IO.File.ReadAllText(cipPublicKeyPath);
                var response = new PostCreditResponse();

                if (string.IsNullOrEmpty(requestDecrypted))
                {

                    response = new PostCreditResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Decryption Error"
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);


                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                Utilities.WriteLog("PostCreditController", $"REQUEST ENCRYPTED: {value}");
                Utilities.WriteLog("PostCreditController", $"REQUEST PLAIN: {requestDecrypted}");

                var reqObj = JsonConvert.DeserializeObject<PostCreditRequest>(requestDecrypted);

                if (reqObj.DestinationInstitutionId != bankCode)
                {
                    response = new PostCreditResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "INVALID DESTINATION INSTITUTION ID",
                        
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        Amount = reqObj.Amount,
                        Channel = reqObj.Channel,
                        CreditAccount = reqObj.CreditAccount,
                        CreditAccountName = reqObj.CreditAccountName,
                        Group = reqObj.Group,
                        Narration = reqObj.Narration,
                        Sector = reqObj.Sector,
                        SourceAccountId = reqObj.SourceAccountId,
                        SourceAccountName = reqObj.SourceAccountName,
                        SessionId = reqObj.SessionId,
                        PaymentRef = reqObj.PaymentRef
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                var context = new BankSimDbContext();

                var accountInDatabase = await context.AccountTb.SingleOrDefaultAsync(x => x.AccountNumber == reqObj.CreditAccount);
                if (accountInDatabase == null)
                {
                    response = new PostCreditResponse
                    {
                        ResponseCode = "02",
                        ResponseMessage = "INVALID ACCOUNT",
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        Amount = reqObj.Amount,
                        Channel = reqObj.Channel,
                        CreditAccount = reqObj.CreditAccount,
                        CreditAccountName = reqObj.CreditAccountName,
                        Group = reqObj.Group,
                        Narration = reqObj.Narration,
                        Sector = reqObj.Sector,
                        SourceAccountId = reqObj.SourceAccountId,
                        SourceAccountName = reqObj.SourceAccountName,
                        SessionId = reqObj.SessionId,
                        PaymentRef = reqObj.PaymentRef
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                var customer = await context.CustomerTb.SingleOrDefaultAsync(x => x.Id == accountInDatabase.CustomerId);
                var transactionInDb = await context.TransactionTb.SingleOrDefaultAsync(x => x.SessionId == reqObj.SessionId);
                var minimumAmount = configuration.GetValue<decimal>("MinimumAmount");

                if(transactionInDb != null)
                {
                    response = new PostCreditResponse
                    {
                        ResponseCode = "94",
                        ResponseMessage = "Duplicate Transaction",
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        Amount = reqObj.Amount,
                        Channel = reqObj.Channel,
                        CreditAccount = reqObj.CreditAccount,
                        CreditAccountName = reqObj.CreditAccountName,
                        Group = reqObj.Group,
                        Narration = reqObj.Narration,
                        Sector = reqObj.Sector,
                        SourceAccountId = reqObj.SourceAccountId,
                        SourceAccountName = reqObj.SourceAccountName,
                        SessionId = reqObj.SessionId,
                        PaymentRef = reqObj.PaymentRef
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }
                
                if(reqObj.Amount < minimumAmount)
                {
                    response = new PostCreditResponse
                    {
                        ResponseCode = "13",
                        ResponseMessage = "INVALID AMOUNT",
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        Amount = reqObj.Amount,
                        Channel = reqObj.Channel,
                        CreditAccount = reqObj.CreditAccount,
                        CreditAccountName = reqObj.CreditAccountName,
                        Group = reqObj.Group,
                        Narration = reqObj.Narration,
                        Sector = reqObj.Sector,
                        SourceAccountId = reqObj.SourceAccountId,
                        SourceAccountName = reqObj.SourceAccountName,
                        SessionId = reqObj.SessionId,
                        PaymentRef = reqObj.PaymentRef
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                if (!accountInDatabase.IsActive)
                {
                    response = new PostCreditResponse
                    {
                        ResponseCode = "05",
                        ResponseMessage = "Bank account restricted/Dormant account",
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        Amount = reqObj.Amount,
                        Channel = reqObj.Channel,
                        CreditAccount = reqObj.CreditAccount,
                        CreditAccountName = reqObj.CreditAccountName,
                        Group = reqObj.Group,
                        Narration = reqObj.Narration,
                        Sector = reqObj.Sector,
                        SourceAccountId = reqObj.SourceAccountId,
                        SourceAccountName = reqObj.SourceAccountName,
                        SessionId = reqObj.SessionId,
                        PaymentRef = reqObj.PaymentRef
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                var customerName = $"{customer.LastName} {customer.FirstName} {customer.MiddleName}";
                
                if (reqObj.CreditAccountName != customerName)
                {
                    response = new PostCreditResponse
                    {
                        ResponseCode = "14",
                        ResponseMessage = "Account name mismatch",
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        Amount = reqObj.Amount,
                        Channel = reqObj.Channel,
                        CreditAccount = reqObj.CreditAccount,
                        CreditAccountName = reqObj.CreditAccountName,
                        Group = reqObj.Group,
                        Narration = reqObj.Narration,
                        Sector = reqObj.Sector,
                        SourceAccountId = reqObj.SourceAccountId,
                        SourceAccountName = reqObj.SourceAccountName,
                        SessionId = reqObj.SessionId,
                        PaymentRef = reqObj.PaymentRef
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                var expectedBalance = accountInDatabase.Balance + reqObj.Amount;

                if(expectedBalance > accountInDatabase.MaximumBalance)
                {
                    response = new PostCreditResponse
                    {
                        ResponseCode = "61",
                        ResponseMessage = "Credit Limit exceeded",
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        Amount = reqObj.Amount,
                        Channel = reqObj.Channel,
                        CreditAccount = reqObj.CreditAccount,
                        CreditAccountName = reqObj.CreditAccountName,
                        Group = reqObj.Group,
                        Narration = reqObj.Narration,
                        Sector = reqObj.Sector,
                        SourceAccountId = reqObj.SourceAccountId,
                        SourceAccountName = reqObj.SourceAccountName,
                        SessionId = reqObj.SessionId,
                        PaymentRef = reqObj.PaymentRef
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }
                //PROCESSING CREDIT
                var transactionId = CoralPay.Miscellaneous.UniqueNumber();
                var transaction = new TransactionTb
                {
                    Amount = reqObj.Amount,
                    Channel = reqObj.Channel,
                    CreditAccountName = reqObj.CreditAccountName,
                    DebitAccountName = reqObj.SourceAccountName, 
                    DestinationAccount = reqObj.CreditAccount,
                    SourceAccount = reqObj.SourceAccountId,
                    DestinationBankCode = reqObj.DestinationInstitutionId,
                    SourceBankCode = reqObj.SessionId[..6],
                    Narration = reqObj.Narration,                    
                    SessionId = reqObj.SessionId.Trim(),                    
                    TransactionId = reqObj.PaymentRef,
                    TransactionType = "CREDIT",
                    ResponseCode = "09",
                    ResponseMessage = "Pending",
                    EntryDate = DateTime.Now,
                    Sector = reqObj.Sector,
                    Group = reqObj.Group
                };


                await context.TransactionTb.AddAsync(transaction);
                await context.SaveChangesAsync();

                accountInDatabase.Balance += reqObj.Amount;
                accountInDatabase.DateModified = DateTime.Now;

                var updated = 0;
                updated = await context.SaveChangesAsync();

                if(updated < 1)
                {
                    transaction.ResponseCode = "01";
                    transaction.ResponseMessage = "Failed";                    
                    
                     updated = await context.SaveChangesAsync();

                    if(updated < 1)
                    {
                        response = new PostCreditResponse
                        {
                            DestinationInstitutionId = reqObj.DestinationInstitutionId,
                            Amount = reqObj.Amount,
                            Channel = reqObj.Channel,
                            CreditAccount = reqObj.CreditAccount,
                            CreditAccountName = reqObj.CreditAccountName,                            
                            Narration = reqObj.Narration,                            
                            SourceAccountId = reqObj.SourceAccountId,
                            SourceAccountName = reqObj.SourceAccountName,
                            ResponseCode = "01",
                            ResponseMessage = "Internal Server Error",
                            SessionId = reqObj.SessionId,
                            PaymentRef = reqObj.PaymentRef                            
                        };
                        respJson = JsonConvert.SerializeObject(response);
                        respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                        Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                        Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                        return Ok(respEnc);
                    }

                    response = new PostCreditResponse
                    {
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        Amount = reqObj.Amount,
                        Channel = reqObj.Channel,
                        CreditAccount = reqObj.CreditAccount,
                        CreditAccountName = reqObj.CreditAccountName,
                        Group = transaction.Group,
                        Narration = reqObj.Narration,
                        Sector = transaction.Sector,
                        SourceAccountId = reqObj.SourceAccountId,
                        SourceAccountName = reqObj.SourceAccountName,
                        ResponseCode = "01",
                        ResponseMessage = "Internal Server Error",
                        SessionId = reqObj.SessionId,
                        PaymentRef = reqObj.PaymentRef,
                        TransactionDate = transaction.EntryDate
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                transaction.ResponseCode = "00";
                transaction.ResponseMessage = "Success";
                transaction.EntryDate = DateTime.Now;

                //await context.TransactionTb.AddAsync(transaction);
                updated = await context.SaveChangesAsync();

                if (updated < 1)
                {
                    response = new PostCreditResponse
                    {
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        Amount = reqObj.Amount,
                        Channel = reqObj.Channel,
                        CreditAccount = reqObj.CreditAccount,
                        CreditAccountName = reqObj.CreditAccountName,                      
                        Narration = reqObj.Narration,                        
                        SourceAccountId = reqObj.SourceAccountId,
                        SourceAccountName = reqObj.SourceAccountName,
                        ResponseCode = "01",
                        ResponseMessage = "Internal Server Error",
                        SessionId = reqObj.SessionId,
                        PaymentRef = reqObj.PaymentRef
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                response = new PostCreditResponse
                {
                    DestinationInstitutionId = reqObj.DestinationInstitutionId,
                    Amount = reqObj.Amount,
                    Channel = reqObj.Channel,
                    CreditAccount = reqObj.CreditAccount,
                    CreditAccountName = reqObj.CreditAccountName,
                    Group = transaction.Group,
                    Narration = reqObj.Narration,
                    Sector = transaction.Sector,
                    SourceAccountId = reqObj.SourceAccountId,
                    SourceAccountName = reqObj.SourceAccountName,
                    ResponseCode = "00",
                    ResponseMessage = "Successful",
                    SessionId = reqObj.SessionId,
                    PaymentRef = reqObj.PaymentRef,
                    TransactionDate = transaction.EntryDate
                };
                respJson = JsonConvert.SerializeObject(response);
                respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                Utilities.WriteLog("PostCreditController", $"RESPONSE PLAIN: {respJson}");
                Utilities.WriteLog("PostCreditController", $"RESPONSE ENC {respEnc}");
                return Ok(respEnc);
            }
            catch (Exception ex)
            {
                Utilities.WriteLog("PostCreditController", $"ERR: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
