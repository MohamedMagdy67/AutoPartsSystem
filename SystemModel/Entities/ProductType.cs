using SystemModel.Entities;

namespace Model.Entities
{
    public class ProductType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int CategoryID { get; set; }
        public int UserID { get; set; }

        #region Navigational Properties
        public virtual Category Category { get; set; }
        public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
        public virtual User User { get; set; }
        #endregion

    }
}