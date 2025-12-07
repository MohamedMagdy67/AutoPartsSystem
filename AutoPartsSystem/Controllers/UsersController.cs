using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SystemContext;
using Model.Entities;
using Dtos;
using BCrypt.Net;
using SystemModel.Entities;
using Microsoft.AspNetCore.Authorization;

namespace AutoPartsControllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AutoPartsSystemDB _context;
        private readonly IConfiguration _configuration;


        public UsersController(AutoPartsSystemDB context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Name == dto.Name))
                return BadRequest("User with this name already exists.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password) // encryption
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDTO
            {
                Id = user.ID,
                Name = user.Name,
                Email = user.Email
            };

            return Ok(response);
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == dto.Name);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Invalid username or password.");

            // Generate JWT token (contains only user ID)
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("userID", user.ID.ToString())
            }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }

        // ================= GET CURRENT USER =================
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userID");
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var response = new UserResponseDTO
            {
                Id = user.ID,
                Name = user.Name,
                Email = user.Email
            };

            return Ok(response);
        }
        [Authorize]
        [HttpDelete]
        public ActionResult DeleteUser()
        {
            int UserID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userID").Value);
            User u = _context.Users.Include(u => u.Categories).Include(u => u.ProductTypes).Include(u => u.Products).Include(u => u.Expenses).Include(u => u.Cars)
                .Include(u => u.Orders)
                .FirstOrDefault(u => u.ID == UserID);
            var o = u.Orders.ToList();
            _context.Orders.RemoveRange(o);
            _context.SaveChanges();  
            var E = u.Expenses.ToList();
            _context.Expenses.RemoveRange(E);
            _context.SaveChanges(); 
            
            var P = _context.Products.Include(P => P.ProductCars).Where(p => p.UserID == UserID).ToList();
            foreach (Product p in P)
            {
                var PC = _context.ProductCars.Where(PC => p.ProductCars.Contains(PC)).ToList();
                _context.ProductCars.RemoveRange(PC);
                _context.SaveChanges();

            }
            var cars = _context.Cars.Where(c => c.UserID == UserID).ToList();
            _context.Cars.RemoveRange(cars);
            _context.SaveChanges();
            var c = u.Categories.ToList();
            _context.Categories.RemoveRange(c);
            _context.SaveChanges();
            _context.Users.Remove(u);
            _context.SaveChanges();
            return Ok("The user has been deleted successfully");
        }
    }


}