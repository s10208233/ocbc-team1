using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ocbc_team1.Models
{
    public class UpdateUserProfileForm
    {
        [Display(Name = "Select Profile Picture")]
        public IFormFile? ProfilePictureFile { get; set; }
        public string? ProfilePic_StringIdentifier { get; set; }
        public string? ProfilePic_Url { get; set; }

        [RegularExpression(@"[\w-]+@([\w-]+\.)+[\w-]+", ErrorMessage = "Please enter your email address")]
        public string Email { get; set; }

        [Required]
        [MaxLength(8), MinLength(8)]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "Please enter an 8 digit phone number")]
        public string PhoneNumber { get; set; }
    }
}
