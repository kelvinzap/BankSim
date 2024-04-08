using System.ComponentModel.DataAnnotations;

namespace BankSim.Models.Dtos
{
    public class TransactionDto
    {
        
        public int Id { get; set; }
        [StringLength(50)]
        public string TransactionId { get; set; }
        
        [StringLength(50)]
        public string MainTransactionId { get; set; }
        [StringLength(50)]
        public string SessionId { get; set; }
        
        [StringLength(50)]
        public string Sector { get; set; }
        public string Group { get; set; }
        [StringLength(200)]
        public string Narration { get; set; }
        public DateTime EntryDate { get; set; }
        public decimal Amount { get; set; }
        public decimal ChargeAmount { get; set; }
        public string TransactionType { get; set; }
        [StringLength(10)]
        public string DestinationBankCode { get; set; }
        [StringLength(10)]
        public string SourceBankCode { get; set; }
        [StringLength(10)]
        public string SourceAccount { get; set; }
        [StringLength(10)]
        public string DestinationAccount { get; set; }
        [StringLength(50)]
        public string DebitAccountName { get; set; }
        [StringLength(50)]
        public string CreditAccountName { get; set; }
        [StringLength(50)]
        public string Channel { get; set; }
        [StringLength(50)]
        public string ResponseCode { get; set; }
        [StringLength(50)]
        public string ResponseMessage { get; set; }
        public string SourceProcessor { get; set; }
        public string DestinationProcessor { get; set; }
        public int RequeryCount { get; set; }
        public DateTime? EntryTime { get; set; } //Recieve Req
        public DateTime? PushTime { get; set; } // Send to Destination
        public DateTime? ResponseTime { get; set; } //Receive from Destination
        public string SettleResponseCode { get; set; } 
    }
}
