using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Authentication
{
    public class Login
    {
        [StringLength(maximumLength: 30, MinimumLength = 3, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        [Required]
        public string Username { get; set; }
        [StringLength(maximumLength: 30, MinimumLength = 6, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }
    }
}
