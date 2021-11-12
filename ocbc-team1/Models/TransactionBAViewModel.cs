﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class TransactionBAViewModel
    {
        [Required]
        [MaxLength(9), MinLength(9)]
        [RegularExpression("^[0-9]{9}$", ErrorMessage = "Please enter one of your account to send from")]
        public string From_AccountNumber { get; set; }

        [Required]
        [MaxLength(9), MinLength(9)]
        [RegularExpression("^[0-9]{9}$", ErrorMessage = "Please enter one of your account to send to")]
        public string To_AccountNumber { get; set; }

        [Required]
        public double TransferAmount { get; set; }
    }
}
