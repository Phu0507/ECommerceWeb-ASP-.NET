using Microsoft.AspNetCore.Mvc;
using Shopping_web.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Shopping_web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Shopping_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Author")]
    [Route("Admin/User")]
    public class UserController : Controller
    {
        public readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
  
		public UserController(DataContext dataContext, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
            _dataContext = dataContext;

        }
        [Route("Index")]
        public async Task<IActionResult> Index()
		{
			var users = await (from u in _dataContext.Users
                               join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                               into userRoles
                               from ur in userRoles.DefaultIfEmpty()
                               join r in _dataContext.Roles on ur.RoleId equals r.Id into userRoleNames
                               from r in userRoleNames.DefaultIfEmpty()
                               select new
                               {
                                   User = u,
                                   RoleName = r.Name
                               }).ToListAsync();
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.LoggedInUserId = loggedInUserId;
            return View(users);

        }
		[HttpGet]
		[Route("Create")]
		public async Task<IActionResult> Create()
		{
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(new AppUserModel());
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
		{
			if (ModelState.IsValid)
			{
				var createUser = await _userManager.CreateAsync(user, user.PasswordHash);
                if (createUser.Succeeded)
				{
                    var newUser = await _userManager.FindByEmailAsync(user.Email);
                    var userId = newUser.Id;
                    var role = await _roleManager.FindByIdAsync(user.RoleId);
                    var addToRole = await _userManager.AddToRoleAsync(newUser, role.Name);
                    if (!addToRole.Succeeded)
                    {
                        TempData["Error"] = "Thêm user thất bại";
                    }
                    TempData["Success"] = "Thêm user thành công";
                    return RedirectToAction("Index","User");
                }
				else
				{
                    TempData["Error"] = "Thêm user thất bại";
                    return View(user);
                }

            }
			else
			{
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu nhập vào";
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(String Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string Id, AppUserModel user)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu";
                ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Id", "Name");
                return View(user);
            }

            var existingUser = await _userManager.FindByIdAsync(Id);
            if (existingUser == null)
                return NotFound();

            // 🔹 Update thông tin user
            existingUser.Email = user.Email;
            existingUser.UserName = user.UserName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.RoleId = user.RoleId;

            var updateUser = await _userManager.UpdateAsync(existingUser);
            if (!updateUser.Succeeded)
            {
                TempData["Error"] = "Cập nhật user thất bại";
                ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Id", "Name");
                return View(user);
            }

            // 🔹 XỬ LÝ ROLE
            var currentRoles = await _userManager.GetRolesAsync(existingUser);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
            }

            var newRole = await _roleManager.FindByIdAsync(user.RoleId);
            if (newRole != null)
            {
                await _userManager.AddToRoleAsync(existingUser, newRole.Name);
            }

            TempData["Success"] = "Cập nhật user thành công";
            return RedirectToAction("Index", "User");
        }
        public async Task<IActionResult> Delete(String Id)
        {
			if (string.IsNullOrEmpty(Id))
			{
				return NotFound();
            }
            var user = await _userManager.FindByIdAsync(Id);
			if (user == null)
			{
                return NotFound();
            }
            var deleteUser = await _userManager.DeleteAsync(user);
            if (deleteUser.Succeeded)
            {
                TempData["Success"] = "Xóa user thành công";
            }
            else
            {
                TempData["Error"] = "Xóa user thất bại";
            }
            return RedirectToAction("Index", "User");
        }

    }
}
