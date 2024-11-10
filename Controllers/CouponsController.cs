using CouponCodes.Data;
using CouponCodes.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CouponCodes.Controllers
{
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Static variable to hold the final price between requests
        private static decimal finalPrice = 100m;  // Initialize with the base price (100)


        public CouponsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all coupons
        public IActionResult Index()
        {
            var coupons = _context.Coupon.ToList();
            return View(coupons);
        }

        // GET: Coupons/CouponEnter
        public IActionResult CouponEnter()
        {
            return View();
        }

        // Function to make the parse to the discount value (precentege or fixed) and lower it from our price
        [HttpPost]
        public IActionResult ShowPriceAfterCut(string CouponPhrase)
        {
            decimal baseOrderPrice = 100m;  // The fixed order price

            // If static finalPrice has already been set, use that as the base price
            decimal currentPrice = finalPrice;

            // Find the coupon in the database using the provided code
            var coupon = _context.Coupon.FirstOrDefault(c => c.CodeCoupon == CouponPhrase);

            if (coupon != null)
            {
                if (coupon.DiscountValue.Contains("%")) // Checking if discount input is percentage
                {
                    var percentage = coupon.DiscountValue.Replace("%", "").Trim();
                    if (decimal.TryParse(percentage, out var parsedPercentage))
                    {
                        if (parsedPercentage >= 0 && parsedPercentage <= 100)
                        {
                            // Apply discount percentage based on the latest final price (static finalPrice)
                            currentPrice -= currentPrice * (parsedPercentage / 100);
                        }
                    }
                }
                else // Discount input is fixed price
                {
                    var parsedFixed = decimal.Parse(coupon.DiscountValue);
                    currentPrice -= parsedFixed;
                }

                // Make sure the final price doesn’t go below zero
                currentPrice = Math.Max(currentPrice, 0);
            }
            else // Coupon not found in the database
            {
                ModelState.AddModelError("", "Invalid coupon code.");
                currentPrice = baseOrderPrice;  // Reset to base price
            }

            // Update the static finalPrice variable
            finalPrice = currentPrice;

            // Pass the final price and coupon status to the view
            ViewBag.DiscountedPrice = currentPrice;

            // Return to the "CouponEnter" view
            return View("CouponEnter");
        }

        // Display form to create a new coupon
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create a new coupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                _context.Coupon.Add(coupon);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        // Display existing form to edit the coupon
        public IActionResult Edit(int id)
        {
            var coupon = _context.Coupon.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        // POST: Edit an existing coupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                _context.Coupon.Update(coupon);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        // Display confirmation page to delete a coupon
        public IActionResult Delete(int id)
        {
            var coupon = _context.Coupon.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        // POST: Delete a coupon
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var coupon = _context.Coupon.Find(id);
            if (coupon != null)
            {
                _context.Coupon.Remove(coupon);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
