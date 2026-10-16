using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_web.Models;
using Shopping_web.Repositories;

namespace Shopping_web.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = "Admin,Author")]
    public class ProductController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
			_dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
        }
		public async Task<IActionResult> Index()
		{
			return View(
				await _dataContext.Products
					.OrderByDescending(p => p.Id)
					.Include(p => p.Category)
                    .Include(p => p.Brand)
                    .ToListAsync()
			);
		}
        [HttpGet]
        public IActionResult Create()
		{
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
			return View();
        }
        [HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
			if (ModelState.IsValid)
			{
				product.Slug = product.Name.ToLower().Replace(" ", "-");
				var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Tên sản phẩm đã tồn tại");
					return View(product);
				}
				if (product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fileStream);
                    fileStream.Close();
                    product.ImageUrl = imageName;
                }
                _dataContext.Products.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Thêm sản phẩm thành công";
				return RedirectToAction("Index");
            }
			else
			{
				TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu nhập vào";
			}
            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(long Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long Id, ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng kiểm tra lại dữ liệu";
                return View(product);
            }

            // 🔹 Lấy sản phẩm cũ
            var existingProduct = await _dataContext.Products.FindAsync(Id);
            if (existingProduct == null)
                return NotFound();

            // 🔹 Tạo slug mới
            string slug = product.Name.ToLower().Replace(" ", "-");

            // 🔹 Check trùng tên (loại trừ chính nó)
            bool isSlugExist = await _dataContext.Products
                .AnyAsync(p => p.Slug == slug && p.Id != Id);

            if (isSlugExist)
            {
                ModelState.AddModelError("", "Tên sản phẩm đã tồn tại");
                return View(product);
            }

            // 🔹 Update dữ liệu
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BrandId = product.BrandId;
            existingProduct.Slug = slug;

            // 🔹 Upload ảnh mới (nếu có)
            if (product.ImageUpload != null)
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string imageName = Guid.NewGuid() + "_" + product.ImageUpload.FileName;
                string filePath = Path.Combine(uploadsDir, imageName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageUpload.CopyToAsync(stream);
                }

                existingProduct.ImageUrl = imageName;
            }
            // ❗ Không upload ảnh → giữ ảnh cũ

            await _dataContext.SaveChangesAsync();

            TempData["Success"] = "Cập nhật sản phẩm thành công";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string oldImagePath = Path.Combine(uploadsDir, product.ImageUrl);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["Success"] = "Xóa sản phẩm thành công";
            return RedirectToAction("Index");

        }

        //add quantity
        [HttpGet]
        public async Task<IActionResult> AddQuantity(int Id)
        {
            var productQuantity = await _dataContext.ProductQuantities
                .Where(pq => pq.ProductId == Id)
                .ToListAsync();
            ViewBag.ProductQuantities = productQuantity;
            ViewBag.Id = Id;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreProductQuantity(ProductQuantityModel productQuantityModel)
        {
            var product = await _dataContext.Products.FindAsync(productQuantityModel.ProductId);
            if(product== null)
            {
                return NotFound();
            }
            product.Quantity += productQuantityModel.Quantity;
            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateCreated = DateTime.Now;
            _dataContext.ProductQuantities.Add(productQuantityModel);
            await _dataContext.SaveChangesAsync();
            TempData["Success"] = "Thêm số lượng sản phẩm thành công";
            return RedirectToAction("AddQuantity","Product", new { id = productQuantityModel.ProductId });
        }
        
    }
}
