using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOsLayer.ProductDtos
{
    public class WareHouseDTO
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int StockOfProduct { get; set; }
        public string ProductTypeName { get; set; }

    }
}
