using DTOsLayer.OrderDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Entities;
using System.Security.Cryptography.X509Certificates;
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
        [Authorize]
        [HttpPost]
        public ActionResult PostOrders([FromBody] Order order)
        {
            if (order.Quantity <= 0 || order.Price <= 0 || order.ProductID <= 0 || order.Date == null) { return BadRequest("Invalid Order"); }
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Order o = new Order() { Quantity = order.Quantity, Date = order.Date, Price = order.Price, UserID = UserID, ProductID = order.ProductID };
            if (order.CustomerName != null) { o.CustomerName = order.CustomerName; }
            _context.Orders.Add(o);
            _context.SaveChanges();
            Product product = _context.Products.FirstOrDefault(p => p.ID == order.ProductID && p.UserID == UserID);
            product.stock -= order.Quantity;
            _context.SaveChanges();
            return Ok(o);
        }
        [Authorize]
        [HttpPut("{OrderID}")]
        public ActionResult PutOrders([FromBody] OrderDTO order,[FromRoute]int OrderID)
        {
            if (order.Quantity <= 0 || order.price <= 0 || OrderID <= 0 ) { return BadRequest("Invalid Order"); }
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Order o = _context.Orders.FirstOrDefault(o => o.ID == OrderID && o.UserID == UserID);
            Product p = _context.Products.FirstOrDefault(p => p.ID == o.ProductID && p.UserID == UserID);
            p.stock += o.Quantity;
            p.stock -= order.Quantity;
            _context.SaveChanges();
            o.Quantity = order.Quantity;
            o.Price = order.price;
            _context.SaveChanges();
            return Ok(o);
        
        }

        [Authorize]
        [HttpDelete("{OrderID}")]
        public ActionResult DeleteOrders([FromRoute] int OrderID)
        {
             int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
             Order order = _context.Orders.FirstOrDefault(o => o.UserID == UserID && o.ID == OrderID);
             if (order == null) { return NotFound("This Order Is Not Exist"); }
             Product p = _context.Products.FirstOrDefault(P => P.ID == order.ProductID && P.UserID == UserID);
             p.stock += order.Quantity;
                _context.SaveChanges();
                _context.Orders.Remove(order);
                _context.SaveChanges();
                return Ok("Order Deleted Succesfully");
        }

    }
}
