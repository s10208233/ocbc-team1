using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class User
    {
        [Required]
        [MaxLength(6), MinLength(6)]
        public string AccessCode { get; set; }

        [Required]
        [RegularExpression(@"[\w-]+@([\w-]+\.)+[\w-]+", ErrorMessage = "Please enter your email address")]
        public string Email { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Exceeded 50 characters")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Please enter alphabets only")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Exceeded 50 characters")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Please enter alphabets only")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(8), MinLength(8)]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Please enter the last 8 digits of your card")]
        public string CardNumber { get; set; }

        [Required]
        [MaxLength(6), MinLength(6)]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "Please enter your 6 digit PIN")]
        public string BankPIN { get; set; }

        [StringLength(9)]
        [Required]
        [RegularExpression("^(S|T|F|G)[0-9]{7}[A-Z]{1}$", ErrorMessage = "Please enter your a valid IC format")]

        public string IC { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(8), MinLength(8)]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Please enter an 8 digit phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        public List<BankAccount> AccountsList { get; set; }
        [Required]
        public List<Transaction> TransactionList { get; set; }

        public int? TelegramChatID { get; set; }

        //  ASSIGNMENT 2
        public List<Gift>? GiftList { get; set; }
    }
}
