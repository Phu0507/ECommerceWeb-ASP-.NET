using Microsoft.AspNetCore.Mvc;
using Shopping_web.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Shopping_web.Models;
using Microsoft.AspNetCore.Authorization;

namespace Shopping_web.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = "Admin,Publisher,Auhtor")]

    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<IActionResult> Index()
        {
            return View(
                await _dataContext.Brands
                    .OrderByDescending(p => p.Id)
                    .ToListAsync()
            );
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.ToLower().Replace(" ", "-");
                var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tên thương hiệu đã tồn tại");
                    return View(brand);
                }
                _dataContext.Brands.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Thêm thương hiệu thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu nhập vào";
            }
            return View(brand);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            return View(brand);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, BrandModel brand)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu";
                return View(brand);
            }

            // 🔹 Lấy brand cũ
            var existingBrand = await _dataContext.Brands.FindAsync(Id);
            if (existingBrand == null)
                return NotFound();

            // 🔹 Tạo slug mới
            string slug = brand.Name.ToLower().Replace(" ", "-");

            // 🔹 Check trùng slug (loại trừ chính nó)
            bool isSlugExist = await _dataContext.Brands
				.AnyAsync(c => c.Slug == slug && c.Id != Id);

            if (isSlugExist)
            {
                ModelState.AddModelError("", "Tên thương hiệu đã tồn tại");
                return View(brand);
            }

            // 🔹 Update dữ liệu
            existingBrand.Name = brand.Name;
            existingBrand.Description = brand.Description;
            existingBrand.Slug = slug;
            existingBrand.Status = brand.Status;

            await _dataContext.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thương hiệu thành công";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            _dataContext.Brands.Remove(brand);
            await _dataContext.SaveChangesAsync();
            TempData["Success"] = "Xóa thương hiệu thành công";
            return RedirectToAction("Index");

        }
    }
}
