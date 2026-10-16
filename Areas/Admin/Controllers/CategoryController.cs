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
    public class CategoryController : Controller
	{
		private readonly DataContext _dataContext;
		public CategoryController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<CategoryModel> category = _dataContext.Categories.ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = category.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-");
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tên sản phẩm đã tồn tại");
                    return View(category);
                }
                _dataContext.Categories.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu nhập vào";
            }
            return View(category);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, CategoryModel category)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu";
                return View(category);
            }

            // 🔹 Lấy category cũ
            var existingCategory = await _dataContext.Categories.FindAsync(Id);
            if (existingCategory == null)
                return NotFound();

            // 🔹 Tạo slug mới
            string slug = category.Name.ToLower().Replace(" ", "-");

            // 🔹 Check trùng slug (loại trừ chính nó)
            bool isSlugExist = await _dataContext.Categories
                .AnyAsync(c => c.Slug == slug && c.Id != Id);

            if (isSlugExist)
            {
                ModelState.AddModelError("", "Tên danh mục đã tồn tại");
                return View(category);
            }

            // 🔹 Update dữ liệu
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Slug = slug;
            existingCategory.Status = category.Status;

            await _dataContext.SaveChangesAsync();

            TempData["Success"] = "Cập nhật danh mục thành công";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["Success"] = "Xóa danh mục thành công";
            return RedirectToAction("Index");

        }
    }
}
