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
	[Authorize(Roles = "Admin,Publisher")]
	[Route("Admin/Slider")]
	public class SliderController : Controller
	{
		public readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public SliderController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
			_dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }
		[Route("Index")]
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Sliders.OrderByDescending(s => s.Id).ToListAsync());
		}
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel slider)
        {
            if (ModelState.IsValid)
            {
                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fileStream);
                    fileStream.Close();
                    slider.ImageUrl = imageName;
                }
                _dataContext.Sliders.Add(slider);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Thêm slider thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu nhập vào";
            }
            return View(slider);
        }
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);
            return View(slider);
        }
        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, SliderModel slider)
        {
         
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu";
                return View(slider);
            }

            // 🔹 Lấy sản phẩm cũ
            var existingProduct = await _dataContext.Sliders.FindAsync(Id);
            if (existingProduct == null)
                return NotFound();

            // 🔹 Check trùng tên (loại trừ chính nó)
            bool isExist = await _dataContext.Sliders
                .AnyAsync(p => p.Name == slider.Name && p.Id != Id);

            if (isExist)
            {
                ModelState.AddModelError("", "Tên slider đã tồn tại");
                return View(slider);
            }

            // 🔹 Update dữ liệu
            existingProduct.Name = slider.Name;
            existingProduct.Description = slider.Description;

            // 🔹 Upload ảnh mới (nếu có)
            if (slider.ImageUpload != null)
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                string imageName = Guid.NewGuid() + "_" + slider.ImageUpload.FileName;
                string filePath = Path.Combine(uploadsDir, imageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await slider.ImageUpload.CopyToAsync(stream);
                }

                existingProduct.ImageUrl = imageName;
            }
            // ❗ Không upload ảnh → giữ ảnh cũ

            await _dataContext.SaveChangesAsync();

            TempData["Success"] = "Cập nhật slider thành công";
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);
            if (!string.IsNullOrEmpty(slider.ImageUrl))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                string oldImagePath = Path.Combine(uploadsDir, slider.ImageUrl);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            _dataContext.Sliders.Remove(slider);
            await _dataContext.SaveChangesAsync();
            TempData["Success"] = "Xóa slider thành công";
            return RedirectToAction("Index");

        }
    }
}
