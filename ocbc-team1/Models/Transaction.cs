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
        public int SenderUserID { get; set; }

        [Required]
        public int RecipientUserID { get; set; }

        [Required]
        public int AmountSent { get; set; }

        public DateTime TimeSent { get; set;}

        public DateTime? TimeRecieved { get; set; }

        public string Status { get; set; }
    }
}
