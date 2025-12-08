using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOsLayer.OrderDtos
{
    public class OrderDTOO
    {
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string CustomerName { get; set; }
        [Required]
        public int ProductID { get; set; }
        public int UserID { get; set; }
    }
}
