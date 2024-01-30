using Microsoft.AspNetCore.Mvc;
using NityCityWeb.db;
using NityCityWeb.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NityCityWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly NightCityContext _context;
        private readonly ILogger<UserController> _logger;
        public BookingController(NightCityContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================== CREATE BOOKING ==============================

        // GET: Booking/Create
        public IActionResult Create()
        {
            // You might want to pass a list of districts to the view for the user to choose from
            ViewBag.Districts = _context.Districts.ToList();
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DistrictId,TripStartDate,TripEndDate,NumberOfTravelers")] Booking booking)
        {
            _logger.LogInformation("Creating a new booking.");

            // Set BookingDate to the current date
            booking.BookingDate = DateTime.Now;

            // Ensure TripStartDate is before TripEndDate
            if (booking.TripStartDate >= booking.TripEndDate)
            {
                ModelState.AddModelError("TripEndDate", "The trip end date must be after the start date.");
                return View(booking);
            }

            // Ignore validation for navigation properties
            ModelState.Remove("User");
            ModelState.Remove("District");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        _logger.LogWarning($"Validation error for '{modelStateKey}': {error.ErrorMessage}");
                    }
                }

                ViewBag.Districts = _context.Districts.ToList(); // Repopulate districts in case of failure
                return View(booking);
            }

            // Set the UserId from the currently logged-in user
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning($"userIdClaim: {userIdClaim}");

            if (!int.TryParse(userIdClaim, out int userIdInt))
            {
                _logger.LogWarning("Failed to parse user ID from claims.");
                ModelState.AddModelError(string.Empty, "Invalid user.");
                ViewBag.Districts = _context.Districts.ToList();
                return View(booking);
            }

            booking.UserId = userIdInt;

            // Set the booking status
            booking.Status = BookingStatus.Confirmed;

            // Fetch the appropriate price per traveler based on the DistrictId and booking dates
            var pricing = await _context.Pricings
                                        .Where(p => p.DistrictId == booking.DistrictId
                                                    && p.StartDate <= booking.TripStartDate
                                                    && p.EndDate >= booking.TripEndDate)
                                        .OrderBy(p => p.StartDate) // Optional: ensures you get the most relevant price
                                        .FirstOrDefaultAsync();

            decimal pricePerTraveler = pricing?.PricePerPerson ?? 0;

            if (pricePerTraveler == 0)
            {
                _logger.LogInformation("Specific pricing not found, fetching default pricing.");
                // Fetch default price if no specific pricing is found
                var defaultPricing = await _context.Pricings
                                                   .Where(p => p.DistrictId == booking.DistrictId)
                                                   .Select(p => p.DefaultPrice)
                                                   .FirstOrDefaultAsync();

                if (defaultPricing > 0)
                {
                    pricePerTraveler = defaultPricing;
                    _logger.LogInformation($"Using default pricing: {defaultPricing}.");
                }
                else
                {
                    // Handle cases where there is no pricing information at all
                    _logger.LogWarning("No pricing information available for the selected district.");
                    ModelState.AddModelError(string.Empty, "No pricing information available for the selected district.");
                    ViewBag.Districts = _context.Districts.ToList();
                    return View(booking);
                }
            }

            // Calculate the total price based on the number of travelers and the price per person
            booking.TotalPrice = booking.NumberOfTravelers * pricePerTraveler;

            _logger.LogInformation($"Total price for booking: {booking.TotalPrice}.");

            try
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Booking saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving booking.");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the booking.");
                ViewBag.Districts = _context.Districts.ToList();
                return View(booking);
            }

            return RedirectToAction(nameof(MyBookings)); // Redirect to the user's bookings
        }

        // ============================== VIEW BOOKING ==============================
        // GET: Booking/Index
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings.Include(b => b.User).Include(b => b.District).ToListAsync();
            return View(bookings);
        }

        // ============================== VIEW USER BOOKINGS ==============================

        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Use NameIdentifier to match the Create method
            if (!int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized(); // or RedirectToAction("Login", "User") if you prefer
            }

            var bookings = await _context.Bookings
                                         .Include(b => b.District) 
                                         .Where(b => b.UserId == userIdInt)
                                         .ToListAsync();

            return View(bookings);
        }
    }
}
