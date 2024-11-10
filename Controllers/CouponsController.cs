using Microsoft.AspNetCore.Mvc;
using CouponCodes.Models;
using System.Linq;
using CouponCodes.Data;

namespace CouponCodes.Controllers
{
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _context;

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

        // Display form to edit an existing coupon
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
