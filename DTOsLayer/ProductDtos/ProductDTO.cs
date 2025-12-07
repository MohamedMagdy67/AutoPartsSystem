using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOsLayer.ProductDtos
{
    public class ProductDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int stock { get; set; }
        [Required]
        public string CarModel { get; set; }
    }
}
