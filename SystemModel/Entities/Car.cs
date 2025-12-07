using System.ComponentModel.DataAnnotations;
using SystemModel.Entities;

namespace Model.Entities
{
    public class Car
    {

        public int ID { get; set; }
        [Required]
        public string Model { get; set; }
        public int UserID { get; set; }
        #region Navigational Properties
        public virtual ICollection<ProductCar> ProductCars { get; set; } = new HashSet<ProductCar>();
        public virtual User User { get; set; }
        #endregion
    }
}
