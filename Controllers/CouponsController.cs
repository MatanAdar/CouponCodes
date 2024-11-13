using ClosedXML.Excel;
using CouponCodes.Data;
using CouponCodes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;

namespace CouponCodes.Controllers
{
    public class CouponsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly DataContext _context;

        // Static variable to hold the final price between requests
        private static decimal finalPrice = 100m;

        private static bool CheckIfDoubleDealAllow = true;


        public CouponsController(UserManager<IdentityUser> userManager, DataContext context)
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
		[HttpPost("ShowPriceAfterCut")]
		public IActionResult ShowPriceAfterCut(string CouponPhrase)
        {
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
                    return BadRequest(ModelState);
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
            return Ok(new { DiscountedPrice = currentPrice });
			/*return View("CouponEnter");*/
		}


        // *********************************** Coupons system Functions *************************************

        // View function
        // GET: Coupons/Index go to this page and show all the copouns
        // Only admin can see the coupons and create/delete and etc
        [Authorize]
        [HttpGet("CouponsList")]
        public IActionResult Index()
        {
            // Display all coupons
            var coupons = _context.Coupon.ToList();
            return Ok(coupons);
			/*return View(coupons);*/
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
        [HttpPost("CreateCoupon")]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] Coupon coupon)
        {
            if (ModelState.IsValid)
            {

				// Check if the coupon code already exists in the database
				var existingCoupon =  _context.Coupon.Any(c => c.CodeCoupon == coupon.CodeCoupon);

				if (existingCoupon)
				{
                    // If the coupon code already exists, add an error to the model state
                    ModelState.AddModelError("CodeCoupon", "This coupon code already exists. Choose another one");
                    return BadRequest("This coupon code already exists.Choose another one");
					/*return View(coupon); // Return the view with the error*/
				}

                // Fetch user data
                // We have UserId header in the database(Generate when created account),
                // but i want that the coupon userId will be the email of the account that made the coupon becuase its unique
                // and when filter by userId in the reports we can enter something easier like email and not generated userId
                var user = await _userManager.GetUserAsync(User);
                if(user != null)
                {
                    coupon.UserId = user.Email;
                }

                if (coupon.UsageLimit == 0)
                {
                    coupon.UsageLimit = null;
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

                return Ok("Coupon created Successfully");
                /*return RedirectToAction("Index");*/
            }

			// If the model state is invalid, return the same view with the coupon object
			return BadRequest(ModelState);
			//return View(coupon);
		}

        //View function
        // GET: Display existing form details of the coupon
        // Only admin can do this (beacuse only admin can enter to the coupons page)
        [HttpGet("GetCouponDetails")]
        [Authorize]
        public IActionResult Details(int id)
        {
            var coupon = _context.Coupon.Find(id);
            if (coupon == null)
            {
                return NotFound("Coupon not found!");
            }

            return Ok(coupon);
            /*return View(coupon);*/
        }

        //View function
        // GET: Display existing form to edit the coupon
        // Only admin can do this (beacuse only admin can enter to the coupons page)
        [Authorize]
        public IActionResult Edit(int id)
        {
            var coupon = _context.Coupon.Find(id);
            if (coupon == null)
            {
                return NotFound("Coupon not found!");
            }
            return Ok(coupon);
            /*return View(coupon);*/
        }

        // POST: Edit an existing coupon (Excute the command)
        // Only admin can do this (beacuse only admin can enter to the coupons page)
        [HttpPost("EditCoupon")]
        [Authorize]
        /*[ValidateAntiForgeryToken]*/
        public IActionResult Edit(int id, Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                _context.Coupon.Update(coupon);
                _context.SaveChanges();
                return Ok(coupon);
                /*return RedirectToAction("Index");*/
            }
            return BadRequest("Invalid coupon data.");
            /* return View(coupon);*/
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
            return Ok(coupon);
            /*return View(coupon);*/
        }

        // POST: Delete a coupon(Excute the command)
        // Only admin can do this (beacuse only admin can enter to the coupons page)
        [HttpPost("DeleteCoupon"), ActionName("Delete")]
        [Authorize]
        /*[ValidateAntiForgeryToken]*/
        public IActionResult DeleteConfirmed(int id)
        {
            var coupon = _context.Coupon.Find(id);
            if (coupon != null)
            {
                _context.Coupon.Remove(coupon);
                _context.SaveChanges();
            }
            return Ok("Deleted Successfully");
            /*return RedirectToAction("Index");*/
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
        [HttpPost("CouponsByUser")]
        public IActionResult FilterCouponsByUser(string userId)
        {
            var coupons = _context.Coupon.Where(c => c.UserId == userId).ToList();
            TempData["FilteredCoupons"] = JsonConvert.SerializeObject(coupons); // Store filtered data for the excel
            return Ok(coupons);
            /*return View("CouponsReport", coupons);*/
        }

        // Create a class to handle the DateRange input
        public class DateRangeRequest
        {
            [JsonConverter(typeof(IsoDateTimeConverter))]
            public DateTime StartDate { get; set; }

            [JsonConverter(typeof(IsoDateTimeConverter))]
            public DateTime EndDate { get; set; }
        }

        // Function that filter coupons by Date
        [HttpPost("CouponsByDate")]
        public IActionResult FilterCouponsByDate([FromBody] DateRangeRequest dateRange)
        {
            var coupons = _context.Coupon.Where(c => c.CreationDateAndTime >= dateRange.StartDate && c.CreationDateAndTime <= dateRange.EndDate).ToList();
            // Store filtered data for the excel (convert from .net object to string)
            TempData["FilteredCoupons"] = JsonConvert.SerializeObject(coupons);
            return Ok(coupons);
            /*return View("CouponsReport",coupons);*/
        }

        // Generate coupons excel (through ClosedXML Package)
        [HttpGet("GenerateToExcel")]
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

