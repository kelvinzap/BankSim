using BankSim.Models;
using BankSim.Models.Contracts;
using BankSim.Models.Internals;
using Newtonsoft.Json;

namespace BankSim.Actions;

public interface ICipClient
{
    Task<PostCreditResult> PostCredit(PostCreditRequest request);
    Task<NameEnquiryResult> NameEnquiry(NameEnquiryRequest request);
    Task<TransactionQueryResult> TransactionQuery(TransactionQueryRequest request);
}

public class CipClient : ICipClient
{
    private readonly IConfiguration configuration;

    public CipClient(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<NameEnquiryResult> NameEnquiry(NameEnquiryRequest request)
    {
        try
        {
            var cipPublicKeyPath = configuration.GetValue<string>("CIP:CipPublicKeyPath");
            var cipPublicKey = System.IO.File.ReadAllText(cipPublicKeyPath);

            var client = new HttpClient();
            var url = configuration.GetValue<string>("CIP:CipNamenquiryEndPoint");

            var reqJson = JsonConvert.SerializeObject(request);
            var reqEnc = Utilities.Encrypt(reqJson, cipPublicKey);

            Utilities.WriteLog("CipClient:NameEnquiry", $"REQUEST DATA PLAIN: {reqJson}");
            Utilities.WriteLog("CipClient:NameEnquiry", $"REQUEST DATA ENCRYPTED: {reqEnc}");
            Utilities.WriteLog("CipClient:NameEnquiry", $"Sending name enquiry request to CIP. Url: {url}");

            var response = await client.PostAsJsonAsync(url, reqEnc);

            var responseContent = await response.Content.ReadAsStringAsync();


            Utilities.WriteLog("CipClient:NameEnquiry", $"RESPONSE STATUS CODE: {response.StatusCode}");
            Utilities.WriteLog("CipClient:NameEnquiry", $"RESPONSE FROM CIP ENCRYPTED: {responseContent}");

            var responseDecrypted = Utilities.Decrypt(responseContent);

            Utilities.WriteLog("CipClient:NameEnquiry", $"RESPONSE FROM CIP DECRYPTED: {responseDecrypted}");

            var cipResponse = JsonConvert.DeserializeObject<NameEnquiryResponse>(responseDecrypted);

            var result = new NameEnquiryResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Success"
                },
                NameEnquiryResponse = cipResponse
            };

            return result;

        }
        catch (Exception ex)
        {
            Utilities.WriteLog("CipClient:NameEnquiry", $"ERR: {ex.Message}");

            return new NameEnquiryResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed"
                }

            };
        }
    }

    public async Task<PostCreditResult> PostCredit(PostCreditRequest request)
    {
        try
        {
            var cipPublicKeyPath = configuration.GetValue<string>("CIP:CipPublicKeyPath");
            var cipPublicKey = System.IO.File.ReadAllText(cipPublicKeyPath);

            var client = new HttpClient();
            var url = configuration.GetValue<string>("CIP:CipPostCreditEndPoint");

            var reqJson = JsonConvert.SerializeObject(request);
            var reqEnc = Utilities.Encrypt(reqJson, cipPublicKey);
         
            Utilities.WriteLog("CipClient:PostCredit", $"REQUEST DATA PLAIN: {reqJson}");
            Utilities.WriteLog("CipClient:PostCredit", $"REQUEST DATA ENCRYPTED: {reqEnc}");
            Utilities.WriteLog("CipClient:PostCredit", $"Sending post credit request to CIP. Url: {url}");

            var response = await client.PostAsJsonAsync(url, reqEnc);

            var responseContent = await response.Content.ReadAsStringAsync();


            Utilities.WriteLog("CipClient:PostCredit", $"RESPONSE STATUS CODE: {response.StatusCode}");
            Utilities.WriteLog("CipClient:PostCredit", $"RESPONSE FROM CIP ENCRYPTED: {responseContent}");
            
            var responseDecrypted = Utilities.Decrypt(responseContent);

            Utilities.WriteLog("CipClient:PostCredit", $"RESPONSE FROM CIP DECRYPTED: {responseDecrypted}");

            var cipResponse = JsonConvert.DeserializeObject<PostCreditResponse>(responseDecrypted);

            var result = new PostCreditResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Success"
                },
                PostCreditResponse = cipResponse
            };

            return result;

        }
        catch (Exception ex)
        {
            Utilities.WriteLog("CipClient:PostCredit", $"ERR: {ex.Message}");

            return new PostCreditResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed"
                }

            };
        }
    }
    
    public async Task<TransactionQueryResult> TransactionQuery(TransactionQueryRequest request)
    {
        try
        {
            var cipPublicKeyPath = configuration.GetValue<string>("CIP:CipPublicKeyPath");
            var cipPublicKey = System.IO.File.ReadAllText(cipPublicKeyPath);

            var client = new HttpClient();
            var url = configuration.GetValue<string>("CIP:CipTransactionQueryEndPoint");

            var reqJson = JsonConvert.SerializeObject(request);
            var reqEnc = Utilities.Encrypt(reqJson, cipPublicKey);
         
            Utilities.WriteLog("CipClient:TransactionQuery", $"REQUEST DATA PLAIN: {reqJson}");
            Utilities.WriteLog("CipClient:TransactionQuery", $"REQUEST DATA ENCRYPTED: {reqEnc}");
            Utilities.WriteLog("CipClient:TransactionQuery", $"Sending transaction query request to CIP. Url: {url}");

            var response = await client.PostAsJsonAsync(url, reqEnc);

            var responseContent = await response.Content.ReadAsStringAsync();


            Utilities.WriteLog("CipClient:TransactionQuery", $"RESPONSE STATUS CODE: {response.StatusCode}");
            Utilities.WriteLog("CipClient:TransactionQuery", $"RESPONSE FROM CIP ENCRYPTED: {responseContent}");
            
            var responseDecrypted = Utilities.Decrypt(responseContent);

            Utilities.WriteLog("CipClient:TransactionQuery", $"RESPONSE FROM CIP DECRYPTED: {responseDecrypted}");

            var cipResponse = JsonConvert.DeserializeObject<TransactionQueryResponse>(responseDecrypted);

            var result = new TransactionQueryResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Success"
                },
                TransactionQueryResponse = cipResponse
            };

            return result;

        }
        catch (Exception ex)
        {
            Utilities.WriteLog("CipClient:TransactionQuery", $"ERR: {ex.Message}");

            return new TransactionQueryResult
            {
                ResponseHeader = new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed"
                }

            };
        }
    }
      
}
