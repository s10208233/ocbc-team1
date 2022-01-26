using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class NewBankAccountViewModel
    {
        [Required]
        public string AccountType { get; set; }

        [Required]
        public double AmountRemaining { get; set; }

        [MaxLength(3)]
        public string AccountCurrency { get; set; }
    }
}
