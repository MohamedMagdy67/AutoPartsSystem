using System.ComponentModel.DataAnnotations;
using SystemModel.Entities;

namespace Model.Entities
{
    public class Expens
    {
        public int ID { get; set; }
        public string Name { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string Message { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public int UserID { get; set; }
        #region Navigitional Properties
        public virtual User User { get; set; }
        #endregion


    }
}