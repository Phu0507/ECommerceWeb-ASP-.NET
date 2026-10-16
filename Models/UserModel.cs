using System.ComponentModel.DataAnnotations;

namespace Shopping_web.Models
{
	public class UserModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
		public string Username { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
