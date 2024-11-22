using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EvYoneticisiAPI.Models;
using EvYoneticisiAPI.Data;
using Microsoft.Extensions.Logging;

namespace EvYoneticisiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EvYoneticisiContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(EvYoneticisiContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // Kullanıcı Kaydı (Register)
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserLogin userLogin)
        {
            try
            {
                // Kullanıcı adı kontrolü
                if (_context.Users.Any(x => x.UserName == userLogin.UserName))
                {
                    _logger.LogWarning("Kayıt Hatası: Kullanıcı zaten mevcut.");
                    return BadRequest("Kullanıcı zaten mevcut.");
                }

                // Şifre hash'leme
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userLogin.Password);

                // Yeni kullanıcı oluştur
                var newUser = new User
                {
                    UserName = userLogin.UserName,
                    Password = hashedPassword
                };

                // Veritabanına ekle
                _context.Users.Add(newUser);
                _context.SaveChanges();

                _logger.LogInformation("Yeni kullanıcı başarıyla kaydedildi.");
                return Ok("Kullanıcı başarıyla kaydedildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Register sırasında bir hata oluştu: {ex.Message}");
                return StatusCode(500, "Bir hata oluştu, lütfen tekrar deneyin.");
            }
        }

        // Kullanıcı Girişi (Login)
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            try
            {
                // Kullanıcı adı kontrolü
                var user = _context.Users.SingleOrDefault(x => x.UserName == userLogin.UserName);
                if (user == null)
                {
                    _logger.LogWarning("Giriş Hatası: Kullanıcı bulunamadı.");
                    return Unauthorized("Geçersiz kullanıcı adı.");
                }

                // Şifre kontrolü
                if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password))
                {
                    _logger.LogWarning("Giriş Hatası: Şifre eşleşmiyor.");
                    return Unauthorized("Geçersiz şifre.");
                }

                // JWT Token oluştur
                var token = GenerateJwtToken(user);

                _logger.LogInformation($"Kullanıcı giriş yaptı: {user.UserName}");
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login sırasında bir hata oluştu: {ex.Message}");
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }

        // JWT Token Oluşturma
        private string GenerateJwtToken(User user)
        {
            // JWT için gerekli anahtar (appsettings.json'dan alınır)
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.UserName)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    // Kullanıcı Giriş Modeli
    public class UserLogin
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
