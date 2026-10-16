using Shopping_web.Repositories.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_web.Models
{
    public class ProductModel
    {
		[Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [MinLength(4, ErrorMessage = "Tên sản phẩm phải ít nhất 4 ký tự")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        [MinLength(4, ErrorMessage = "Mô tả sản phẩm phải ít nhất 4 ký tự")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập giá cho sản phẩm")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public string Slug { get; set; }
		public string ImageUrl { get; set; }
        [Required(ErrorMessage = "Chọn 1 Brand")]
        public int BrandId { get; set; }
        [Required(ErrorMessage = "Chọn 1 Category")]
        public int CategoryId { get; set; }
		public CategoryModel Category { get; set; }
		public BrandModel Brand { get; set; }
        public RatingModel Ratings { get; set; }
		[NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
