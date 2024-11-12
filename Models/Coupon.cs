using System.ComponentModel.DataAnnotations;

namespace CouponCodes.Models
{
    public class Coupon
    {
        // Constructor
        public Coupon()
        {

        }
        [Key]
        public int Id { get; set; }

        // Unique coupon code used for discount
        public string CodeCoupon { get; set; }

        // Description of the coupon, visible to admins only
        public string Description { get; set; }

        // ID of the user who created the coupon
        public string? UserId { get; set; }

        // The date and time when the coupon was created, automatically set to Now upon creation
        public DateTime CreationDateAndTime { get; set; } = DateTime.Now;

        //Discount value, which can be either amount or percentage (based on input)
        //Make a validation to the input that not go under 0 or above 100 and only get number, %
        [RegularExpression(@"^(\d+(\.\d{1,2})?%?)$", ErrorMessage = "Please enter a valid discount value (e.g., 20 or 20%).")]
        [ValidDiscountValueAttribute(ErrorMessage = "Discount value must be between 0 and 100, either as an amount or percentage.")]
        public string DiscountValue { get; set; }

        // Expiration date, if applicable (Null if no expiration)
        public DateTime? ExpirationDate { get; set; }

        // Flag indicating if the coupon can be stacked with other coupons discount
        public bool IsStackable { get; set; }

        // Maximum number of times the coupon can be used
        public int? UsageLimit { get; set; }

        // Tracks the number of times the coupon has been used
        public int TimesUsed { get; set; } = 0;

        // Check if the coupon is still valid - True if valid, False if expired
        public bool IsDateValid()
        {
            return (!ExpirationDate.HasValue || ExpirationDate > DateTime.Now);
        }

        // Check if the coupon is still valid - True if valid, False if surpass the usage limit he have
        public bool IsUsageValid()
        {
            return UsageLimit == null || TimesUsed < UsageLimit;
        }
    }
}
