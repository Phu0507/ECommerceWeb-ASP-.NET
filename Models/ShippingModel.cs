using System.ComponentModel.DataAnnotations;

namespace Shopping_web.Models
{
	public class ShippingModel
	{
		[Key]
		public int Id { get; set; }
		public decimal Price { get; set; }
		public string Address { get; set; }
		public string Ward { get; set; }
		public string City { get; set; }

	}
}
