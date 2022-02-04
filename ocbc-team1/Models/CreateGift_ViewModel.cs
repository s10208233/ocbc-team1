using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class CreateGift_ViewModel
    {
        public string From_AccountNumber { get; set; }

        [MaxLength(9), MinLength(9)]
        [RegularExpression("^[0-9]{9}$", ErrorMessage = "Please enter a 9 digit Bank Account Number")]
        public string? To_AccountNumber { get; set; }

        [MaxLength(8), MinLength(8)]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Please enter an 8 digit phone number")]
        public string? To_PhoneNumber { get; set; }

        public string? sticker_src { get; set; }

        [MaxLength(3)]
        public string GiftCurrency { get; set; }

        public double Amount { get; set; }

        public string? fail { get; set; }

        [MaxLength(50)]
        public string? Message { get; set; }

    }
}
