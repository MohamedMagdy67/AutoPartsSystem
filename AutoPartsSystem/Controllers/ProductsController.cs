using DTOsLayer.ProductDtos;
using Microsoft.AspNetCore.Authorization;
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

        // ==========================================================
        // GET PRODUCTS
        // ==========================================================
        [Authorize]
        [HttpGet("{ProductTypeID}")]
        public ActionResult GetProducts(int ProductTypeID)
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var products = _context.Products
                .Where(p => p.ProductTypeID == ProductTypeID && p.UserID == userID)
                .ToList();

            if (!products.Any())
                return NotFound("No products exist");

            return Ok(products);
        }

        // ==========================================================
        // ADD PRODUCT
        // ==========================================================
        [Authorize]
        [HttpPost("{ProductTypeID}")]
        public ActionResult AddProduct(int ProductTypeID, [FromBody] ProductDTO dto)
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

            // Create new product
            var newProduct = new Product
            {
                Name = dto.Name,
                stock = dto.stock,
                ProductTypeID = ProductTypeID,
                UserID = userID
            };

            _context.Products.Add(newProduct);
            _context.SaveChanges();

            // create car relations
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
            ProductDTO pp = new ProductDTO() { Name = newProduct.Name, stock = newProduct.stock, CarModel = dto.CarModel };
            return Ok(pp);
        }

        // ==========================================================
        // UPDATE PRODUCT
        // ==========================================================
        [Authorize]
        [HttpPut("{ProductID}")]
        public ActionResult PutProduct(int ProductID, [FromBody] ProductDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.CarModel) ||
                dto.stock < 0)
            {
                return BadRequest("Invalid product data");
            }

            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var product = _context.Products
                .Include(p => p.ProductCars)
                    .ThenInclude(pc => pc.Car)
                .FirstOrDefault(p => p.ID == ProductID && p.UserID == userID);

            if (product == null)
                return NotFound("Product not found");

            var requestedModels = dto.CarModel
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim())
                .OrderBy(m => m)
                .ToList();

            product.Name = dto.Name;
            product.stock = dto.stock;

            var oldLinks = product.ProductCars.ToList();
            var oldCars = oldLinks.Select(pc => pc.Car).Distinct().ToList();

            _context.ProductCars.RemoveRange(oldLinks);

            // delete cars only if unused
            foreach (var car in oldCars)
            {
                bool used = _context.ProductCars.Any(pc => pc.CarID == car.ID && pc.ProductID != product.ID);
                if (!used)
                    _context.Cars.Remove(car);
            }

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
                    ProductID = product.ID,
                    CarID = car.ID
                });
            }

            _context.SaveChanges();

            return Ok(product);
        }

        // ==========================================================
        // DELETE PRODUCT
        // ==========================================================
        [Authorize]
        [HttpDelete("{ProductID}")]
        public ActionResult DeleteProduct(int ProductID)
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var product = _context.Products
               .Include(p => p.Orders)
               .Include(p => p.ProductCars)
                    .ThenInclude(pc => pc.Car)
               .FirstOrDefault(p => p.ID == ProductID && p.UserID == userID);

            if (product == null)
                return NotFound("Product not found");

            // 1) delete orders
            _context.Orders.RemoveRange(product.Orders);

            // 2) delete productCars
            var carLinks = product.ProductCars.ToList();
            _context.ProductCars.RemoveRange(carLinks);

            // 3) delete cars ONLY if unused by other products
            foreach (var car in carLinks.Select(pc => pc.Car).Distinct())
            {
                bool usedByOthers = _context.ProductCars.Any(pc => pc.CarID == car.ID);
                if (!usedByOthers)
                    _context.Cars.Remove(car);
            }

            // 4) delete product
            _context.Products.Remove(product);

            _context.SaveChanges();

            return Ok("Product deleted successfully");
        }
    }
}
