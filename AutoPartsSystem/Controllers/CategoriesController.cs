using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Model.Entities;
using System.Linq.Expressions;
using SystemContext;
using SystemModel.Entities;

namespace AutoPartsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AutoPartsSystemDB _context;
        public CategoriesController(AutoPartsSystemDB context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet]
        public ActionResult GetCategories()
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Category test = _context.Categories.FirstOrDefault(C => C.User.ID == userId);
            if (test == null) { return NotFound("You Have No Categories"); }
            var Categories = _context.Categories.Where(C => C.User.ID == userId);
            return Ok(Categories);


        }
        [Authorize]
        [HttpPost]
        public ActionResult PostCategories([FromBody]string Name)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(C => C.Type == "userID").Value);
            Category test = _context.Categories.FirstOrDefault(c => c.Name == Name && c.UserID == UserID);
            if (test != null)
            {
                 return NotFound("You already have this category"); 
            }
            if (Name == null) { return NotFound("please enter name of category"); }
            Category Categoryy = new Category { Name = Name, UserID = UserID };
            _context.Categories.Add(Categoryy);
            _context.SaveChanges();
            return Ok(Categoryy);

        }
        [Authorize]
        [HttpPut("{ID}")]
        public ActionResult PutCategories([FromRoute]int ID,[FromBody]string NewName)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Category c = _context.Categories.FirstOrDefault(c => c.UserID == UserID && c.ID == ID);
            if (c == null) { return NotFound("Not Found This Category"); }
            c.Name = NewName;
            _context.SaveChanges();
            return Ok(c);

        }
        [Authorize]
        [HttpDelete("{CategoryID}")]
        public ActionResult DeleteCategory([FromRoute] int CategoryID)
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);


            var category = _context.Categories
                .Include(c => c.ProductTypes)
                    .ThenInclude(pt => pt.Products)
                    .ThenInclude(p => p.Orders) // Include Orders from Products
                .Include(c => c.ProductTypes)
                    .ThenInclude(pt => pt.Products)
                    .ThenInclude(p => p.ProductCars)
                    .ThenInclude(pc => pc.Car) // Include Cars via ProductCars
                .Include(c => c.ProductTypes)
                    .ThenInclude(pt => pt.Products)
                .FirstOrDefault(c => c.ID == CategoryID && c.UserID == userID);

            if (category == null)
                return NotFound("Category not found");

            // ===========================
            // 1) Delete Orders
            // ===========================
            var orders = category.ProductTypes
                .SelectMany(pt => pt.Products)
                .SelectMany(p => p.Orders)
                .ToList();
            _context.Orders.RemoveRange(orders);

            // ===========================
            // 2) Delete ProductCars
            // ===========================
            var productCars = category.ProductTypes
                .SelectMany(pt => pt.Products)
                .SelectMany(p => p.ProductCars)
                .ToList();
            _context.ProductCars.RemoveRange(productCars);

            // ===========================
            // 2.5) Delete Cars
            // ===========================
            var cars = productCars
                .Select(pc => pc.Car)
                .Where(c => c != null)
                .Distinct()
                .ToList();
            _context.Cars.RemoveRange(cars);

            // ===========================
            // 3) Delete Products
            // ===========================
            var products = category.ProductTypes
                .SelectMany(pt => pt.Products)
                .ToList();
            _context.Products.RemoveRange(products);

            // ===========================
            // 4) Delete ProductTypes
            // ===========================
            var productTypes = category.ProductTypes.ToList();
            _context.ProductTypes.RemoveRange(productTypes);

            // ===========================
            // 5) Delete the Category itself
            // ===========================
            _context.Categories.Remove(category);

            _context.SaveChanges();

            return Ok("Category and all related data deleted successfully");

}



    }
}