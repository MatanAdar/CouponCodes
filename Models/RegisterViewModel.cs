using System.ComponentModel.DataAnnotations;

// Created this register class to controller over the the roles i give for the created account (Admin role)
namespace CouponCodes.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Both password and confirmPassword field need to match to go through
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
