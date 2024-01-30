using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NityCityWeb.db;
using NityCityWeb.Models;
using System.Diagnostics;

namespace NityCityWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NightCityContext _context;

        public HomeController(ILogger<HomeController> logger, NightCityContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var districts = await _context.Districts.ToListAsync();
            return View(districts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
