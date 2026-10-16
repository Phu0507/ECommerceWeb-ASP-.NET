using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_web.Models;
using Shopping_web.Repositories;

namespace Shopping_web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin,Publisher,Auhtor")]
	[Route("Admin/Coupon")]
	public class CouponController : Controller
	{
		private readonly DataContext _dataContext;
		public CouponController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IActionResult> Index()
		{
            var coupons = await _dataContext.Coupons.ToListAsync();
            ViewBag.Coupons = coupons;
            return View();
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(CouponModel coupon)
        {
            if (coupon.DiscountType == 1)
            {
                if (coupon.DiscountPercent <= 0)
                {
                    ModelState.AddModelError("DiscountPercent", "Phải nhập % giảm giá");
                }
                coupon.DiscountAmount = 0;
            }
            else if (coupon.DiscountType == 2)
            {
                if (coupon.DiscountAmount <= 0)
                {
                    ModelState.AddModelError("DiscountAmount", "Phải nhập số tiền giảm");
                }
                coupon.DiscountPercent = 0;
            }

            if (ModelState.IsValid)
            {
                _dataContext.Coupons.Add(coupon);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Thêm couponthành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu nhập vào";
            }
            return View(coupon);
        }
    }
}
