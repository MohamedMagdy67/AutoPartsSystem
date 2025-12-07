using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Model.Entities;
using System.Xml.Linq;
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
        [Authorize]
        [HttpGet("{CategoryID}")]
        public ActionResult GetProductTypes([FromRoute] int CategoryID)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            ProductType test = _context.ProductTypes.FirstOrDefault(T => T.CategoryID == CategoryID && T.UserID == UserID);
            if (test == null) { return NotFound("This Category Has No Products"); }
            var Types = _context.ProductTypes.Where(T => T.CategoryID == CategoryID && T.UserID == UserID).ToList();
            return Ok(Types);
        }
        [Authorize]
        [HttpPost("{CategoryID}")]
        public ActionResult PostProductTypes([FromRoute]int CategoryID,[FromBody] string TypeName)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            ProductType test = _context.ProductTypes.FirstOrDefault(t => t.Name == TypeName && t.CategoryID == CategoryID && t.UserID == UserID);
            if (test != null) { return NotFound("This ProductType Already Exist For This Category"); }
            if (TypeName == null) { return NotFound("please enter name of ProductType"); }
            ProductType Type = new ProductType() { Name = TypeName, CategoryID = CategoryID, UserID = UserID };
            _context.ProductTypes.Add(Type);
            _context.SaveChanges();
            return Ok(Type);

        }
        [Authorize]
        [HttpPut("{TypeID}")]
        public ActionResult PutProductTypes([FromRoute]int TypeID,[FromBody]string NewTypeName)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            ProductType Type = _context.ProductTypes.FirstOrDefault(T => T.ID == TypeID);
            if (Type == null || Type.UserID!=UserID) { return NotFound("This Type Not Exist"); }
            Type.Name = NewTypeName;
            _context.SaveChanges();
            return Ok(Type);

        }
        [Authorize]
        [HttpDelete("{TypeID}")]
        public ActionResult DeleteProductType([FromRoute] int TypeID)
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);


            var ProductType = _context.ProductTypes
               .Include(pt => pt.Products)
                    .ThenInclude(p => p.Orders) // Include Orders from Products
               .Include(pt => pt.Products)
                    .ThenInclude(p => p.ProductCars)
                    .ThenInclude(pc => pc.Car) // Include Cars via ProductCars
               .Include(pt => pt.Products)
               .FirstOrDefault(T => T.ID == TypeID && T.UserID == userID);

            if (ProductType == null)
                return NotFound("Type not found");

            // ===========================
            // 1) Delete Orders
            // ===========================
            var orders = ProductType.Products
                .SelectMany(p => p.Orders)
                .ToList();
            _context.Orders.RemoveRange(orders);

            // ===========================
            // 2) Delete ProductCars
            // ===========================
            var productCars = ProductType.Products
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
            var products = ProductType.Products
                .ToList();
            _context.Products.RemoveRange(products);

            // ===========================
            // 4) Delete ProductTypes
            // ===========================
            _context.ProductTypes.Remove(ProductType);

            _context.SaveChanges();

            return Ok("ProductType and all related data deleted successfully");

        }
    }
}