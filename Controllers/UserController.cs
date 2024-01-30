using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NityCityWeb.Models;
using NityCityWeb.db;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;


namespace NityCityWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly NightCityContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;

        public UserController(NightCityContext context, IConfiguration configuration, ILogger<UserController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // ============================== SIGNUP FUNCTIONS ==============================

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber,Country,DateOfBirth,Password,SecurityLevel")] User user)
        {
            if (ModelState.IsValid)
            {
                // Hash the password before saving it to the database
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                _context.Add(user);
                await _context.SaveChangesAsync();

                // After successfully creating the user, log them in by generating a JWT token
                var tokenString = GenerateJwtToken(user);
                SetJwtTokenCookie(tokenString);

                // Redirect to the Profile page
                return RedirectToAction("Profile", new { id = user.UserId });
            }
            return View(user);
        }

        // ============================== LOGIN FUNCTIONS ==============================

        // GET: User/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginModel.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password))
            {
                // Return the view with an error message
                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return View(loginModel);
            }

            // Generate the JWT token for the user
            var tokenString = GenerateJwtToken(user);

            _logger.LogInformation($"Generated JWT token: {tokenString}");

            // Set the JWT token in an HttpOnly cookie
            SetJwtTokenCookie(tokenString);

            _logger.LogInformation($"Set JWT token in cookie: {tokenString}");

            // Redirect to the Profile page
            return RedirectToAction("Profile", new { id = user.UserId });
        }

        // ============================== PROFILE FUNCTIONS ==============================
        // GET: User/Profile/id
        [Authorize]
        public async Task<IActionResult> Profile(int? id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User is not authenticated. Redirecting to Unauthorized.");
                return Unauthorized();
            }

            if (!id.HasValue)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdStr))
                {
                    _logger.LogWarning("UserID claim is null or empty. Redirecting to Unauthorized.");
                    return Unauthorized();
                }

                if (!int.TryParse(userIdStr, out int userId))
                {
                    _logger.LogError($"Failed to parse userID from claim: {userIdStr}. Redirecting to Unauthorized.");
                    return Unauthorized();
                }
                id = userId;
            }
                        
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User not found with ID: {id.Value}. Redirecting to NotFound.");
                return NotFound();
            }
            try
            {
                var bookingsQuery = _context.Bookings
                    .Where(b => b.UserId == user.UserId)
                    .Include(b => b.District)
                    .Select(b => new
                    {
                        Booking = b,
                        Status = (BookingStatus)b.Status // Convert INT to BookingStatus enum directly
                    });

                var bookingInfos = await bookingsQuery.ToListAsync();
                var bookings = bookingInfos.Select(bi =>
                {
                    var booking = bi.Booking;
                    booking.Status = bi.Status; // Assign the converted status back to the booking
                    return booking;
                }).ToList();

                var currentTime = DateTime.Now;
                var upcomingBookings = bookings.Where(b => b.TripStartDate >= currentTime).ToList();
                var pastBookings = bookings.Where(b => b.TripStartDate < currentTime).ToList();

                ViewBag.UpcomingBookings = upcomingBookings;
                ViewBag.PastBookings = pastBookings;

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching bookings.");
                throw; // or handle the exception as appropriate
            }
        }

        // ============================== JWT COOKIE FUNCTIONS ==============================
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtConfig:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
                    // Add other claims as needed
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Set token expiration as needed
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private void SetJwtTokenCookie(string tokenString)
        {
            _logger.LogInformation($"Setting cookie - JwtToken: {tokenString}");

            Response.Cookies.Append("JwtToken", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to false if you're not using HTTPS
                SameSite = SameSiteMode.Strict, // Adjust as per your requirement
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        // ============================== LOG OUT FUNCTION ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear the JWT token cookie
            Response.Cookies.Delete("JwtToken", new CookieOptions { Secure = true, HttpOnly = true });

            // Redirect to the home page after successful logout
            return RedirectToAction("Index", "Home");
        }

    }
}
