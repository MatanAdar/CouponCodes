namespace CouponCodes.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        // Unique coupon code used for discount
        public int CodeCoupon { get; set; }

        // Description of the coupon, visible to admins only
        public string Description { get; set; }

        // ID of the user who created the coupon
        public int UserId { get; set; }

        // The date and time when the coupon was created, automatically set to Now upon creation
        public DateTime CreationDateAndTime { get; set; } = DateTime.Now;

        // Discount type: either a fixed amount or a percentage
        public decimal DiscountAmount { get; set; }  // Discount amount if it's a fixed discount
        public float DiscountPercentage { get; set; } // Discount percentage if it's a percentage discount

        // Expiration date, if applicable (Null if no expiration)
        public DateTime? ExpirationDate { get; set; }

        // Flag indicating if the coupon can be stacked with other discounts
        public bool IsStackable { get; set; }

        // Maximum number of times the coupon can be used
        public int UsageLimit { get; set; }

        // Tracks the number of times the coupon has been used
        public int TimesUsed { get; set; } = 0;

        // Constructor
        public Coupon()
        {
            
        }

        // Function to check if the coupon is still valid - True if valid, False if expired
        public bool IsValid()
        {
            return (!ExpirationDate.HasValue || ExpirationDate > DateTime.Now) &&
                   (UsageLimit == 0 || TimesUsed < UsageLimit);
        }
    }
}
