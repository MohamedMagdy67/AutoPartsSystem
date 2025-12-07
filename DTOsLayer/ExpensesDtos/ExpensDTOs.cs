using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOsLayer.ExpensesDtos
{
    public class ExpensDTO
    {
        public decimal? Amount { get; set; }
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public string? Message { get; set; }
    }
}
