using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_web.Models;
using Shopping_web.Repositories;

namespace Shopping_web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin,Author")]
	public class ContactController : Controller
	{
		public readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ContactController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
			_dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
		{
			var contacts = _dataContext.Contacts.ToList();
			return View(contacts);

        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _dataContext.Contacts.FirstOrDefaultAsync();
            return View(contact);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( ContactModel contact)
        {

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu";
                return View(contact);
            }

            // 🔹 Lấy sản phẩm cũ
            var existingContact = await _dataContext.Contacts.FirstOrDefaultAsync();
            if (existingContact == null)
                return NotFound();

            // 🔹 Update dữ liệu
            existingContact.Name = contact.Name;
            existingContact.Description = contact.Description;
            existingContact.Map = contact.Map;
            existingContact.Email = contact.Email;
            existingContact.Phone = contact.Phone;

            // 🔹 Upload ảnh mới (nếu có)
            if (contact.ImageUpload != null)
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/logo");
                string imageName = Guid.NewGuid() + "_" + contact.ImageUpload.FileName;
                string filePath = Path.Combine(uploadsDir, imageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await contact.ImageUpload.CopyToAsync(stream);
                }

                existingContact.LogoImg = imageName;
            }
            // ❗ Không upload ảnh → giữ ảnh cũ

            await _dataContext.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin liên hệ thành công";
            return RedirectToAction("Index");
        }
    }
}
