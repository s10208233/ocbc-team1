using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class ScheduledTransfer
    {
        [Required]
        [MaxLength(9), MinLength(9)]
        [RegularExpression("^[0-9]{9}$", ErrorMessage = "Please enter one of your account to send from")]
        public string From_AccountNumber { get; set; }

        [Required]
        [MaxLength(9), MinLength(9)]
        [RegularExpression("^[0-9]{9}$", ErrorMessage = "Please enter one of your account to send to")]
        public string? To_AccountNumber { get; set; }

        [MaxLength(8), MinLength(8)]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Please enter an 8 digit phone number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Range(0.00,double.MaxValue, ErrorMessage = "Please enter a number larger than 0")]
        [RegularExpression("[+]?\\d*\\.?\\d+", ErrorMessage = "Please enter a number larger than 0")]
        public double TransferAmount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime TransferDate { get; set; }

        //[Required]
        //[Range(0.00, double.MaxValue, ErrorMessage = "Please enter a number larger than 0")]
        //[RegularExpression("[+]?\\d*\\.?\\d+", ErrorMessage = "Please enter a number larger than 0")]
        //public double TransferRepeated { get; set; }
        public string? fail { get; set; }

    }
}
