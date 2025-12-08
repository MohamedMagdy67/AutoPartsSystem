using DTOsLayer.Cars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entities;
using SystemContext;

namespace AutoPartsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly AutoPartsSystemDB _context;
        public CarsController(AutoPartsSystemDB context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("{ProductID}")]
        public ActionResult GetProductCars([FromRoute] int ProductID)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
                return Unauthorized("UserID claim not found");

            int UserID = int.Parse(userIdClaim.Value);

            var product = _context.Products.FirstOrDefault(p => p.ID == ProductID && p.UserID == UserID);
            if (product == null)
                return NotFound("This Product Not Exist");

            var productCars = _context.ProductCars
                .Where(pc => pc.ProductID == ProductID)
                .Include(pc => pc.Car)
                .ToList();

            if (productCars.Count == 0)
                return NotFound("This Product Has No Cars");

            var carsList = productCars
                .Where(pc => pc.Car != null)
                .Select(pc => pc.Car.Model)
                .ToList();

            if (carsList.Count == 0)
                return NotFound("This Product Has No Cars");

            var carsDTO = new CarsDTO
            {
                ProductName = product.Name,
                ProductID = product.ID,
                ProductCars = carsList
            };

            return Ok(carsDTO);
        }
    }
}
