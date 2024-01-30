using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NityCityWeb.db;
using NityCityWeb.Models;
using System.Threading.Tasks;

namespace NityCityWeb.Controllers
{
    public class DistrictController : Controller
    {
        private readonly NightCityContext _context;
        private readonly IWebHostEnvironment _env;

        public DistrictController(NightCityContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index() 
        {
            var districts = await _context.Districts.ToListAsync();
            return View(districts);
        }

        private bool ImageExists(string imageUrl)
        {
            // Get the absolute path of the image
            var imagePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            return System.IO.File.Exists(imagePath);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _context.Districts.FirstOrDefaultAsync(m => m.DistrictId == id);

            if (district == null)
            {
                return NotFound();
            }

            // Prepare the list of image names 
            var imageNames = new List<string> { "One", "Two" };
            district.ImageUrls = new List<string>();

            foreach (var imageName in imageNames)
            {
                string formattedDistrictName = district.DistrictName.Replace(" ", "");
                string imageUrl = $"/Pictures/District/{district.DistrictName}/{formattedDistrictName}_Image_{imageName}.jpg";
                // Check if the image exists, and if so, add it to the list
                if (ImageExists(imageUrl))
                {
                    district.ImageUrls.Add(imageUrl);
                }
            }

            var districts = await _context.Districts.OrderBy(d => d.DistrictId).ToListAsync();
            int currentIndex = districts.FindIndex(d => d.DistrictId == id);
            int totalDistricts = districts.Count;

            int prevId = currentIndex > 0 ? districts[currentIndex - 1].DistrictId : districts[totalDistricts - 1].DistrictId;
            int nextId = currentIndex < totalDistricts - 1 ? districts[currentIndex + 1].DistrictId : districts[0].DistrictId;

            ViewBag.PrevId = prevId;
            ViewBag.NextId = nextId;

            return View(district);
        }

    }
}
