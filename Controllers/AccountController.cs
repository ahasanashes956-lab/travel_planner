using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TravelPlanner.Models;
using TravelPlanner.Services;

namespace TravelPlanner.Controllers
{
    /// <summary>
    /// Account Controller for Authentication & Profile Management
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ILogger<AccountController> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Generate email confirmation token
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, protocol: Request.Scheme);

                    await _emailService.SendConfirmationEmailAsync(user.Email, confirmationLink);

                    TempData["Message"] = "Registration successful! Please check your email to confirm your account.";
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.IsVerified = true;
                await _userManager.UpdateAsync(user);
                TempData["Message"] = "Email confirmed successfully! You can now log in.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = "Email confirmation failed.";
            return RedirectToAction("Register");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Please confirm your email first.");
                    return View(model);
                }

                if (user != null && !user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    user.LastLogin = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    return RedirectToLocal(returnUrl);
                }
                if (result.IsLockedOut)
                    ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
                else
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetLink = Url.Action("ResetPassword", "Account",
                        new { userId = user.Id, token = token }, protocol: Request.Scheme);

                    await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
                }

                TempData["Message"] = "If an account exists with that email, you will receive password reset instructions.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token = null)
        {
            if (token == null)
                return BadRequest("Token is required");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                    if (result.Succeeded)
                    {
                        TempData["Message"] = "Password reset successful! You can now log in.";
                        return RedirectToAction("Login");
                    }

                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Message"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // API Endpoints for Frontend
        // ==========================================

        [HttpPost("/api/register")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RegisterApi([FromBody] RegisterViewModel model)
        {
            try
            {
                _logger.LogInformation("RegisterApi called with model: " + (model != null ? $"Email={model.Email}" : "null"));

                if (model == null)
                {
                    _logger.LogWarning("Model is null");
                    return BadRequest(new { message = "Request body is required" });
                }

                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    _logger.LogWarning($"Missing email or password. Email empty: {string.IsNullOrEmpty(model.Email)}, Password empty: {string.IsNullOrEmpty(model.Password)}");
                    return BadRequest(new { message = "Email and password are required" });
                }

                if (model.Password != model.ConfirmPassword)
                {
                    _logger.LogWarning("Passwords do not match");
                    return BadRequest(new { message = "Passwords do not match" });
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName ?? "",
                    LastName = model.LastName ?? "",
                    EmailConfirmed = true // Auto-confirm for testing
                };

                _logger.LogInformation($"Creating user: {model.Email}");
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User created successfully: {user.Id}");
                    return Ok(new { message = "Registration successful", user = new { id = user.Id, email = user.Email } });
                }

                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning($"User creation failed: {errorMessages}");
                var duplicateUser = result.Errors.Any(error =>
                    error.Code is "DuplicateUserName" or "DuplicateEmail");
                return StatusCode(duplicateUser ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest,
                    new { message = errorMessages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Registration error: {ex.Message}");
                return StatusCode(500, new { message = "Registration failed: " + ex.Message });
            }
        }

        [HttpPost("/api/login")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoginApi([FromBody] LoginViewModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin && !user.IsActive)
                {
                    return Unauthorized(new { message = "This account has been rejected." });
                }

                if (!isAdmin && !user.IsVerified)
                {
                    return Unauthorized(new { message = "This account is awaiting admin approval." });
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    user.LastLogin = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    return Ok(new { 
                        message = "Login successful",
                        user = new {
                            id = user.Id,
                            email = user.Email,
                            firstName = user.FirstName,
                            lastName = user.LastName
                        }
                    });
                }

                if (result.IsLockedOut)
                {
                    return Unauthorized(new { message = "Account locked. Try again later." });
                }

                return Unauthorized(new { message = "Invalid email or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "Login failed: " + ex.Message });
            }
        }

        [HttpGet("/api/current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Ok(new { isAuthenticated = false });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phone = user.PhoneNumber,
                city = user.City,
                country = user.Country,
                address = user.Address,
                bio = user.Bio,
                profilePictureUrl = user.ProfilePictureUrl,
                roles,
                isAuthenticated = true
            });
        }

        [HttpPost("/api/profile/photo")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile? photo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Please log in first" });

            if (photo == null || photo.Length == 0)
                return BadRequest(new { message = "Please choose an image" });

            if (photo.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Image must be 5 MB or smaller" });

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Only JPG, PNG, and WebP images are allowed" });

            var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadDirectory);

            var fileName = $"{user.Id}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDirectory, fileName);
            await using (var stream = System.IO.File.Create(filePath))
            {
                await photo.CopyToAsync(stream);
            }

            if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath,
                    user.ProfilePictureUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            user.ProfilePictureUrl = $"/uploads/profiles/{fileName}";
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(error => error.Description)) });

            return Ok(new { message = "Profile photo updated successfully", profilePictureUrl = user.ProfilePictureUrl });
        }

        [HttpPut("/api/profile")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateProfileApi([FromBody] ProfileUpdateViewModel model)
        {
            if (model == null)
                return BadRequest(new { message = "Profile data is required" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Please log in first" });

            user.FirstName = model.FirstName?.Trim();
            user.LastName = model.LastName?.Trim();
            user.PhoneNumber = model.Phone?.Trim();
            user.City = model.City?.Trim();
            user.Country = model.Country?.Trim();
            user.Address = model.Address?.Trim();
            user.Bio = model.Bio?.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errorMessages = string.Join(", ", result.Errors.Select(error => error.Description));
                return BadRequest(new { message = errorMessages });
            }

            return Ok(new
            {
                message = "Profile updated successfully",
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    phone = user.PhoneNumber,
                    city = user.City,
                    country = user.Country,
                    address = user.Address,
                    bio = user.Bio
                    ,profilePictureUrl = user.ProfilePictureUrl
                }
            });
        }

        [HttpPost("/api/forgot-password")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ForgotPasswordApi([FromBody] ForgotPasswordViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(new { message = "Email is required" });

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
                return Ok(new { message = "If the email exists, reset instructions are ready." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password.html?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            return Ok(new
            {
                message = "Password reset link generated",
                resetLink
            });
        }

        [HttpPost("/api/change-password")]
        [Authorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ChangePasswordApi([FromBody] ChangePasswordApiViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword) || string.IsNullOrWhiteSpace(model.ConfirmPassword))
                return BadRequest(new { message = "Current password, new password, and confirmation are required" });

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Please log in again" });

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errorMessages = string.Join(" ", result.Errors.Select(error => error.Description));
                return BadRequest(new { message = errorMessages });
            }

            await _signInManager.RefreshSignInAsync(user);
            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("/api/reset-password")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ResetPasswordApi([FromBody] ResetPasswordApiViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.NewPassword))
                return BadRequest(new { message = "Email, reset token, and new password are required" });

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match" });

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
                return BadRequest(new { message = "Invalid password reset request" });

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                var errorMessages = string.Join(", ", result.Errors.Select(error => error.Description));
                return BadRequest(new { message = errorMessages });
            }

            return Ok(new { message = "Password reset successful" });
        }

        [HttpPost("/api/logout")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LogoutApi()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard");
        }
    }

    // ViewModels
    public class RegisterViewModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class ProfileUpdateViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }
    }

    public class ResetPasswordApiViewModel
    {
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }

    public class ChangePasswordApiViewModel
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
