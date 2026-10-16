using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_web.Models;
using Shopping_web.Repositories;

namespace Shopping_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Publisher,Auhtor")]
	[Route("Admin/Shipping")]
    public class ShippingController : Controller
	{
		public readonly DataContext _dataContext;
        public ShippingController(DataContext dataContext)
		{
            _dataContext = dataContext;
        }
		[Route("Index")]
        public async Task<IActionResult> Index()
		{
            var shippings = await _dataContext.Shippings.ToListAsync();
            ViewBag.Shippings = shippings;
            return View();
		}
		[HttpPost]
        [Route("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string tinh, string quan, string address, decimal price)
        {
            shippingModel.City = tinh;
            shippingModel.Ward = quan;
            shippingModel.Address = address;
            shippingModel.Price = price;
            try
            {
                var existingShipping = await _dataContext.Shippings
                    .AnyAsync(s => s.Address == shippingModel.Address
                        && s.Ward == shippingModel.Ward
                        && s.City == shippingModel.City);
                if (existingShipping)
                {
                    return Ok(new { duplicate = true, message = "Shipping entry already exists." });
                }
                _dataContext.Shippings.Add(shippingModel);
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Shipping entry added successfully." });
            }
            catch (Exception ex) {
                return Ok(new { success = false, message = "Error checking existing shipping entry: " + ex.Message });
            }

        }

        public async Task<IActionResult> Delete(int Id)
        {
            ShippingModel shipping = await _dataContext.Shippings.FindAsync(Id);
            _dataContext.Shippings.Remove(shipping);
            await _dataContext.SaveChangesAsync();
            TempData["Success"] = "Xóa thành công";
            return RedirectToAction("Index");

        }
    }
}
