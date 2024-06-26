using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyApiProject.Data;
using MyApiProject.Models;
using MyApiProject.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace MyApiProject.Controllers
{
    [ApiController]
    [Route("api")]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly NotificationService _notificationService;

        public HomeController(ApplicationDbContext context, IConfiguration configuration, ILogger<HomeController> logger, ILoggerFactory loggerFactory, NotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _loggerFactory = loggerFactory;
            _notificationService = notificationService;
        }

        [HttpGet("")]
        public IActionResult GetRoot()
        {
            return Ok("API is running.");
        }

        [HttpPost("users/check-password")]
        public async Task<IActionResult> CheckUserPassword([FromBody] CheckPasswordRequest request)
        {
            try
            {
                // Verify the admin password first
                if (request.AdminPassword != "Blanco+Pino#34")
                {
                    return Unauthorized("Invalid admin password.");
                }

                var user = await _context.usuarios.FirstOrDefaultAsync(u => u.username == request.Username);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Decrypt the password
                string decryptedPassword;
                try
                {
                    decryptedPassword = BCrypt.Net.BCrypt.HashPassword(user.password_hash);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Format error occurred while decrypting password for user: {Username}. Encrypted value: {EncryptedValue}", request.Username, user.password_hash);
                    return StatusCode(500, new { Message = "Format error. Please try again later.", Detail = ex.Message });
                }

                return Ok(new { Password = decryptedPassword });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking user password.");
                return StatusCode(500, new { Message = "Internal server error. Please try again later.", Detail = ex.Message });
            }
        }

        // ... (other methods)

        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            try
            {
                await _notificationService.SendNotificationAsync(request.Title, request.Message);
                return Ok(new { Message = "Notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendNotification: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // ... (other methods)
    }

    public class RegisterUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string Username { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class NotificationRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }

    public class ReplyNotificationRequest
    {
        public string ToUsername { get; set; }
        public string FromUsername { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }  // Add the Title property
    }

    public class CheckPasswordRequest
    {
        public string Username { get; set; }
        public string AdminPassword { get; set; }
    }
}
