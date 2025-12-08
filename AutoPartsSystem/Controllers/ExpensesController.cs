using DTOsLayer.ExpensesDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Entities;
using System.Linq;
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

        // ====================== GET ALL EXPENSES ======================
        [Authorize]
        [HttpGet]
        public ActionResult GetExpenses()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
                return Unauthorized("UserID claim not found");

            int userID = int.Parse(userIdClaim.Value);

            var test = _context.Expenses.Where(t => t.UserID == userID).Select(e => e.ID);
            if (test == null ) { return NotFound("You Have No Expenses"); }
            var Expenses = _context.Expenses.Where(e => e.UserID == userID);
            if (!Expenses.Any()) { return NotFound("You Have No Expenses"); }
            var ee = Expenses.ToList();
            return Ok(ee);
        }

        // ====================== CREATE EXPENSE ======================
        [Authorize]
        [HttpPost]
        public ActionResult PostExpense([FromBody] ExpensDTO expenseDTO)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
                return Unauthorized("UserID claim not found");

            int userID = int.Parse(userIdClaim.Value);

            if (expenseDTO == null || string.IsNullOrWhiteSpace(expenseDTO.Name) || expenseDTO.Amount == null || expenseDTO.Date == null)
                return BadRequest("Invalid expense data");

            var expense = new Expens
            {
                Name = expenseDTO.Name,
                Amount = expenseDTO.Amount.Value,
                Date = expenseDTO.Date.Value,
                UserID = userID,
                Message = expenseDTO.Message
            };

            _context.Expenses.Add(expense);
            _context.SaveChanges();

            return Ok(expense);
        }

        // ====================== UPDATE EXPENSE ======================
        [Authorize]
        [HttpPut("{id}")]
        public ActionResult PutExpense([FromRoute] int id, [FromBody] ExpensDTO expenseDTO)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
                return Unauthorized("UserID claim not found");

            int userID = int.Parse(userIdClaim.Value);

            if (expenseDTO == null || string.IsNullOrWhiteSpace(expenseDTO.Name) || expenseDTO.Amount == null || expenseDTO.Date == null)
                return BadRequest("Invalid expense data");

            var expense = _context.Expenses.FirstOrDefault(e => e.ID == id && e.UserID == userID);
            if (expense == null)
                return NotFound("Expense not found");

            expense.Name = expenseDTO.Name;
            expense.Amount = expenseDTO.Amount.Value;
            expense.Date = expenseDTO.Date.Value;
            expense.Message = expenseDTO.Message;

            _context.SaveChanges();

            return Ok(expense);
        }

        // ====================== DELETE EXPENSE ======================
        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult DeleteExpense([FromRoute] int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null)
                return Unauthorized("UserID claim not found");

            int userID = int.Parse(userIdClaim.Value);

            var expense = _context.Expenses.FirstOrDefault(e => e.ID == id && e.UserID == userID);
            if (expense == null)
                return NotFound("Expense not found");

            _context.Expenses.Remove(expense);
            _context.SaveChanges();

            return Ok("Expense deleted successfully");
        }
    }

}
