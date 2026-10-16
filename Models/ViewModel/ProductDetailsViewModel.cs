using System.ComponentModel.DataAnnotations;

namespace Shopping_web.Models.ViewModel
{
	public class ProductDetailsViewModel
	{
		public ProductModel ProductDetails { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập đánh giá")]
		public string Comment { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập tên")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập Email")]
		public string Email { get; set; }
	}
}
