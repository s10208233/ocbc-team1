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
        public int UserID { get; set; }

        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [Required]
        public string AccountName { get; set; }

        [Required]
        public int CardNumber { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }
    }
}
