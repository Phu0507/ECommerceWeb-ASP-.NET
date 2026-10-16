using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_web.Models
{
	public class OrderDetails
	{
		public int Id { get; set; }
		public string Username { get; set; }
		public string OrderCode { get; set; }
		public long ProductId { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		[ForeignKey("ProductId")]
        public ProductModel Product { get; set; }

    }
}
