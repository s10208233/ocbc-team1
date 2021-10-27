using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class LoginViewModel
    {
        [Display(Name = "AccessCode"), Required]
        public string AccessCode { get; set; }
        [Display(Name = "Pin"), Required, DataType(DataType.Password)]
        public string Pin { get; set; }
    }
}
