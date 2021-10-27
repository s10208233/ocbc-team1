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
        public int UserID { get; set; }

        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [Required]
        public string Name { get; set; }

        [StringLength(6)]
        [Required]
        public string AccessCode { get; set; }

        [StringLength(6)]
        [Required]
        public string BankPIN { get; set; }

        [StringLength(9)]
        [Required]
        public string IC { get; set; }

        [Required]
        public string Nationality { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public int PhoneNumber { get; set; }
    }
}
