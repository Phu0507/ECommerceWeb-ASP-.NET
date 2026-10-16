using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_web.Models;
using Shopping_web.Models.ViewModel;
using Shopping_web.Repositories;

namespace Shopping_web.Controllers
{
	public class CartController : Controller
	{
		private readonly DataContext _dataContext;
		public CartController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IActionResult> Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new();

			decimal subTotal = cartItems.Sum(item => item.Price * item.Quantity);

			decimal shippingPrice = 0;
			var shippingPriceCookie = Request.Cookies["ShippingPrice"];
			if (shippingPriceCookie != null)
			{
				shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
			}

			decimal discount = 0;
			var couponCode = Request.Cookies["CouponValue"];

			if (!string.IsNullOrEmpty(couponCode))
			{
				var coupon = await _dataContext.Coupons
					.FirstOrDefaultAsync(x =>
						x.Name == couponCode &&
						x.Status == 1 &&
						x.Quantity > 0 &&
						x.DateExpired >= DateTime.Now);

				if (coupon != null)
				{
					if (coupon.DiscountType == 1)
						discount = subTotal * coupon.DiscountPercent / 100;
					else if (coupon.DiscountType == 2)
						discount = coupon.DiscountAmount;

					if (discount > subTotal)
						discount = subTotal;
				}
			}

			decimal totalAmount = subTotal - discount + shippingPrice;

			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = subTotal,
				ShippingCost = shippingPrice,
				DiscountAmount = discount,
				TotalAmount = totalAmount,
				CouponCode = couponCode,
				ShippingAddress = Request.Cookies["ShippingAddress"]
			};

			return View(cartVM);
		}

		[HttpPost]
		public async Task<IActionResult> Add(long id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItem = cartItems.FirstOrDefault(item => item.ProductId == id);
			if (cartItem != null)
			{
				cartItem.Quantity++;
			}
			else
			{
				cartItems.Add(new CartItemModel(product));
			}
			HttpContext.Session.SetJson("Cart", cartItems);
			TempData["Success"] = "Product added to cart successfully!";
			return RedirectToAction("Index");
		}

		public IActionResult Decrease(long id)
		{
			var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

			var cartItem = cartItems.FirstOrDefault(p => p.ProductId == id);

			if (cartItem != null)
			{
				if (cartItem.Quantity > 1)
					cartItem.Quantity--;
				else
					cartItems.Remove(cartItem);
			}

			if (cartItems.Count == 0)
				HttpContext.Session.Remove("Cart");
			else
				HttpContext.Session.SetJson("Cart", cartItems);

			return RedirectToAction("Index");
		}


		public async Task<IActionResult> Increase(long id)
		{
			ProductModel productModel = await _dataContext.Products.Where(p => p.Id == id).FirstOrDefaultAsync();
			var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

			if (cartItems == null)
				return RedirectToAction("Index");

			var cartItem = cartItems.FirstOrDefault(p => p.ProductId == id);

			if (cartItem != null && productModel.Quantity > cartItem.Quantity)
			{
				cartItem.Quantity++;
			}
			else
			{
				TempData["Error"] = "Not enough stock available!";
			}

			HttpContext.Session.SetJson("Cart", cartItems);
			return RedirectToAction("Index");
		}

		public IActionResult Remove(long id)
		{
			var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

			if (cart == null)
				return RedirectToAction("Index");

			cart.RemoveAll(p => p.ProductId == id);

			if (cart.Count == 0)
				HttpContext.Session.Remove("Cart");
			else
				HttpContext.Session.SetJson("Cart", cart);
			TempData["Success"] = "Product removed from cart successfully!";
			return RedirectToAction("Index");
		}
		[HttpPost]
		public async Task<IActionResult> GetShipping(
	string tinh,
	string quan,
	string address
)
		{
			// Tính phí ship
			var existingShipping = await _dataContext.Shippings
				.FirstOrDefaultAsync(s => s.City == tinh && s.Ward == quan);

			decimal shippingCost = existingShipping != null ? existingShipping.Price : 50000;
			var shippingPriceJson = JsonConvert.SerializeObject(shippingCost);

			// GỘP FULL ADDRESS
			var parts = new List<string>();

			if (!string.IsNullOrWhiteSpace(address))
				parts.Add(address);

			if (!string.IsNullOrWhiteSpace(quan))
				parts.Add(quan);

			if (!string.IsNullOrWhiteSpace(tinh))
				parts.Add(tinh);

			var fullAddress = string.Join(", ", parts);


			try
			{
				var cookiesOptions = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTimeOffset.UtcNow.AddMinutes(30),
					Secure = true,
				};

				// Cookie phí ship
				Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookiesOptions);

				// Cookie địa chỉ giao hàng
				Response.Cookies.Append("ShippingAddress", fullAddress, cookiesOptions);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error setting cookie: {ex.Message}");
			}

			return Json(new
			{
				success = true,
				shippingCost,
				address = fullAddress
			});
		}

		[HttpPost]
		public IActionResult DeleteShipping()
		{
			Response.Cookies.Delete("ShippingPrice");
			return Json(new { success = true });
		}


		[HttpPost]
        public async Task<IActionResult> GetCoupon(string coupon_value)
        {
            if (string.IsNullOrWhiteSpace(coupon_value))
            {
                return Ok(new { success = false, message = "Vui lòng nhập mã coupon" });
            }

            var validCoupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(c => c.Name == coupon_value && c.Quantity >= 1);

            if (validCoupon == null)
            {
                return Ok(new { success = false, message = "Coupon không tồn tại" });
            }

            TimeSpan remaining = validCoupon.DateExpired - DateTime.Now;
            if (remaining.Days <= 0)
            {
                return Ok(new { success = false, message = "Coupon đã hết hạn" });
            }

            //string discountText = "";

            //if (validCoupon.DiscountType == 1)
            //{
            //    discountText = $"giảm {validCoupon.DiscountPercent}%";
            //}
            //else if (validCoupon.DiscountType == 2)
            //{
            //    discountText = $"giảm {validCoupon.DiscountAmount.ToString("#,##0")}đ";
            //}

            //string couponTitle = $"{validCoupon.Name} {discountText}";

            try
            {
                var cookiesOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = false,
                    SameSite = SameSiteMode.Lax
                };

                Response.Cookies.Append("CouponValue", validCoupon.Name, cookiesOptions);

                return Ok(new
                {
                    success = true,
                    message = "Áp dụng coupon thành công",
					validCoupon.Name
				});
            }
            catch
            {
                return Ok(new { success = false, message = "Áp dụng coupon thất bại" });
            }
        }


    }
}