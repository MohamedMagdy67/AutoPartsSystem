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
    public class WareHouseController : ControllerBase
    {
        private readonly AutoPartsSystemDB _context;
        public WareHouseController(AutoPartsSystemDB context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet]
        public ActionResult GetStock() 
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            var Products = _context.Products.Where(p => p.UserID == UserID).Include(PT => PT.ProductType).ToList();
            List<WareHouseDTO> warehouse = new List<WareHouseDTO>();
            if (Products == null) { return NotFound("No Products Exist"); }
            foreach(var p in Products)
            {
                WareHouseDTO w = new WareHouseDTO()
                { ProductName = p.Name , ProductID = p.ID , ProductTypeName = p.ProductType.Name};
                warehouse.Add(w);
            }
            return Ok(warehouse);


        }
        [Authorize]
        [HttpPut("{ProductID}")]
        public ActionResult PutStock([FromRoute]int ProductID,[FromBody]int NewStock)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Product product = _context.Products.Include(p => p.ProductType).FirstOrDefault(p => p.UserID == UserID && p.ID == ProductID);
            if (product == null) { return NotFound("This Product Not Exist"); }
            product.stock = NewStock;
            _context.SaveChanges();
            WareHouseDTO w = new WareHouseDTO() { ProductName = product.Name, ProductID = product.ID, StockOfProduct = product.stock, ProductTypeName = product.ProductType.Name };
            return Ok(w);

        }


    }
}
