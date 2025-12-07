using DTOsLayer.ProductDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entities;
using SystemContext;

namespace AutoPartsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public readonly AutoPartsSystemDB _context;
        public ProductsController(AutoPartsSystemDB context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet("{ProductTypeID}")]
        public ActionResult GetProducts([FromRoute] int ProductTypeID)
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var products = _context.Products
                .Where(p => p.ProductTypeID == ProductTypeID && p.UserID == userID)
                .ToList();

            if (products.Count == 0)
                return NotFound("No products exist");

            return Ok(products);
        }
        [Authorize]
        [HttpPost("{ProductTypeID}")]
        public ActionResult AddProduct([FromRoute] int ProductTypeID, [FromBody] ProductDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.CarModel) ||
                ProductTypeID <= 0 ||
                dto.stock < 0)
            {
                return BadRequest("Invalid product data");
            }

            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var requestedModels = dto.CarModel
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim())
                .OrderBy(m => m)
                .ToList();

            var products = _context.Products
                .Include(p => p.ProductCars)
                .Where(p => p.Name == dto.Name &&
                            p.ProductTypeID == ProductTypeID &&
                            p.UserID == userID)
                .ToList();

            foreach (var product in products)
            {
                var existingCarIDs = product.ProductCars.Select(pc => pc.CarID).ToList();

                var existingModels = _context.Cars
                    .Where(c => existingCarIDs.Contains(c.ID))
                    .Select(c => c.Model)
                    .OrderBy(m => m)
                    .ToList();

                if (requestedModels.SequenceEqual(existingModels))
                {
                    product.stock += dto.stock;
                    _context.SaveChanges();
                    return Ok(product);
                }
            }

            var newProduct = new Product
            {
                Name = dto.Name,
                stock = dto.stock,
                ProductTypeID = ProductTypeID,
                UserID = userID
            };

            _context.Products.Add(newProduct);
            _context.SaveChanges();

            var allUserCars = _context.Cars.Where(c => c.UserID == userID).ToList();

            foreach (var model in requestedModels)
            {
                var car = allUserCars.FirstOrDefault(c => c.Model == model);

                if (car == null)
                {
                    car = new Car { Model = model, UserID = userID };
                    _context.Cars.Add(car);
                    _context.SaveChanges();
                }

                _context.ProductCars.Add(new ProductCar
                {
                    ProductID = newProduct.ID,
                    CarID = car.ID
                });
            }

            _context.SaveChanges();

            return Ok(new
            {
                newProduct.ID,
                newProduct.Name,
                newProduct.stock,
                newProduct.ProductTypeID

            });
        }
        //[HttpPut("ProductID")]
        //public ActionResult PutProduct([FromRoute]int ProductID, [FromBody] ProductDTO dto)
        //{

        
        //}
        [Authorize]
        [HttpDelete("{ProductID}")]
        public ActionResult DeleteProduct([FromRoute] int ProductID)
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);


            var Product = _context.Products
               .Include(p => p.Orders) // Include Orders from Products
               .Include(p => p.ProductCars)
                    .ThenInclude(pc => pc.Car) // Include Cars via ProductCars
               .FirstOrDefault(P => P.ID == ProductID && P.UserID == userID);

            if (Product == null)
                return NotFound("Product not found");

            // ===========================
            // 1) Delete Orders
            // ===========================
            var orders = Product.Orders
                .ToList();
            _context.Orders.RemoveRange(orders);

            // ===========================
            // 2) Delete ProductCars
            // ===========================
            var productCars = Product.ProductCars
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
            _context.Products.Remove(Product);
            _context.SaveChanges();

            return Ok("Product and all related data deleted successfully");

        }

    }
}
