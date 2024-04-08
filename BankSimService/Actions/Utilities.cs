﻿using System.Text;

namespace BankSimService.Actions
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

            var randomDigits = chars.ToString();

            var currentDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            return currentDateTime + randomDigits;

        }

        public static string RandomDigits(int length)
        {
            var random = new Random();
            string s = string.Empty;
            for (int i = 0; i < length; i++)
                s = String.Concat(s, random.Next(10).ToString());
            return s;
        }
    }
}