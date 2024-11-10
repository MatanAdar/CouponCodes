using System.ComponentModel.DataAnnotations;

namespace CouponCodes.Models
{
    public class ValidDiscountValueAttribute : ValidationAttribute
    {
        public ValidDiscountValueAttribute() : base("Discount value must be between 0 and 100.")
        {
        }

        // Override the IsValid method to perform custom validation
        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            var discountValue = value.ToString();

            // Check if the input contains '%', indicating it's a percentage
            if (discountValue.Contains("%"))
            {
                // Try to parse the percentage value (without the '%')
                var percentage = discountValue.Replace("%", "").Trim();

                if (decimal.TryParse(percentage, out var parsedPercentage))
                {
                    // Ensure percentage is between 0 and 100
                    
                    return parsedPercentage >= 0 && parsedPercentage <= 100;
                }
                return false;
            }
            else
            {
                // If not a percentage, treat it as a fixed amount
                if (decimal.TryParse(discountValue, out var parsedAmount))
                {
                    // Ensure fixed amount is between 0 and 100
                    return parsedAmount >= 0 && parsedAmount <= 100;
                }
                return false;
            }
        }
    }
}
