using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class Transaction
    {
        [Required]
        public int TransactionID { get; set; }

        [Required]
        public int Sender_AccountNumber { get; set; }

        [Required]
        public int Recipient_AccountNumber { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public DateTime TimeSent { get; set;}

        [Required]
        public DateTime? TimeRecieved { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
