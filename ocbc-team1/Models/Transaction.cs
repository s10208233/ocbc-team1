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
        public int From_AccountNumber { get; set; }

        [Required]
        public int To_AccountNumber { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public DateTime TimeSent { get; set;}

        [MaxLength(3)]
        public string Currency { get; set; }

        [Required]
        public string? Status { get; set; }
    }
}
