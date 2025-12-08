using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOsLayer.Cars
{
    public class CarsDTO
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public List<string> ProductCars { get; set; }
    }
}
