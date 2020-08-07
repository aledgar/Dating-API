using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Dtos
{
    public class UserForLoginDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; } 
    }
}
