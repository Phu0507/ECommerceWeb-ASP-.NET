using System.ComponentModel.DataAnnotations;

namespace Shopping_web.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Code is required")]
        public string Description { get; set; }
		public int DiscountType { get; set; }
		// 1 = %, 2 = tiền
		public int DiscountPercent { get; set; }
		public decimal DiscountAmount { get; set; }

		public DateTime DateStart { get; set; }
        public DateTime DateExpired { get; set; }
        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }
        public int Status { get; set; }

    }
}
