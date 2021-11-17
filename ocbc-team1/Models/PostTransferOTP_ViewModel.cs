using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class PostTransferOTP_ViewModel
    {
        [Required]
        public TransferViewModel tfvm { get; set; }

        [Required]
        public string OTP { get; set; }
    }
}
