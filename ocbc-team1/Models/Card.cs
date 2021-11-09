using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class Card
    {
        [Required]
        [MaxLength(16), MinLength(16)]
        public string CardNumber { get; set; }

        [Required]
        [MaxLength(6), MinLength(6)]
        public string CardPIN { get; set; }
    }
}
