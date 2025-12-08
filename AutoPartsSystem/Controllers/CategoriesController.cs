using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entities;
using SystemContext;

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

        // ====================== GET CATEGORIES ======================
        [Authorize]
        [HttpGet]
        public ActionResult GetCategories()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
                return Unauthorized("UserID claim not found");

            int userId = int.Parse(userIdClaim.Value);

            var test = _context.Categories.FirstOrDefault(c => c.UserID == userId);
            if (test == null)
                return NotFound("You Have No Categories");

            var Categories = _context.Categories.Where(c => c.UserID == userId).ToList();
            return Ok(Categories);
        }

        // ====================== POST CATEGORY ======================
        [Authorize]
        [HttpPost]
        public ActionResult PostCategories([FromBody] string Name)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
                return Unauthorized("UserID claim not found");

            int UserID = int.Parse(userIdClaim.Value);

            if (string.IsNullOrWhiteSpace(Name))
                return BadRequest("Please enter name of category");

            var test = _context.Categories.FirstOrDefault(c => c.Name == Name && c.UserID == UserID);
            if (test != null)
                return BadRequest("You already have this category");

            Category category = new Category { Name = Name, UserID = UserID };
            _context.Categories.Add(category);
            _context.SaveChanges();

            return Ok(category);
        }

        // ====================== PUT CATEGORY ======================
        [Authorize]
        [HttpPut("{ID}")]
        public ActionResult PutCategories([FromRoute] int ID, [FromBody] string NewName)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
                return Unauthorized("UserID claim not found");

            int UserID = int.Parse(userIdClaim.Value);

            var c = _context.Categories.FirstOrDefault(cat => cat.UserID == UserID && cat.ID == ID);
            if (c == null)
                return NotFound("Not Found This Category");

            if (string.IsNullOrWhiteSpace(NewName))
                return BadRequest("New category name cannot be empty");

            c.Name = NewName;
            _context.SaveChanges();

            return Ok(c);
        }

        // ====================== DELETE CATEGORY ======================
        [Authorize]
        [HttpDelete("{CategoryID}")]
        public ActionResult DeleteCategory([FromRoute] int CategoryID)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
                return Unauthorized("UserID claim not found");

            int userID = int.Parse(userIdClaim.Value);

            var category = _context.Categories
                .Include(c => c.ProductTypes)
                    .ThenInclude(pt => pt.Products)
                        .ThenInclude(p => p.Orders)
                .Include(c => c.ProductTypes)
                    .ThenInclude(pt => pt.Products)
                        .ThenInclude(p => p.ProductCars)
                        .ThenInclude(pc => pc.Car)
                .Include(c => c.ProductTypes)
                    .ThenInclude(pt => pt.Products)
                .FirstOrDefault(c => c.ID == CategoryID && c.UserID == userID);

            if (category == null)
                return NotFound("Category not found");

            // ========================= DELETE RELATED DATA =========================
            var orders = category.ProductTypes
                .SelectMany(pt => pt.Products)
                .SelectMany(p => p.Orders)
                .ToList();
            _context.Orders.RemoveRange(orders);

            var productCars = category.ProductTypes
                .SelectMany(pt => pt.Products)
                .SelectMany(p => p.ProductCars)
                .ToList();
            _context.ProductCars.RemoveRange(productCars);

            var cars = productCars
                .Select(pc => pc.Car)
                .Where(c => c != null)
                .Distinct()
                .ToList();
            _context.Cars.RemoveRange(cars);

            var products = category.ProductTypes
                .SelectMany(pt => pt.Products)
                .ToList();
            _context.Products.RemoveRange(products);

            var productTypes = category.ProductTypes.ToList();
            _context.ProductTypes.RemoveRange(productTypes);

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return Ok("Category and all related data deleted successfully");
        }
    }
}
