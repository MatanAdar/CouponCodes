﻿using System.ComponentModel.DataAnnotations;

namespace CouponCodes.Models
{
    public class Coupon
    {
        // Constructor
        public Coupon()
        {

        }

        // Incresing Primiry key of coupons
        [Key]
        public int Id { get; set; }


        // Unique coupon code used for discount
        [Required]
        public string CodeCoupon { get; set; }


		// Description of the coupon, visible to admins only
		[Required]
		public string Description { get; set; }


        // ID of the user who created the coupon
        public string? UserId { get; set; }


        // The date and time when the coupon was created, automatically set to Now upon creation
        public DateTime CreationDateAndTime { get; set; } = DateTime.Now;


        //Discount value, which can be either amount or percentage (based on input)
        //Make a validation to the input that not go under 0 or above 100 and only get number, %
        [RegularExpression(@"^(\d+(\.\d{1,2})?%?)$", ErrorMessage = "Please enter a valid discount value (e.g., 20 or 20%).")]
        [ValidDiscountValueAttribute(ErrorMessage = "Discount value must be between 0 and 100, either as an amount or percentage.")]
		[Required]
		public string DiscountValue { get; set; }


        // Expiration date, if applicable (Null if no expiration)
        public DateTime? ExpirationDate { get; set; }


        // Flag indicating if the coupon can be stacked with other coupons discount (Double deal)
        public bool IsStackable { get; set; }


        // Tracks the number of times the coupon has been used
        public int TimesUsed { get; set; } = 0;


        // Maximum number of times the coupon can be used
        public int? UsageLimit { get; set; }

   
        // Check if the coupon date is still valid
        public bool IsDateValid()
        {
            return (!ExpirationDate.HasValue || ExpirationDate > DateTime.Now);
        }


        // Check if the coupon Limit is not reached
        public bool IsUsageValid()
        {
            return UsageLimit == null || TimesUsed < UsageLimit;
        }
    }
}
