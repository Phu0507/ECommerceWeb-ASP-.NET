using System.ComponentModel.DataAnnotations;

namespace Shopping_web.Models.ViewModel
{
	public class LoginViewModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
		public string Username { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public string ReturnUrl { get; set; }
	}
}
