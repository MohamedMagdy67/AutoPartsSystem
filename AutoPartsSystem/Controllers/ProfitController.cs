using DTOsLayer.ProfitDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemContext;

namespace AutoPartsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfitController : ControllerBase
    {
        private AutoPartsSystemDB _context;
        public ProfitController(AutoPartsSystemDB context) 
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("{day}")]
        public ActionResult GetProfitDay([FromQuery]int day)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            var Orders = _context.Orders.Where(O => O.UserID == UserID && O.Date.Day == day).Sum(O => O.Price);
            var Expenses = _context.Expenses.Where(E => E.UserID == UserID && E.Date.Day == day).Sum(E => E.Amount);
            ProfitDTO p = new ProfitDTO() { Orders = Orders, Expenses = Expenses, Sum = Orders - Expenses };
            if (Orders == null && Expenses == null) { return NotFound("No Expenses And Orders In This Day"); }
            return Ok(p);
        }
        [Authorize]
        [HttpGet("{Month}")]
        public ActionResult GetProfitMonth([FromQuery] int Month)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            var Orders = _context.Orders.Where(O => O.UserID == UserID && O.Date.Month == Month).Sum(O => O.Price);
            var Expenses = _context.Expenses.Where(E => E.UserID == UserID && E.Date.Month == Month).Sum(E => E.Amount);
            ProfitDTO p = new ProfitDTO() { Orders = Orders, Expenses = Expenses, Sum = Orders - Expenses };
            if (Orders == null && Expenses == null) { return NotFound("No Expenses And Orders In This Month"); }
            return Ok(p);
        }
    }
}
