using BankSim.Models.Database;
using BankSim.Models.Database.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Text;

namespace BankSim.Actions
{
    public class Utilities
    {

        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static void WriteLog(string method, string message)
        {
            try
            {
                if (!Directory.Exists(_configuration.GetValue<string>("LogPath")))
                    Directory.CreateDirectory(_configuration.GetValue<string>("LogPath"));

                var writer = new StreamWriter($"{_configuration.GetValue<string>("LogPath")}/" +
                                              $"{DateTime.Today:dd-MMM-yyyy}.Txt", true);
                writer.Write($"~{DateTime.Now} | Method: {method} | Message: {message} | {Environment.NewLine}");
                writer.Close();
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        public static string Encrypt(string payLoad, string publicKeyData)
        {
            try
            {
                var enc = CoralPay.Cryptography.Pgp.Invoke.Encrypt(new CoralPay.Cryptography.Pgp.Models.EncryptionParam
                {
                    ToEncryptText = payLoad,
                    ExternalPublicKeyStream = new MemoryStream(Encoding.ASCII.GetBytes(publicKeyData))
                }).Result;

                if (enc.Header.ResponseCode != "00")
                    WriteLog("Utility.Encrypt", enc.Header.ResponseMessage);
                return enc.Encryption;
            }
            catch (Exception ex)
            {
                WriteLog("Utility.Encrypt", $"Err: {ex.Message}");
                return null;
            }
        }

        public static string Decrypt(string payLoad)
        {
            var pubKeyPath = _configuration.GetValue<string>("PublicKeyPath");
            var privKeyPath = _configuration.GetValue<string>("PrivateKeyPath");
            var keyPassword = _configuration.GetValue<string>("KeyPassword");

            try
            {
                var publicKey = File.ReadAllText(pubKeyPath);
                var privateKey = File.ReadAllText(privKeyPath);

                var dec = CoralPay.Cryptography.Pgp.Invoke.Decrypt(new CoralPay.Cryptography.Pgp.Models.DecryptionParam
                {
                    EncryptedData = payLoad,
                    InternalKeyPassword = keyPassword,
                    InternalPublicKeyStream = new MemoryStream(Encoding.ASCII.GetBytes(publicKey)),
                    InternalPrivateKeyStream = new MemoryStream(Encoding.ASCII.GetBytes(privateKey))
                }).Result;



                if (dec.Header.ResponseCode != "00")
                    WriteLog("Utility.Decrypt", dec.Header.ResponseMessage);

                return dec.Decryption;
            }
            catch (Exception ex)
            {
                WriteLog("Utility.Decrypt", $"Err: {ex.Message}");
                return null;
            }
        }

        public static  string GenerateSessionId()
        {
            var allowedChars = "0123456789";
            var randomDigitsLength = 10; // Random digits
            

            var random = new Random();

            var chars = new char[randomDigitsLength];
            for (int i = 0; i < randomDigitsLength; i++)
            {
                chars[i] = allowedChars[random.Next(0, allowedChars.Length)];
            }

            var randomDigits = new string(chars);

            var currentDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            return currentDateTime + randomDigits;

        }

        public static IEnumerable<BankDto> GetAllBanks()
        {
            
            var filePath = _configuration.GetValue<string>("Banks");
            var banks = new List<BankDto>();
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (sr.Peek() >= 0)
                {
                    string BankLine = sr.ReadLine();

                    if (string.IsNullOrEmpty(BankLine))
                        break;

                    var bank = new BankDto();

                    bank.Name = BankLine[11..];
                    bank.Code = BankLine.Substring(4, 6);
                    banks.Add(bank);
                }
                    
                return banks;
            }
            
            
        }

        public static async Task PerformReversal(TransactionTb transaction)
        {
            try
            {
                var bankCode = _configuration.GetValue<string>("BankCode");
                var allowReversal = _configuration.GetValue<bool>("AllowReversal");

                if (!allowReversal) return;

                var context = new BankSimDbContext();
                var sessionId = bankCode + Utilities.GenerateSessionId();
                var account = await context.AccountTb.SingleOrDefaultAsync(x => x.AccountNumber == transaction.SourceAccount);

                account.Balance += transaction.Amount;

                var reversalTransaction = new TransactionTb
                {
                    Amount = transaction.Amount,
                    Channel = transaction.Channel,
                    ChargeAmount = 0,
                    CreditAccountName = transaction.DebitAccountName,
                    DestinationAccount = transaction.SourceAccount,
                    DestinationBankCode = transaction.SourceBankCode,
                    EntryDate = DateTime.Now,
                    Narration = transaction.Narration,
                    ResponseCode = "00",
                    ResponseMessage = "Success",
                    SessionId = sessionId,
                    TransactionId = CoralPay.Miscellaneous.UniqueNumber(),
                    TransactionType = "REVERSAL",
                    MainTransactionId = transaction.SessionId
                };

                await context.TransactionTb.AddAsync(reversalTransaction);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                return;
            }
        }

    }
}
