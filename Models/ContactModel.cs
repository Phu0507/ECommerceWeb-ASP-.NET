using Shopping_web.Repositories.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_web.Models
{
	public class ContactModel
	{
		[Key]
		[Required(ErrorMessage = "Vui lòng nhập tiêu đề web")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập thông tin liên hệ")]
		public string Description { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập ban do")]
		public string Map { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập sdt")]
		public string Phone { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập Email")]
		public string Email { get; set; }
		public string LogoImg { get; set; }
		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }
	}
}
