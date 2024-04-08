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
    public class NameEnquiryController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public NameEnquiryController(IConfiguration configuration)
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
                var response = new NameEnquiryResponse();

                if (string.IsNullOrEmpty(requestDecrypted))
                {
                    
                    response = new NameEnquiryResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Decryption Error"
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);


                    Utilities.WriteLog("NameEnquiryController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("NameEnquiryController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                Utilities.WriteLog("NameEnquiryController", $"REQUEST ENCRYPTED: {value}");
                Utilities.WriteLog("NameEnquiryController", $"REQUEST PLAIN: {requestDecrypted}");

                var reqObj = JsonConvert.DeserializeObject<NameEnquiryRequest>(requestDecrypted);

                if(reqObj.DestinationInstitutionId != bankCode)
                {
                    response = new NameEnquiryResponse
                    {
                        ResponseCode = "01",
                        ResponseMessage = "INVALID DESTINATION INSTITUTION ID",
                        SessionId = reqObj.SessionId,
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        AccountId = reqObj.AccountId
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("NameEnquiryController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("NameEnquiryController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                var context = new BankSimDbContext();

                var accountInDatabase = await context.AccountTb.SingleOrDefaultAsync(x => x.AccountNumber == reqObj.AccountId);
                if(accountInDatabase == null) 
                {
                    response = new NameEnquiryResponse
                    {
                        ResponseCode = "02",
                        ResponseMessage = "INVALID ACCOUNT",
                        SessionId = reqObj.SessionId,
                        DestinationInstitutionId = reqObj.DestinationInstitutionId,
                        AccountId = reqObj.AccountId
                    };
                    respJson = JsonConvert.SerializeObject(response);
                    respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                    Utilities.WriteLog("NameEnquiryController", $"RESPONSE PLAIN: {respJson}");
                    Utilities.WriteLog("NameEnquiryController", $"RESPONSE ENC {respEnc}");
                    return Ok(respEnc);
                }

                var customer = await context.CustomerTb.SingleOrDefaultAsync(x => x.Id == accountInDatabase.CustomerId);

                response = new NameEnquiryResponse
                {
                    AccountId = accountInDatabase.AccountNumber,
                    AccountName = $"{customer.LastName} {customer.FirstName} {customer.MiddleName}",
                    AccountType = accountInDatabase.Type,
                    Bvn = customer.BVN,
                    DestinationInstitutionId = bankCode,
                    SessionId = reqObj.SessionId,
                    Status = accountInDatabase.IsActive ? "Active" : "InActive",
                    KycLevel = customer.KYCLevel,
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                respJson = JsonConvert.SerializeObject(response);
                respEnc = Utilities.Encrypt(respJson, cipPublicKey);

                Utilities.WriteLog("NameEnquiryController", $"RESPONSE PLAIN: {respJson}");
                Utilities.WriteLog("NameEnquiryController", $"RESPONSE ENC {respEnc}");                
                return Ok(respEnc);
            }
            catch (Exception ex)
            {
                Utilities.WriteLog("NameEnquiryController", $"ERR: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
