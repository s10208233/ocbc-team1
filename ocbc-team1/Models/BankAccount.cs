using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class BankAccount
    {
        [Required]
        public int AccountNumber { get; set; }

        [Required]
        public string AccountType { get; set; }

        [Required]
        public double AmountAvaliable { get; set; }

        [Required]
        public double AmountRemaining { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }
    }
}
