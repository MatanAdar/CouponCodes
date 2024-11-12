using CouponCodes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CouponCodes.Models; // Adjust according to your models' namespace

// This Controller created to controller over the register data (email,password) and give it a admin role
namespace CouponCodes.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // Get the registration view
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Post registration form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign "Admin" role to the newly created user
                    await _userManager.AddToRoleAsync(user, "Admin");

                    // Automatically sign the user in
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect to home page or desired location
                    return RedirectToAction("CouponEnter", "Coupons");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
    }
}