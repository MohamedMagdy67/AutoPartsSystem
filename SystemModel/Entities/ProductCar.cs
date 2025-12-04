using System.ComponentModel.DataAnnotations;
using SystemModel.Entities;

namespace Model.Entities
{
    public class ProductCar
    {
        [Required]
        public int ProductID { get; set; }
        [Required]
        public int CarID { get; set; }

        #region Navigational Properties
        public virtual Car Car { get; set; }
        public virtual Product Product { get; set; }
        #endregion

    }
}