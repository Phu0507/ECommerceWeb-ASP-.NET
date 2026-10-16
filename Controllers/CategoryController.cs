using Microsoft.AspNetCore.Mvc;
using Shopping_web.Models;
using Shopping_web.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shopping_web.Controllers
{
    public class CategoryController : Controller
    {
		private readonly DataContext _dataContext;
		public CategoryController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
        public async Task<IActionResult> Index(
    string Slug,
    decimal? minPrice,
    decimal? maxPrice,
    string sort_by = ""
)
        {
            if (string.IsNullOrEmpty(Slug))
                return NotFound();

            var category = await _dataContext.Categories
                .FirstOrDefaultAsync(c => c.Slug == Slug);

            if (category == null)
                return NotFound();

            var products = _dataContext.Products
                .Where(p => p.CategoryId == category.Id);

            // ✅ LỌC GIÁ
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // ✅ SORT
            products = sort_by switch
            {
                "price_increase" => products.OrderBy(p => p.Price),
                "price_decrease" => products.OrderByDescending(p => p.Price),
                "price_newest" => products.OrderByDescending(p => p.Id),
                _ => products.OrderByDescending(p => p.Id)
            };

            ViewBag.Slug = Slug;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(await products.ToListAsync());
        }



    }
}
