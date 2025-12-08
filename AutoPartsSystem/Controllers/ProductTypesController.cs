using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entities;
using SystemContext;

namespace AutoPartsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypesController : ControllerBase
    {
        private readonly AutoPartsSystemDB _context;

        public ProductTypesController(AutoPartsSystemDB context)
        {
            _context = context;
        }

        // ===============================
        // GET Product Types for Category
        // ===============================
        [Authorize]
        [HttpGet("{CategoryID}")]
        public ActionResult GetProductTypes(int CategoryID)
        {
            int UserID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            if (!_context.Categories.Any(c => c.ID == CategoryID && c.UserID == UserID))
                return NotFound("No Category Exist With This ID");

            var types = _context.ProductTypes
                .Where(t => t.CategoryID == CategoryID && t.UserID == UserID)
                .ToList();

            if (!types.Any())
                return NotFound("This Category Has No Products");

            return Ok(types);
        }

        // ===============================
        // POST Create Product Type
        // ===============================
        [Authorize]
        [HttpPost("{CategoryID}")]
        public ActionResult PostProductTypes(int CategoryID, [FromBody] string TypeName)
        {
            int UserID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            if (!_context.Categories.Any(c => c.ID == CategoryID && c.UserID == UserID))
                return NotFound("No Category Exist With This ID");

            if (string.IsNullOrWhiteSpace(TypeName))
                return BadRequest("Please enter a valid ProductType name");

            var exists = _context.ProductTypes
                .Any(t => t.Name == TypeName && t.CategoryID == CategoryID && t.UserID == UserID);

            if (exists)
                return BadRequest("This ProductType Already Exists For This Category");

            var type = new ProductType
            {
                Name = TypeName,
                CategoryID = CategoryID,
                UserID = UserID
            };

            _context.ProductTypes.Add(type);
            _context.SaveChanges();

            return Ok(type);
        }

        // ===============================
        // PUT Update Product Type
        // ===============================
        [Authorize]
        [HttpPut("{TypeID}")]
        public ActionResult PutProductTypes(int TypeID, [FromBody] string NewTypeName)
        {
            int UserID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var type = _context.ProductTypes.FirstOrDefault(t => t.ID == TypeID && t.UserID == UserID);

            if (type == null)
                return NotFound("This Type Does Not Exist");

            type.Name = NewTypeName;
            _context.SaveChanges();

            return Ok(type);
        }

        // ===============================
        // DELETE Product Type + Related
        // ===============================
        [Authorize]
        [HttpDelete("{TypeID}")]
        public ActionResult DeleteProductType(int TypeID)
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var productType = _context.ProductTypes
                .Include(pt => pt.Products)
                    .ThenInclude(p => p.Orders)
                .Include(pt => pt.Products)
                    .ThenInclude(p => p.ProductCars)
                .FirstOrDefault(t => t.ID == TypeID && t.UserID == userID);

            if (productType == null)
                return NotFound("Type not found");

            // 1) Delete Orders
            var orders = productType.Products.SelectMany(p => p.Orders).ToList();
            _context.Orders.RemoveRange(orders);

            // 2) Delete ProductCars only — DO NOT delete Cars
            var productCars = productType.Products.SelectMany(p => p.ProductCars).ToList();
            _context.ProductCars.RemoveRange(productCars);

            // 3) Delete Products
            var products = productType.Products.ToList();
            _context.Products.RemoveRange(products);

            // 4) Delete ProductType
            _context.ProductTypes.Remove(productType);

            _context.SaveChanges();

            return Ok("ProductType and all related data deleted successfully");
        }
    }
}
