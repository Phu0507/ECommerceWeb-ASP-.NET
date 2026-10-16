using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_web.Models;
using Shopping_web.Repositories;
using System.Diagnostics;

namespace Shopping_web.Controllers;

public class HomeController : Controller
{
    private readonly DataContext _dataContext;
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUserModel> _userManager;
	public HomeController(ILogger<HomeController> logger, DataContext dataContext, UserManager<AppUserModel> userManager)
	{
        _logger = logger;
		_dataContext = dataContext;
		_userManager = userManager;
	}

    public IActionResult Index()
    {
        var products = _dataContext.Products.Include("Category").Include("Brand").ToList();
        var sliders = _dataContext.Sliders.Where(s => s.Status==1).ToList();
		ViewBag.Sliders = sliders;
		return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int statusCode)
	{
        if (statusCode == 404)
		{
			return View("NotFound");
		}
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

	public async Task<IActionResult> Contact()
	{
        var contact = await _dataContext.Contacts.FirstOrDefaultAsync();
		return View(contact);
	}
	[HttpPost]
	public async Task<IActionResult> AddWishList(long id)
	{
		var user = await _userManager.GetUserAsync(User);
		var wishlistProduct = new WishlistModel
		{
			ProductId = id,
			UserId = user.Id
		};
		_dataContext.Wishlists.Add(wishlistProduct);
		try
		{
			await _dataContext.SaveChangesAsync();
			return Ok(new { success = true, message = "Product added to wishlist successfully." });
		}
		catch (Exception)
		{
			TempData["Error"] = "An error occurred while adding the product to the wishlist.";
			return RedirectToAction("Index", "Home");
		}
	}

	[HttpPost]
	public async Task<IActionResult> AddCompare(long id)
	{
		var user = await _userManager.GetUserAsync(User);
		var compare = new CompareModel
		{
			ProductId = id,
			UserId = user.Id
		};
		_dataContext.Compares.Add(compare);
		try
		{
			await _dataContext.SaveChangesAsync();
			return Ok(new { success = true, message = "Compare successfully." });
		}
		catch (Exception)
		{
			TempData["Error"] = "An error occurred while Compare.";
			return RedirectToAction("Index", "Home");
		}
	}

	public async Task<IActionResult> Compare()
	{
		var compareItems = await (from c in _dataContext.Compares
								  join p in _dataContext.Products on c.ProductId equals p.Id
								  join u in _dataContext.Users on c.UserId equals u.Id
								  select new {User = u, Product = p, Compare = c }).ToListAsync();
		return View(compareItems);
	}

	public async Task<IActionResult> Wishlist()
	{
		var wishlist = await (from w in _dataContext.Wishlists
								  join p in _dataContext.Products on w.ProductId equals p.Id
								  join u in _dataContext.Users on w.UserId equals u.Id
								  select new { User = u, Product = p, Wishlist = w }).ToListAsync();
		return View(wishlist);
	}

    public async Task<IActionResult> DeleteCompare(int Id)
    {
        CompareModel compare = await _dataContext.Compares.FindAsync(Id);
        _dataContext.Compares.Remove(compare);
        await _dataContext.SaveChangesAsync();
        TempData["Success"] = "Xóa thành công";
        return RedirectToAction("Compare","Home");

    }

    public async Task<IActionResult> DeleteWishlist(int Id)
    {
        WishlistModel wishlish = await _dataContext.Wishlists.FindAsync(Id);
        _dataContext.Wishlists.Remove(wishlish);
        await _dataContext.SaveChangesAsync();
        TempData["Success"] = "Xóa thành công";
        return RedirectToAction("Wishlist","Home");

    }
}
