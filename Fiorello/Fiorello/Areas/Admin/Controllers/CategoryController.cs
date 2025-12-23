using Fiorello.Areas.Admin.ViewModels.CategoryVM;
using Fiorello.Data;
using Fiorello.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Category> categories = await _context.Categories.ToListAsync();
            IEnumerable<GetAllCategoryVM> getAllCategoryVMs = categories.Select(c => new GetAllCategoryVM()
            {
                Name = c.Name
            });

            return View(getAllCategoryVMs);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid) return View(categoryVM);

            var dbName = await _context.Categories.FirstOrDefaultAsync(m => m.Id == 6);

            if (dbName != null)
            {
                string ctName = dbName.Name.ToLower().Trim();
            }

            string newName = categoryVM.Name.ToLower().Trim();

            bool isExist = await _context.Categories.AnyAsync(c => c.Name.ToUpper().Trim() ==
                                                         categoryVM.Name.ToUpper().Trim());

            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu adda movcuddur!");
                return View(categoryVM); 
            }

            Category category = new Category
            {
                Name = categoryVM.Name.Trim()
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
