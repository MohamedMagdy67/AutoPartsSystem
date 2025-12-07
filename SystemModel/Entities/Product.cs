using SystemModel.Entities;

namespace Model.Entities
{
    public class Product
    {

        public int ID { get; set; }
        public string Name { get; set; }
        public int stock { get; set; }
        public int ProductTypeID { get; set; }
        public int UserID { get; set; }

        #region Navigational Properties
        public virtual ProductType ProductType { get; set; }
        public virtual ICollection<ProductCar> ProductCars { get; set; } = new HashSet<ProductCar>();
        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public virtual User User { get; set; }
        #endregion

    }
}