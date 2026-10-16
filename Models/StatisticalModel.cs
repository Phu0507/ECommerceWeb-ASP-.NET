using System.ComponentModel.DataAnnotations;

namespace Shopping_web.Models
{
    public class StatisticalModel
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
