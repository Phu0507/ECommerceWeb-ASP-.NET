using Shopping_web.Repositories.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_web.Models
{
	public class SliderModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập tên slider")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập mô tả slider")]
		public string Description { get; set; }
		public int Status { get; set; }
		public string ImageUrl { get; set; }
		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }
	}
}
