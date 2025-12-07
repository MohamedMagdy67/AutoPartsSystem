using SystemModel.Entities;

namespace Model.Entities
{
    public class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int UserID { get; set; }
        #region Navigational Properties
        public virtual ICollection<ProductType> ProductTypes { get; set; } = new HashSet<ProductType>();
        public virtual User User { get; set; }
        #endregion

    }
}
