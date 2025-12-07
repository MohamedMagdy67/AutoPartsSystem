using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Entities;
using SystemContext;

namespace AutoPartsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    { 
        private readonly AutoPartsSystemDB _context;
        public OrdersController(AutoPartsSystemDB context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet]
        public ActionResult GetOrders()
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Order ordertest = _context.Orders.FirstOrDefault(o => o.UserID == UserID);
            if (ordertest == null) { return NotFound("No Orders Exist"); }
            var orders = _context.Orders.Where(o => o.UserID == UserID).ToList();
            return Ok(orders);
        }
        //[Authorize]
        //[HttpPost]
        //public ActionResult PostOrders([FromBody] Order order)
        //{
            
        //}
        [Authorize]
        [HttpDelete("{OrderID}")]
        public ActionResult DeleteOrders([FromRoute] int OrderID)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Order order = _context.Orders.FirstOrDefault(o => o.UserID == UserID && o.ID == OrderID);
            if(order == null) { return NotFound("This Order Is Not Exist"); }
            Product p = _context.Products.FirstOrDefault(P => P.ID == order.ProductID && P.UserID == UserID);
            p.stock += order.Quantity;
            _context.SaveChanges();
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return Ok("Order Deleted Succesfully");
        }
    }
}
