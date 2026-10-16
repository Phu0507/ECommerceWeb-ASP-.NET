using System.ComponentModel.DataAnnotations;

namespace Shopping_web.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [MinLength(4, ErrorMessage = "Tên danh mục phải ít nhất 4 ký tự")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }
	}
}
