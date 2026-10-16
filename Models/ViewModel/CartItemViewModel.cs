namespace Shopping_web.Models.ViewModel
{
	public class CartItemViewModel
	{
		public List<CartItemModel> CartItems { get; set; }
		public decimal GrandTotal { get; set; }
		public decimal ShippingCost { get; set; }
		public string ShippingAddress { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal TotalAmount { get; set; }
		public string CouponCode { get; set; }
	}
}
