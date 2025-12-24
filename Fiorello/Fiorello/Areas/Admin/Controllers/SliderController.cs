
using Fiorello.Areas.Admin.ViewModels.CategoryVM;
using Fiorello.Areas.Admin.ViewModels.SliderVM;
using Fiorello.Data;
using Fiorello.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SliderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SliderController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Slider> sliders = await _context.Sliders.ToListAsync();
            IEnumerable<GetAllSliderVM> getAllSliderVMs = sliders.Select(c => new GetAllSliderVM()
            {
                Id = c.Id,
                Image = c.Image
            });

            return View(getAllSliderVMs);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSliderVM request)
        {
            string fileName = Guid.NewGuid().ToString() + "_" + request.Photo.FileName;

            string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", fileName);

            using (FileStream fileStream = new(path, FileMode.Create))
            {
                request.Photo.CopyTo(fileStream);
            }

            Slider newSlider = new()
            {
                Image = fileName
            };

            await _context.Sliders.AddAsync(newSlider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Detail(int id)
        {
            Slider slider = await _context.Sliders
                .FirstOrDefaultAsync(s => s.Id == id);

            if (slider == null)
                return NotFound();

            DetailSliderVM detailSliderVM = new()
            {
                Id = slider.Id,
                Image = slider.Image
            };

            return View(detailSliderVM);
        }
    }
}
