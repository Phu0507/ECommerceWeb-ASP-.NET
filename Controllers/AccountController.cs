using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_web.Areas.Admin.Repository;
using Shopping_web.Models;
using Shopping_web.Models.ViewModel;
using Shopping_web.Repositories;
using System.Security.Claims;

namespace Shopping_web.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        public AccountController(UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager, DataContext dataContext, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
			_dataContext = dataContext;
            _emailSender = emailSender;
        }
		[HttpGet]
		public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
		}
        [HttpPost]
		public async Task<IActionResult> Login(LoginViewModel loginVM)
		{
			if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
				if (signInResult.Succeeded)
				{
					return Redirect(loginVM.ReturnUrl ?? "/");
				}
				ModelState.AddModelError("", "Đăng nhập không thành công");
			}
			return View(loginVM);
		}

		[HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel userModel)
        {
            if (!ModelState.IsValid)
                return View(userModel);

            var appUser = new AppUserModel
            {
                UserName = userModel.Username,
                Email = userModel.Email
            };

            IdentityResult result = await _userManager.CreateAsync(appUser, userModel.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(appUser, isPersistent: false);
                TempData["Success"] = "Đăng ký tài khoản thành công";
                return Redirect("/account/login");
			}

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(userModel);
        }

        public async Task<IActionResult> Logout(string returnUrl = "/")
		{
			await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
		}

        [HttpGet]
        public async Task<IActionResult> History()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            { 
                return RedirectToAction("Login", "Account");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var orders = await _dataContext.Orders
				.Where(o => o.Username == userEmail)
				.OrderByDescending(o => o.Id)
				.ToListAsync();
            ViewBag.UserEmail = userEmail;
			return View(orders);
		}

        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            try
            {
                var order = await _dataContext.Orders
                    .FirstOrDefaultAsync(o => o.OrderCode == ordercode);
                if (order == null)
                {
                    return NotFound("Không tìm thấy đơn hàng");
                }
                // Optional: chỉ cho hủy khi đang chờ xử lý
                if (order.Status != 1) // 1 = Pending
                {
                    TempData["Error"] = "Không thể hủy đơn hàng ở trạng thái này";
                    return RedirectToAction("History", "Account");
                }
                order.Status = 3; // 3 = Canceled
                await _dataContext.SaveChangesAsync();
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra khi hủy đơn hàng";
                return RedirectToAction("History", "Account");
            }

            return RedirectToAction("History", "Account");
        }

		public async Task<IActionResult> ForgotPassword()
		{
			return View();
		}

		[HttpGet]
		public IActionResult NewPassword(AppUserModel user, string token)
		{
			var checkUser = _userManager.Users.Where(u => u.Email == user.Email && u.Token == token).FirstOrDefaultAsync();
            if (checkUser != null)
            {
                ViewBag.Email = user.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["Error"] = "Email không hợp lệ";
                return RedirectToAction("ForgotPassword", "Account");
            }

            return View();
		}


		[HttpPost]
		public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
		{
			if (string.IsNullOrEmpty(user.Email))
			{
				TempData["error"] = "Vui lòng nhập email";
				return RedirectToAction("ForgotPassword", "Account");
			}

			var checkMail = await _userManager.FindByEmailAsync(user.Email);

			// Không tiết lộ email có tồn tại hay không (best practice)
			if (checkMail == null)
			{
				TempData["success"] = "Email không tồn tại";
				return RedirectToAction("ForgotPassword", "Account");
			}

			// Generate token chuẩn của Identity
			var token = await _userManager.GeneratePasswordResetTokenAsync(checkMail);

			// Encode token để dùng trong URL
			token = Uri.EscapeDataString(token);
            checkMail.Token = token;
            _dataContext.Update(checkMail);
            await _dataContext.SaveChangesAsync();

            var callbackUrl = Url.Action(
				"NewPassword",
				"Account",
				new { email = checkMail.Email, token = token },
				protocol: Request.Scheme
			);

			var subject = "Reset your password";
			var message = $@"
        <p>Bạn đã yêu cầu đặt lại mật khẩu.</p>
        <p>Click vào link bên dưới để đổi mật khẩu:</p>
        <p><a href='{callbackUrl}'>Đổi mật khẩu</a></p>
        <p>Link sẽ hết hạn sau một thời gian.</p>
    ";

			await _emailSender.SendEmailAsync(
				checkMail.Email,
				subject,
				message
			);

			TempData["success"] = "Vui lòng kiểm tra email để đặt lại mật khẩu";
			return RedirectToAction("ForgotPassword", "Account");
		}

        [HttpPost]
        public async Task<IActionResult> UpdateNewPassword(AppUserModel appUserModel, string token)
        {
            var user = await _userManager.Users.Where(u => u.Email == appUserModel.Email && u.Token == token).FirstOrDefaultAsync();

            if (user != null)
            {
                var resetPassResult = new PasswordHasher<AppUserModel>();
                var hashedNewPassword = resetPassResult.HashPassword(user, appUserModel.PasswordHash);
                user.PasswordHash = hashedNewPassword;
                user.Token = null; // Xóa token sau khi đổi mật khẩu
                await _userManager.UpdateAsync(user);
                TempData["Success"] = "Đổi mật khẩu thành công";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["Error"] = "Đổi mật khẩu thất bại";
                return RedirectToAction("ForgotPassword", "Account");
            }
            return View();  

        }

		public async Task<IActionResult> UpdateAccount()
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				return RedirectToAction("Login", "Account");
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Email == userEmail);
			return View(user);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateInfoAccount(AppUserModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("UpdateAccount", model);
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				TempData["Error"] = "Người dùng không tồn tại";
				return RedirectToAction("UpdateAccount");
			}
			user.PhoneNumber = model.PhoneNumber;
			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
			{
				TempData["Error"] = "Cập nhật thất bại";
				return RedirectToAction("UpdateAccount");
			}

			TempData["Success"] = "Cập nhật thông tin thành công";
			return RedirectToAction("UpdateAccount");
		}

	}
}
