using DTOsLayer.ExpensesDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Model.Entities;
using SystemContext;

namespace AutoPartsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly AutoPartsSystemDB _context;
        public ExpensesController(AutoPartsSystemDB context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet]
        public ActionResult GetExpenses()
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Expens test = _context.Expenses.FirstOrDefault(e => e.UserID == UserID);
            if (test == null) { return NotFound("No Expenses Exist"); }
            var Expenses = _context.Expenses.Where(E => E.UserID == UserID);
            return Ok(Expenses);

        }
        [Authorize]
        [HttpPost]
        public ActionResult PostExpenses([FromBody]ExpensDTO Expens)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            if (Expens.Name == null || Expens.Amount == null || Expens.Date == null) { return NotFound("Invalid Expens"); }
            Expens e = new Expens() { Name = Expens.Name, Amount = (decimal)Expens.Amount, Date = (DateTime)Expens.Date, UserID = UserID };
            if (Expens.Message != null) { e.Message = Expens.Message; }
            _context.Expenses.Add(e);
            _context.SaveChanges();
            return Ok(e);

        }
        [Authorize]
        [HttpPut("{id}")]
        public ActionResult PutExpenses([FromRoute]int id, [FromBody]ExpensDTO Expens)
        {
            if (Expens.Name == null || Expens.Amount == null || Expens.Date == null) { return NotFound("Invalid Expens"); }
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Expens e = _context.Expenses.FirstOrDefault(E => E.ID == id && E.UserID == UserID);
            if (e == null) { return NotFound("Expens Not Exist"); }
            if (Expens.Name != null) { e.Name = Expens.Name; }
            if (Expens.Amount != null) { e.Amount = (decimal)Expens.Amount; }
            if (Expens.Date != null) { e.Date = (DateTime)Expens.Date; }
            if (Expens.Message != null) { e.Message = Expens.Message; }
; _context.SaveChanges();
            return Ok(e);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult DeleteExpenses([FromRoute]int id)
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            Expens e = _context.Expenses.FirstOrDefault(E => E.ID == id && E.UserID == UserID);
            if (e == null) {return NotFound("This Expens Is Not Exist");}
            _context.Expenses.Remove(e);
            _context.SaveChanges();
            return Ok("Deleted Succesfully");
        }
    }
}