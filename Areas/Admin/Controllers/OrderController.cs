using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_web.Repositories;

namespace Shopping_web.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
	{
		public readonly DataContext _dataContext;
		public OrderController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IActionResult> Index()
		{
			return View(
				await _dataContext.Orders
					.OrderByDescending(p => p.Id)
					.ToListAsync()
			);
		}
        public async Task<IActionResult> ViewOrder(string orderCode)
        {
            var detailsOrder = await _dataContext.OrderDetails
                .Include(o => o.Product)
                .Where(o => o.OrderCode == orderCode)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
			var shippingCost = await _dataContext.Orders.Where(o => o.OrderCode == orderCode).FirstAsync();
			ViewBag.ShippingCost = shippingCost.ShippingCost;
			ViewBag.Status = shippingCost.Status;
            return View(detailsOrder);
        }
		[HttpPost]
		[Route("UpdateOrder")]
		public async Task<IActionResult> UpdateOrder(string orderCode, int status)
		{
			var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
			if (order == null)
			{
				return NotFound();
			}
			order.Status = status;
			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, message = "ok" });
			}
			catch (Exception ex) 
			{ 
				return StatusCode(500, new { success = false, message = ex.Message });
            }
		}
    }
}
