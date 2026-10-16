using Microsoft.AspNetCore.Mvc;
using Shopping_web.Repositories;
using Microsoft.EntityFrameworkCore;
using Shopping_web.Models.ViewModel;
using Shopping_web.Models;

namespace Shopping_web.Controllers
{
	public class ProductController : Controller
	{
		private readonly DataContext _dataContext;
		public ProductController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Details(long Id)
		{
			if (Id == null) return RedirectToAction("Index");
			var productById = _dataContext.Products.Include(r => r.Ratings).Where(p => p.Id == Id).FirstOrDefault();
			//related products
			var relatedProducts = await _dataContext.Products
				.Where(p => p.CategoryId == productById.CategoryId && p.Id != Id)
				.Take(4)
				.ToListAsync();
			ViewBag.RelatedProducts = relatedProducts;
			var viewModel = new ProductDetailsViewModel
			{
				ProductDetails = productById,
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Search(string searchItem)
		{
			var searchResults = await _dataContext.Products
				.Where(p => p.Name.Contains(searchItem) || p.Description.Contains(searchItem))
				.ToListAsync();
			ViewBag.KeyWord = searchItem;
			return View(searchResults);
		}

		[HttpPost]
		public async Task<IActionResult> CommentProduct(RatingModel rating)
		{
			if (ModelState.IsValid)
			{
				var newRating = new RatingModel
				{
					ProductId = rating.ProductId,
					Name = rating.Name,
					Email = rating.Email,
					Comment = rating.Comment,
					Star = rating.Star,
				};
				_dataContext.Ratings.Add(newRating);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
				return Redirect(Request.Headers["Referer"]);
			}
			else
			{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				TempData["error"] = string.Join("<br />", errors);

				return RedirectToAction("Details", new { Id = rating.ProductId });

			}
			return Redirect(Request.Headers["Referer"]);
		}
	}
}
