using ClosedXML.Excel;
using CouponCodes.Data;
using CouponCodes.Models;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace CouponCodes.Controllers
{
    public class CouponsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        // Static variable to hold the final price between requests
        private static decimal finalPrice = 100m;

        private static bool CheckIfDoubleDealAllow = true;


        public CouponsController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // ********************************** EnterCoupons system Functions ************************************

        // View Function
        // GET: Coupons/CouponEnter
        public IActionResult CouponEnter()
        {
            return View();
        }


        // POST: Handle coupon enter input and reduce the discount from the latest price
        // Function to make the parse to the discount value (precentege or fixed) and lower it from our price
        [HttpPost]
        public IActionResult ShowPriceAfterCut(string CouponPhrase)
        {
            decimal baseOrderPrice = 100m;  // The fixed order price

            // Get the current price after the latest discount
            decimal currentPrice = finalPrice;

            // Find the coupon in the database using the provided code
            var coupon = _context.Coupon.FirstOrDefault(c => c.CodeCoupon == CouponPhrase);

            if (coupon != null)
            {
                // Checking if allow double deal
                if (CheckIfDoubleDealAllow)
                {
                    // Checking if coupon non stackable
                    if (!coupon.IsStackable)
                    {
                        // If true, make CheckIfDoubleDealAllow to false to prevent from the next coupons to work
                        CheckIfDoubleDealAllow = false;
                    }

                    // Checking valid date and Usage of coupon
                    if (coupon.IsDateValid() && coupon.IsUsageValid())
                    {
                        // Check if the discount is a percentage
                        if (coupon.DiscountValue.Contains("%"))
                        {
                            var percentage = coupon.DiscountValue.Replace("%", "").Trim();
                            if (decimal.TryParse(percentage, out var parsedPercentage) && parsedPercentage >= 0 && parsedPercentage <= 100)
                            {
                                currentPrice -= currentPrice * (parsedPercentage / 100);
                            }
                        }
                        else // Discount is a fixed amount
                        {
                            if (decimal.TryParse(coupon.DiscountValue, out var parsedFixed))
                            {
                                currentPrice -= parsedFixed;
                                if (currentPrice < 0)
                                {
                                    ModelState.AddModelError("", "Price is 0, cant go any lower!.");
                                    return BadRequest(ModelState); // HTTP 400
                                }
                            }
                        }

                        // Ensure final price doesn’t go below zero
                        currentPrice = Math.Max(currentPrice, 0);


                        // Increment the coupon's TimesUsed count
                        coupon.TimesUsed += 1;

                        // Save the updated coupon data to the database
                        _context.Update(coupon);
                        _context.SaveChanges();
                    }
                    else if(!coupon.IsDateValid()) // Coupon is expired
                    {
                        ModelState.AddModelError("", "This coupon has expired.");
                        return StatusCode(410, ModelState); // HTTP 410 Gone
                    }
                    else // Coupon usage limit has been reached
                    {
                        ModelState.AddModelError("", "This coupon usage limit has been reached.");
                        return StatusCode(403, ModelState); // HTTP 403 Forbidden
                    }
                }
                else // Last coupon was non stackable
                {
                    ModelState.AddModelError("", "A non-stackable coupon has already been applied. No further discounts are allowed.");
                }
            }
            else  // Coupon not found in the database
            {
                ModelState.AddModelError("", "Invalid coupon code.");
                return NotFound(ModelState); // HTTP 404 Not Found
            }

            // Update the static finalPrice variable
            finalPrice = currentPrice;

            // Pass the final price to the view
            ViewBag.DiscountedPrice = currentPrice;

            // Return to the "CouponEnter" view
            return View("CouponEnter");
        }


        // *********************************** Coupons system Functions *************************************

        // View function
        // GET: Coupons/Index go to this page and show all the copouns
        // Only admin can see the coupons and create/delete and etc
        [Authorize]
        public IActionResult Index()
        {
            // Display all coupons
            var coupons = _context.Coupon.ToList();
            return View(coupons);
        }


        //View function
        // GET: Display form to create a new coupon
        // Only admin can do this (beacuse only admin can enter to the coupons page)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create a new coupon (Excute the command)
        // Only admin can do this (beacuse only admin can enter to the coupons page)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {

                // Fetch user data
                // We have UserId header in the database(Generate when created account),
                // but i want that the coupon userId will be the email of the account that made the coupon becuase its unique
                // and when filter by userId in the reports we can enter something easier like email and not generated userId
                var user = await _userManager.GetUserAsync(User);
                if(user != null)
                {
                    coupon.UserId = user.Email;
                }

                /*// Fetch userId the user who create the coupon (decided to use email and not userId)
                var userId = _userManager.GetUserId(User);
                if(userId != null)
                {
                    coupon.UserId = userId;
                }*/

                // Add the coupon to the database
                _context.Coupon.Add(coupon);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            // If the model state is invalid, return the same view with the coupon object
            return View(coupon);
        }

        //View function
        // GET: Display existing form details of the coupon
        // Only admin can do this (beacuse only admin can enter to the coupons page)
        public IActionResult Details(int id)
        {
            var coupon = _context.Coupon.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        //View function
        // GET: Display existing form to edit the coupon
        // Only admin can do this (beacuse only admin can enter to the coupons page)
        public IActionResult Edit(int id)
        {
            var coupon = _context.Coupon.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        // POST: Edit an existing coupon (Excute the command)
        // Only admin can do this (beacuse only admin can enter to the coupons page)
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

        //View function
        // Display confirmation page to delete a coupon
        // Only admin can do this (beacuse only admin can enter to the coupons page)
        public IActionResult Delete(int id)
        {
            var coupon = _context.Coupon.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        // POST: Delete a coupon(Excute the command)
        // Only admin can do this (beacuse only admin can enter to the coupons page)
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

        // *********************************** Reports system functions *************************************

        // View Function
        // GET: Coupons/CouponsReport
        [Authorize]
        public IActionResult CouponsReport()
        {
            var coupons = _context.Coupon.ToList();
            return View(coupons); // Make sure to pass an empty list if there are no coupons
        }

        // Function that filter coupons by UserId
        public IActionResult FilterCouponsByUser(string userId)
        {
            var coupons = _context.Coupon.Where(c => c.UserId == userId).ToList();
            TempData["FilteredCoupons"] = JsonConvert.SerializeObject(coupons); // Store filtered data for the excel
            return View("CouponsReport", coupons);
        }

        // Function that filter coupons by Date
        public IActionResult FilterCouponsByDate(DateTime startDate, DateTime endDate)
        {
            var coupons = _context.Coupon.Where( c => c.CreationDateAndTime >= startDate && c.CreationDateAndTime <= endDate ).ToList();
            // Store filtered data for the excel (convert from .net object to string)
            TempData["FilteredCoupons"] = JsonConvert.SerializeObject(coupons); 
            return View("CouponsReport",coupons);
        }

        // Generate coupons excel (through ClosedXML Package)
        [HttpGet]
        public async Task<FileResult> ExportCouponsInExcel()
        {
            List<Coupon> coupons;

            // Get filtered data from TempData if available
            if (TempData["FilteredCoupons"] != null)
            {
                // get filter coupons (convert back from json to list object)
                coupons = JsonConvert.DeserializeObject<List<Coupon>>(TempData["FilteredCoupons"].ToString());
            }
            else
            {
                coupons = _context.Coupon.ToList(); // Default to full list if no filter applied
            }

            var fileName = "Coupons.xlsx";
            return GenerateExcel(fileName, coupons);
        }

        private FileResult GenerateExcel(string fileName, IEnumerable<Coupon> coupons)
        {
            DataTable dataTable = new DataTable("Coupons");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Id"),
                new DataColumn("CodeCoupon"),
                new DataColumn("Description"),
                new DataColumn("UserId"),
                new DataColumn("CreationDateAndTime"),
                new DataColumn("DiscountValue"),
                new DataColumn("ExpirationDate"),
                new DataColumn("IsStackable"),
                new DataColumn("TimesUsed"),
                new DataColumn("UsageLimit"),
            });

            foreach (var coupon in coupons)
            {
                dataTable.Rows.Add(
                    coupon.Id,
                    coupon.CodeCoupon,
                    coupon.Description,
                    coupon.UserId,
                    coupon.CreationDateAndTime,
                    coupon.DiscountValue,
                    coupon.ExpirationDate,
                    coupon.IsStackable,
                    coupon.TimesUsed,
                    coupon.UsageLimit
                    );
            }

            // Scope for create the excel
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);

                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }

        }

    }
}

