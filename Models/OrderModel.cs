namespace Shopping_web.Models
{
	public class OrderModel
	{
		public int Id { get; set; }
		public string OrderCode { get; set; }
		public decimal ShippingCost { get; set; }
		public decimal SubTotal { get; set; }      // Tổng tiền sản phẩm
		public decimal DiscountAmount { get; set; }
		public decimal TotalAmount { get; set; }   // Tổng cuối cùng
		public string CouponCode { get; set; }
		public string Address { get; set; }
		public string Username { get; set; }
		public DateTime CreatedDate { get; set; }
		public int Status { get; set; }
	}
}
