using CouponCodes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// This Controller created to controller over the register data (email,password) and give it a admin role
namespace CouponCodes.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Registration view
        [Authorize]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Post registration form
        [HttpPost("Register")]
        [Authorize] // This ensures only authenticated users can register
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Automatically sign the user in
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Return success message
                    return Ok("Register/create new account Successfully");
                }

                // If registration fails, show the errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                // Return a 400 Bad Request with validation errors
                return BadRequest(ModelState);
            }

            return BadRequest(ModelState);
        }


        // GET: Registration view
        [HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		// Post registration form
		[HttpPost("Login")]
		/*[ValidateAntiForgeryToken]*/
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user != null)
				{
					var result = await _signInManager.PasswordSignInAsync(user, model.Password,model.RememberMe, false);
					if (result.Succeeded)
					{
                        // Return the token in the response
                        return Ok("Login Successfully");
                        /*return RedirectToAction("CouponEnter", "Coupons"); //*/
                    }
					ModelState.AddModelError(string.Empty, "Invalid login attempt. Email or password are wrong!");
                    return BadRequest("Invalid login attempt. Email or password are wrong!");
				}
				else
				{
					ModelState.AddModelError(string.Empty, "User does not exist.");
                    return NotFound("User not found!");
				}
			}
            return Ok(model);
			//return View(model);
		}
    }
}