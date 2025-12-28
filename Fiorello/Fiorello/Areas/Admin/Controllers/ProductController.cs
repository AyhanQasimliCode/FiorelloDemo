using Fiorello.Areas.Admin.ViewModels.ProductVM;
using Fiorello.Data;
using Fiorello.Helpers;
using Fiorello.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<GetAllProductVM> getAllProductVMs = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Select(p => new GetAllProductVM
                {
                    Id = p.Id,
                    MainImage = p.ProductImages.FirstOrDefault(m => m.IsMain).Image,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name
                }).OrderByDescending(m => m.Id).ToListAsync();

            return View(getAllProductVMs);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await GetAllCategories();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetAllCategories();
                return View(request);
            }

            if (!request.MainImage.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("MainImage", "Shekil tipi olmalidir!");
                ViewBag.Categories = await GetAllCategories();
                return View(request);
            }

            if (request.MainImage.Length / 1024 > 2048)
            {
                ModelState.AddModelError("MainImage", "Shekilin olcusu max 2MB ola biler!");
                ViewBag.Categories = await GetAllCategories();
                return View(request);
            }

            string mainFileName = Guid.NewGuid().ToString() + "_" + request.MainImage.FileName;
            string mainPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", mainFileName);

            using (FileStream stream = new(mainPath, FileMode.Create))
            {
                await request.MainImage.CopyToAsync(stream);
            }
            bool isExist = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);

            if (!isExist)
            {
                ViewBag.Categories = await GetAllCategories();
                ModelState.AddModelError("CategoryId", "Category tapilmadi!");
                return View();
            }
            List<ProductImage> productImages = new();
            productImages.Add(new ProductImage
            {
                Image = mainFileName,
                IsMain = true
            });

            if (request.AdditionalImages != null)
            {
                foreach (var photo in request.AdditionalImages)
                {
                    if (!photo.ContentType.Contains("image/"))
                    {
                        ModelState.AddModelError("AdditionalImages", "Shekil tipi olmalidir!");
                        ViewBag.Categories = await GetAllCategories();
                        return View(request);
                    }

                    if (photo.Length / 1024 > 2048)
                    {
                        ModelState.AddModelError("AdditionalImages", "Shekilin olcusu max 2MB ola biler!");
                        ViewBag.Categories = await GetAllCategories();
                        return View(request);
                    }

                    string additionalFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    string additionalPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", additionalFileName);

                    using (FileStream stream = new(additionalPath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    productImages.Add(new ProductImage
                    {
                        Image = additionalFileName,
                        IsMain = false
                    });
                }
            }

            Product newProduct = new()
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                ProductImages = productImages
            };

            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.Categories = await GetAllCategories();

            UpdateProductVM updateProductVM = new()
            {
               
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                MainPhoto = product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.Image,
                AdditionalPhotos = product.ProductImages.Where(pi => !pi.IsMain).Select(pi => pi.Image).ToList()
            };

            return View(updateProductVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM request)
        {
            if (id == null) return BadRequest();

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            if (request.MainImage != null)
            {
                if (!request.MainImage.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("MainImage", "Shekil tipi olmalidir!");
                    request.MainPhoto = product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.Image;
                    request.AdditionalPhotos = product.ProductImages.Where(pi => !pi.IsMain).Select(pi => pi.Image).ToList();
                    return View(request);
                }

                if (request.MainImage.Length / 1024 > 2048)
                {
                    ModelState.AddModelError("MainImage", "Shekilin olcusu max 2MB ola bilər!");
                    request.MainPhoto = product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.Image;
                    request.AdditionalPhotos = product.ProductImages.Where(pi => !pi.IsMain).Select(pi => pi.Image).ToList();
                    return View(request);
                }

                var oldMain = product.ProductImages.FirstOrDefault(pi => pi.IsMain);
                if (oldMain != null)
                {
                    string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", oldMain.Image);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                string fileName = Guid.NewGuid().ToString() + "_" + request.MainImage.FileName;
                string newPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", fileName);

                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await request.MainImage.CopyToAsync(stream);
                }

                if (oldMain != null)
                    oldMain.Image = fileName;
                else
                    product.ProductImages.Add(new ProductImage { Image = fileName, IsMain = true, Product = product });
            }

            if (request.AdditionalImages != null && request.AdditionalImages.Any())
            {
                foreach (var img in request.AdditionalImages)
                {
                    if (!img.ContentType.Contains("image/") || img.Length / 1024 > 2048)
                    {
                        ModelState.AddModelError("AdditionalImages", "Shekil tipi olmalidir və max 2MB olmalıdır!");
                        request.MainPhoto = product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.Image;
                        request.AdditionalPhotos = product.ProductImages.Where(pi => !pi.IsMain).Select(pi => pi.Image).ToList();
                        return View(request);
                    }
                }

                var oldAdditional = product.ProductImages.Where(pi => !pi.IsMain).ToList();
                foreach (var img in oldAdditional)
                {
                    string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", img.Image);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                _context.ProductImages.RemoveRange(oldAdditional);

                foreach (var img in request.AdditionalImages)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + img.FileName;
                    string newPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", fileName);

                    using (var stream = new FileStream(newPath, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }

                    product.ProductImages.Add(new ProductImage
                    {
                        Image = fileName,
                        IsMain = false,
                        Product = product
                    });
                }
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<SelectList> GetAllCategories()
        {
            var categories = await _context.Categories.ToListAsync();

            return new SelectList(categories, "Id", "Name");
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var detailVM = new DetailProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryName = product.Category?.Name,
                MainPhoto = product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.Image,
                AdditionalPhotos = product.ProductImages.Where(pi => !pi.IsMain).Select(pi => pi.Image).ToList()
            };

            return View(detailVM);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            foreach (var img in product.ProductImages)
            {
                string filePath = _webHostEnvironment.WebRootPath.GetFilePath("img", img.Image);
                filePath.DeleteFile();
            }
            _context.ProductImages.RemoveRange(product.ProductImages);
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
