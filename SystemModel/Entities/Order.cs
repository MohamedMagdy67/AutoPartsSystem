using System.ComponentModel.DataAnnotations;
using SystemModel.Entities;

namespace Model.Entities
{
    public class Order
    {
        public int ID { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string CustomerName { get; set; }
        [Required]
        public int ProductID { get; set; }

        #region Navigational Properties
        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
        #endregion


    }
}