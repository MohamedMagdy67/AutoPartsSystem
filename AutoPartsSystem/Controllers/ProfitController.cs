using DTOsLayer.ProfitDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemContext;

namespace AutoPartsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfitController : ControllerBase
    {
        private readonly AutoPartsSystemDB _context;

        public ProfitController(AutoPartsSystemDB context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("day")]
        public ActionResult GetProfitDay([FromQuery] int day, [FromQuery] int Month, [FromQuery] int Year)
        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var orders = _context.Orders
                .Where(o => o.UserID == userID && o.Date.Day == day && o.Date.Month == Month && o.Date.Year == Year)
                .Sum(o => o.Price);

            var expenses = _context.Expenses
                .Where(e => e.UserID == userID && e.Date.Day == day && e.Date.Month == Month && e.Date.Year == Year)
                .Sum(e => e.Amount);

            if (orders == 0 && expenses == 0)
                return NotFound("No Expenses And Orders In This Day");

            var p = new ProfitDTO()
            {
                Orders = orders,
                Expenses = expenses,
                Sum = orders - expenses
            };

            return Ok(p);
        }

        [Authorize]
        [HttpGet("month")]
        public ActionResult GetProfitMonth([FromQuery] int month, [FromQuery] int year)

        {
            int userID = int.Parse(User.Claims.First(c => c.Type == "userID").Value);

            var orders = _context.Orders
                .Where(o => o.UserID == userID && o.Date.Month == month  && o.Date.Year == year)
                .Sum(o => o.Price);

            var expenses = _context.Expenses
                .Where(e => e.UserID == userID && e.Date.Month == month  && e.Date.Year == year)
                .Sum(e => e.Amount);

            if (orders == 0 && expenses == 0)
                return NotFound("No Expenses And Orders In This Month");

            var p = new ProfitDTO()
            {
                Orders = orders,
                Expenses = expenses,
                Sum = orders - expenses
            };

            return Ok(p);
        }
    }
}
