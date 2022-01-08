using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class Gift
    {
        public User Sender { get; set; }
        public User Receipient { get; set; }
        public Transaction transaction { get; set; }
        public string? sticker_src { get; set; }
        public bool Received { get; set; }
        public string? Message { get; set; }
    }
}
