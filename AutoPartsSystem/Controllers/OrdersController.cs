using DTOsLayer.OrderDtos;
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
    public class OrdersController : ControllerBase
    {
        private readonly AutoPartsSystemDB _context;

        public OrdersController(AutoPartsSystemDB context)
        {
            _context = context;
        }

        // ====================================
        // GET ALL ORDERS
        // ====================================
        [Authorize]
        [HttpGet]
        public ActionResult GetOrders()
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var orders = _context.Orders
                .Where(o => o.UserID == userID)
                .ToList();

            if (orders.Count == 0)
                return NotFound("No Orders Exist");

            return Ok(orders);
        }

        // ====================================
        // ADD ORDER
        // ====================================
        [Authorize]
        [HttpPost]
        public ActionResult PostOrders([FromBody] OrderDTOO order)
        {
            if (order.Quantity <= 0 || order.Price <= 0 || order.ProductID <= 0)
                return BadRequest("Invalid Order");

            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var product = _context.Products
                .FirstOrDefault(p => p.ID == order.ProductID && p.UserID == userID);

            if (product == null)
                return BadRequest("Product Not Found For This User");

            if (product.stock < order.Quantity)
                return BadRequest("Not Enough Stock");

            // Create new order
            Order o = new Order
            {
                Quantity = order.Quantity,
                Date = order.Date,
                Price = order.Price,
                UserID = userID,
                ProductID = order.ProductID,
                CustomerName = order.CustomerName
            };

            _context.Orders.Add(o);

            // Reduce stock
            product.stock -= order.Quantity;

            _context.SaveChanges();

            return Ok(order);
        }

        // ====================================
        // UPDATE ORDER
        // ====================================
        [Authorize]
        [HttpPut("{OrderID}")]
        public ActionResult PutOrders([FromBody] OrderDTO order, [FromRoute] int OrderID)
        {
            if (order.Quantity <= 0 || order.price <= 0)
                return BadRequest("Invalid Order");

            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var existingOrder = _context.Orders
                .FirstOrDefault(o => o.ID == OrderID && o.UserID == userID);

            if (existingOrder == null)
                return NotFound("Order Not Found");

            var product = _context.Products
                .FirstOrDefault(p => p.ID == existingOrder.ProductID && p.UserID == userID);

            if (product == null)
                return BadRequest("Product Not Found");

            // Restore old stock
            product.stock += existingOrder.Quantity;

            // Check new stock availability
            if (product.stock < order.Quantity)
                return BadRequest("Not Enough Stock For Update");

            // Deduct new quantity
            product.stock -= order.Quantity;

            // Update order
            existingOrder.Quantity = order.Quantity;
            existingOrder.Price = order.price;

            _context.SaveChanges();
            OrderDTOO oo = new OrderDTOO()
            { ProductID = existingOrder.ProductID, Quantity = existingOrder.Quantity, Price = existingOrder.Price, Date = existingOrder.Date, CustomerName = existingOrder.CustomerName, UserID = existingOrder.UserID };
            return Ok(oo);
        }

        // ====================================
        // DELETE ORDER
        // ====================================
        [Authorize]
        [HttpDelete("{OrderID}")]
        public ActionResult DeleteOrders(int OrderID)
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var order = _context.Orders
                .FirstOrDefault(o => o.ID == OrderID && o.UserID == userID);

            if (order == null)
                return NotFound("Order Not Exist");

            var product = _context.Products
                .FirstOrDefault(p => p.ID == order.ProductID && p.UserID == userID);

            if (product != null)
            {
                // Restore stock
                product.stock += order.Quantity;
            }

            _context.Orders.Remove(order);
            _context.SaveChanges();

            return Ok("Order Deleted Successfully");
        }
    }
}
