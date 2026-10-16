using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_web.Models
{
	public class RatingModel
	{
		[Key]
		public int Id { get; set; }
		public long ProductId { get; set; }
		[Required(ErrorMessage = "Yeu cau nhap danh gia")]
		public string Comment { get; set; }
		[Required(ErrorMessage = "Yeu cau nhap ten")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yeu cau nhap Email")]
		public string Email { get; set; }
		public string Star { get; set; }
		[ForeignKey("ProductId")]
		public ProductModel Product { get; set; }

	}
}
