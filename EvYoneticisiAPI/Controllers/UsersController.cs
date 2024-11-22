using Microsoft.AspNetCore.Mvc;
using EvYoneticisiAPI.Data;
using EvYoneticisiAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace EvYoneticisiAPI.Controllers
{
    [Authorize] // JWT doğrulama gerektirir
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EvYoneticisiContext _context;

        public UsersController(EvYoneticisiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _context.Users.Select(user => new
            {
                user.UserId,
                user.UserName
            }).ToList();

            return Ok(users);
        }

        [HttpPost]
        public IActionResult AddUser([FromBody] User user)
        {
            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                return BadRequest("Kullanıcı zaten mevcut.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(user);
        }
    }
}
