using Fiorello.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fiorello.Controllers
{
    public class BasketController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Add(int id)
        {
            List<BasketVM> basketVMs;

            if (HttpContext.Request.Cookies["basket"] != null)
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(
                    HttpContext.Request.Cookies["basket"]
                );
            }
            else
            {
                basketVMs = new();
            }

            BasketVM existBasket = basketVMs.FirstOrDefault(x => x.Id == id);

            if (existBasket != null)
            {
                existBasket.Count++;
            }
            else
            {
                basketVMs.Add(new BasketVM
                {
                    Id = id,
                    Count = 1
                });
            }

            HttpContext.Response.Cookies.Append(
                "basket",
                JsonConvert.SerializeObject(basketVMs)
            );

            return RedirectToAction("Index", "Home");
        }

    }
}
