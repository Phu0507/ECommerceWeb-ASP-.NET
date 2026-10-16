using Microsoft.AspNetCore.Mvc;
using Shopping_web.Models;
using Shopping_web.Services.Momo;

namespace Shopping_web.Controllers
{
	public class PaymentController : Controller
	{
		private IMomoService _momoService;

		public PaymentController(IMomoService momoService)
		{
			_momoService = momoService;
		}

		[HttpPost]
		public async Task<IActionResult> CreatePaymentMoMo(OrderInfoModel model)
		{
			var response = await _momoService.CreatePaymentAsync(model);
			if (response == null)
			{
				return Content("Response null");
			}

			if (string.IsNullOrEmpty(response.PayUrl))
			{
				return Content($"ErrorCode: {response.ErrorCode} - Message: {response.Message}");
			}
			return Redirect(response.PayUrl);
		}

		[HttpGet]
		public IActionResult PaymentCallBack()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			return View(response);
		}

		public IActionResult Index()
		{
			return View();
		}

	}
}
