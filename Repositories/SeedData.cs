using Microsoft.EntityFrameworkCore;
using Shopping_web.Models;

namespace Shopping_web.Repositories
{
	public class SeedData
	{
		public static void SeedingData(DataContext _context) 
		{
			_context.Database.Migrate();
			if (!_context.Products.Any())
			{
				CategoryModel macbook = new CategoryModel() { Name = "Macbook", Slug = "macbook", Description = "Danh mục Macbook", Status = 1 };
				CategoryModel pc = new CategoryModel() { Name = "PC", Slug = "pc", Description = "Danh mục PC", Status = 1 };
				BrandModel apple = new BrandModel() { Name = "Apple", Slug = "apple", Description = "Danh mục sản phẩm Apple", Status = 1 };
				BrandModel samsung = new BrandModel() { Name = "SamSung", Slug = "samsung", Description = "Danh mục sản phẩm SamSung", Status = 1 };

				_context.Products.AddRange(
					new ProductModel() { Name = "Macbook Pro M1", Description = "Macbook Pro M1 2020 13 inch", Price = 29990000, Slug = "macbook-pro-m1", ImageUrl = "https://example.com/macbookpro.jpg", Brand = apple, Category = macbook },
					new ProductModel() { Name = "Macbook Air M2", Description = "Macbook Air M2 2022 13 inch", Price = 31990000, Slug = "macbook-air-m2", ImageUrl = "https://example.com/macbookair.jpg", Brand = apple, Category = macbook },
					new ProductModel() { Name = "Samsung Galaxy Book", Description = "Samsung Galaxy Book 15 inch", Price = 19990000, Slug = "samsung-galaxy-book", ImageUrl = "https://example.com/galaxybook.jpg", Brand = samsung, Category = pc },
					new ProductModel() { Name = "Samsung Notebook 9", Description = "Samsung Notebook 9 13 inch", Price = 17990000, Slug = "samsung-notebook-9", ImageUrl = "https://example.com/notebook9.jpg", Brand = samsung, Category = pc }
				);
				_context.SaveChanges();

			}
		}
	}
}
