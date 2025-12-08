using DTOsLayer.Cars;
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
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            var ProductCars = _context.ProductCars.Where( p => p.ProductID == ProductID).Include(c => c.Car).ToList();
            Product product = _context.Products.FirstOrDefault(p => p.ID == ProductID && p.UserID == UserID);
            if (ProductCars.Count() <= 0) { return NotFound("This Product Not Exist"); }
            List<string> cars = new List<string>();   
            foreach (var c in ProductCars)
            {
                string C = c.Car.Model;
                cars.Add(C);
            }
            if (cars.Count() <= 0) { return NotFound("This Product Has No Cars"); }
            CarsDTO carsDTO = new CarsDTO() {ProductName = product.Name , ProductID = product.ID , ProductCars = new List<string>(cars) };
            return Ok(carsDTO);
       }

    }
}
