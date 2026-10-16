using System.ComponentModel.DataAnnotations;

namespace Shopping_web.Models
{
    public class ProductQuantityModel
    {
        public int Id { get; set; }
        public long ProductId { get; set; }
        [Required(ErrorMessage= "Quantity is required")]
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }

    }
}
