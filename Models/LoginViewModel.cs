using System.ComponentModel.DataAnnotations;

namespace CouponCodes.Models
{
    public class LoginViewModel
    {

        // Email 
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Password
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Remember me option (like all the login page websites)
        public bool RememberMe { get; set; }
    }
}
