using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOsLayer.ProfitDtos
{
    public class ProfitDTO
    {
        public decimal Orders { get; set; }
        public decimal Expenses { get; set; }
        public decimal Sum { get; set; }
    }
}
