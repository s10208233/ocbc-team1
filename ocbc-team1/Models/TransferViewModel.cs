using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class TransferViewModel
    {
        //  First two properties are nullable, fail transaction if both are null.
        [MaxLength(8), MinLength(8)]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Please enter an 8 digit phone number")]
        public string PhoneNumber { get; set; }

        public int From_AccountNumber { get; set; }

        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Please enter an 9 digit bank account number")]
        public int To_AccountNumber { get; set; }
    }
}
