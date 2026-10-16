using Microsoft.AspNetCore.Mvc;
using Shopping_web.Models;
using Shopping_web.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shopping_web.Controllers
{
	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;
		public BrandController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IActionResult> Index(String Slug = "")
		{
			BrandModel brand = _dataContext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();
			if (brand == null) return RedirectToAction("Index");
			var productByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);

			return View(await productByBrand.OrderByDescending(p => p.Id).ToListAsync());
		}
	}
}
