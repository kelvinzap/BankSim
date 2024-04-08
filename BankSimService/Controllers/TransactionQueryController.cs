using BankSimService.Actions;
using BankSimService.Models.Contracts;
using BankSimService.Models.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BankSimService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionQueryController : ControllerBase
    {
        private IConfiguration configuration;

        public TransactionQueryController(IConfiguration configuration)
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
                var response = new TransactionQueryResponse();

                if (string.IsNullOrEmpty(requestDecrypted))
                {

                    response = new TransactionQueryResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Decryption Error"
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);


                    Utilities.WriteLog("TransactionQueryController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("TransactionQueryController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                Utilities.WriteLog("TransactionQueryController", $"REQUEST ENCRYPTED: {value}");
                Utilities.WriteLog("TransactionQueryController", $"REQUEST PLAIN: {requestDecrypted}");

                var reqObj = JsonConvert.DeserializeObject<TransactionQueryRequest>(requestDecrypted);
                

                var context = new BankSimDbContext();
              

                var transaction = await context.TransactionTb.SingleOrDefaultAsync(x => x.SessionId == reqObj.SessionId);

                if (transaction == null)
                {
                    response = new TransactionQueryResponse
                    {                        
                        ResponseCode = "25",
                        ResponseMessage = "TRANSACTION NOT FOUND"
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("TransactionQueryController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("TransactionQueryController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                
                response = new TransactionQueryResponse
                {
                    Amount = transaction.Amount,  
                    Channel = transaction.Channel,
                    CreditAccount = transaction.DestinationAccount,
                    CreditAccountName = transaction.CreditAccountName,
                    DestinationInstitutionId = bankCode,
                    Group = transaction.Group,
                    Narration = transaction.Narration,
                    PaymentRef = transaction.TransactionId,
                    Sector = transaction.Sector,
                    SessionId = transaction.SessionId,
                    SourceAccountId = transaction.SourceAccount,
                    SourceAccountName = transaction.DebitAccountName,
                    TransactionDate = transaction.EntryDate,
                    ResponseCode = transaction.ResponseCode,
                    ResponseMessage = transaction.ResponseMessage
                };
                respJson = JsonConvert.SerializeObject(response);
                respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                Utilities.WriteLog("TransactionQueryController", $"RESPONSE PLAIN: {respJson}");
                Utilities.WriteLog("TransactionQueryController", $"RESPONSE ENC {respEnc}");
                return Ok(respEnc);
            }
            catch (Exception ex)
            {
                Utilities.WriteLog("TransactionQueryController", $"ERR: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
