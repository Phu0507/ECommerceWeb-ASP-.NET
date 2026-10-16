using Shopping_web.Areas.Admin.Repository;
using Microsoft.AspNetCore.Mvc;
using Shopping_web.Models;
using Shopping_web.Repositories;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Shopping_web.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _context;
		private readonly IEmailSender _emailSender;
        public CheckoutController(DataContext context, IEmailSender emailSender)
        {
			_context = context;
            _emailSender = emailSender;
        }
		[HttpPost]
		public async Task<IActionResult> Checkout(string paymentMethod)
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
				return RedirectToAction("Login", "Account");

			var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new();

			if (!cartItems.Any())
			{
				TempData["error"] = "Giỏ hàng trống";
				return RedirectToAction("Index", "Cart");
			}

			var shippingPriceCookie = Request.Cookies["ShippingPrice"];
			var shippingAddress = Request.Cookies["ShippingAddress"];

			if (string.IsNullOrEmpty(shippingPriceCookie) || string.IsNullOrEmpty(shippingAddress))
			{
				TempData["error"] = "Vui lòng nhập địa chỉ và tính phí vận chuyển";
				return RedirectToAction("Index", "Cart");
			}

			decimal shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);

			if (shippingPrice <= 0)
			{
				TempData["error"] = "Phí vận chuyển không hợp lệ";
				return RedirectToAction("Index", "Cart");
			}

			// 🔥 1. Tính SubTotal
			decimal subTotal = cartItems.Sum(x => x.Price * x.Quantity);

			// 🔥 2. Tính Discount
			decimal discount = 0;
			string couponCode = Request.Cookies["CouponValue"];

			if (!string.IsNullOrEmpty(couponCode))
			{
				var coupon = await _context.Coupons
					.FirstOrDefaultAsync(x =>
						x.Name == couponCode &&
						x.Status == 1 &&
						x.Quantity > 0 &&
						x.DateStart <= DateTime.Now &&
						x.DateExpired >= DateTime.Now);

				if (coupon != null)
				{
					if (coupon.DiscountType == 1) // giảm %
					{
						discount = subTotal * coupon.DiscountPercent / 100;
					}
					else if (coupon.DiscountType == 2) // giảm tiền
					{
						discount = coupon.DiscountAmount;
					}

					// Không cho giảm quá tiền hàng
					if (discount > subTotal)
						discount = subTotal;

					// Trừ số lượng coupon
					coupon.Quantity -= 1;
					_context.Update(coupon);
				}
			}

			// 🔥 3. Tính Total
			decimal totalAmount = subTotal - discount + shippingPrice;

			var orderCode = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper();

			var orderItem = new OrderModel
			{
				OrderCode = orderCode,
				Username = userEmail,
				SubTotal = subTotal,
				ShippingCost = shippingPrice,
				DiscountAmount = discount,
				TotalAmount = totalAmount,
				Address = shippingAddress,
				CouponCode = couponCode,
				CreatedDate = DateTime.Now,
				Status = 1
			};

			_context.Orders.Add(orderItem);
			await _context.SaveChangesAsync();

			// 🔥 4. Lưu OrderDetails + trừ kho
			foreach (var item in cartItems)
			{
				var product = await _context.Products
					.FirstOrDefaultAsync(p => p.Id == item.ProductId);

				if (product == null || product.Quantity < item.Quantity)
				{
					TempData["error"] = "Sản phẩm không đủ số lượng";
					return RedirectToAction("Index", "Cart");
				}

				product.Quantity -= item.Quantity;
				product.Sold += item.Quantity;

				var orderDetail = new OrderDetails
				{
					Username = userEmail,
					OrderCode = orderCode,
					ProductId = item.ProductId,
					Quantity = item.Quantity,
					Price = item.Price
				};

				_context.Products.Update(product);
				_context.OrderDetails.Add(orderDetail);
			}

			await _context.SaveChangesAsync();

			// Nếu thanh toán VNPay
			if (paymentMethod == "vnpay")
			{
				var paymentUrl = CreateVNPayUrl(orderItem);
				return Redirect(paymentUrl);
			}

			// Nếu COD
			// Xóa session + cookie
			HttpContext.Session.Remove("Cart");
			Response.Cookies.Delete("ShippingAddress");
			Response.Cookies.Delete("ShippingPrice");
			Response.Cookies.Delete("CouponValue");

			// Gửi mail
			await _emailSender.SendEmailAsync(
				userEmail,
				"Đặt hàng thành công",
				$"Đơn hàng của bạn đã được đặt thành công! Mã đơn hàng: {orderCode}"
			);

			TempData["Success"] = "Đặt hàng thành công";
			return RedirectToAction("History", "Account");


		}

		private string CreateVNPayUrl(OrderModel order)
		{
			string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
			string vnp_Returnurl = Url.Action("VNPayReturn", "Checkout", null, Request.Scheme);

			string vnp_TmnCode = "HEIH5HO1";
			string vnp_HashSecret = "PM0YI86V14D5OV26WU2GRX60M3I2D6MS";

			var vnp_Params = new SortedDictionary<string, string>
	{
		{ "vnp_Version", "2.1.0" },
		{ "vnp_Command", "pay" },
		{ "vnp_TmnCode", vnp_TmnCode },
		{ "vnp_Amount", ((long)(order.TotalAmount * 100)).ToString() },
		{ "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
		{ "vnp_CurrCode", "VND" },
		{ "vnp_IpAddr", "127.0.0.1" },
		{ "vnp_Locale", "vn" },
		{ "vnp_OrderInfo", "Thanh toan don hang " + order.OrderCode },
		{ "vnp_OrderType", "other" },
		{ "vnp_ReturnUrl", vnp_Returnurl },
		{ "vnp_TxnRef", order.OrderCode }
	};

			var query = new StringBuilder();

			foreach (var item in vnp_Params)
			{
				if (!string.IsNullOrEmpty(item.Value))
				{
					query.Append(WebUtility.UrlEncode(item.Key) + "="
							   + WebUtility.UrlEncode(item.Value) + "&");
				}
			}

			string queryString = query.ToString().TrimEnd('&');

			string secureHash;
			using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret)))
			{
				var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
				secureHash = BitConverter.ToString(hash)
										.Replace("-", "")
										.ToLower();
			}

			return vnp_Url + "?" + queryString + "&vnp_SecureHash=" + secureHash;
		}


		public async Task<IActionResult> VNPayReturn()
		{
			var vnp_ResponseCode = Request.Query["vnp_ResponseCode"];
			var orderCode = Request.Query["vnp_TxnRef"];

			var order = await _context.Orders
				.FirstOrDefaultAsync(x => x.OrderCode == orderCode);

			if (order == null)
				return RedirectToAction("Index", "Home");

			if (vnp_ResponseCode == "00")
			{
				order.Status = 2; // đã thanh toán
				TempData["Success"] = "Thanh toán VNPay thành công!";
			}
			else
			{
				order.Status = 0; // thất bại
				TempData["Error"] = "Thanh toán thất bại!";
			}

			_context.Update(order);
			await _context.SaveChangesAsync();

			return RedirectToAction("History", "Account");
		}


		public IActionResult Index()
		{
			return View();
		}
	}
}
