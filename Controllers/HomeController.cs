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

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    _logger.LogWarning("Username or password is empty.");
                    return BadRequest("Username and password are required.");
                }

                _logger.LogInformation("Checking if username exists.");
                var existingUser = await _context.usuarios.FirstOrDefaultAsync(u => u.username == request.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning("Username already exists.");
                    return Conflict("Username already exists.");
                }

                _logger.LogInformation("Encrypting password.");
                string encryptedPassword = EncryptionUtils.Encrypt(request.Password);

                var user = new User
                {
                    username = request.Username,
                    password_hash = encryptedPassword
                };

                _logger.LogInformation("Adding user to the database.");
                _context.usuarios.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User registered successfully.");
                return Ok(new { Message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // User login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.usuarios.FirstOrDefaultAsync(u => u.username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.password_hash))
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            return Ok(new { Message = "Login successful" });
        }

        // Change password
        [HttpPost("users/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Received change password request for user: {Username}", request.Username);

                var user = await _context.usuarios.FirstOrDefaultAsync(u => u.username == request.Username);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Username}", request.Username);
                    return Unauthorized(new { Message = "Invalid username" });
                }

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.password_hash))
                {
                    _logger.LogWarning("Invalid current password for user: {Username}", request.Username);
                    return Unauthorized(new { Message = "Invalid current password" });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    _logger.LogWarning("New password and confirm password do not match for user: {Username}", request.Username);
                    return BadRequest(new { Message = "New password and confirm password do not match" });
                }

                user.password_hash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password successfully changed for user: {Username}", request.Username);
                return Ok(new { Message = "Password successfully changed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user: {Username}", request.Username);
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }


        // Send notification
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

        // Reply notifications
        [HttpPost("send-reply-notification")]
        public async Task<IActionResult> SendReplyNotification([FromBody] ReplyNotificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Title))
                {
                    request.Title = "New Reply to Your Comment";
                }
                await _notificationService.SendReplyNotificationAsync(request.ToUsername, request.FromUsername, request.Message);
                return Ok(new { Message = "Reply notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendReplyNotification: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // Get all notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await _context.notifications.ToListAsync();
            return Ok(notifications);
        }

        [HttpPost("notifications")]
        public async Task<IActionResult> PostNotification([FromBody] Notification notification)
        {
            if (notification == null)
            {
                return BadRequest("Invalid notification data");
            }

            try
            {
                _context.notifications.Add(notification);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error posting notification: {ex.Message}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("notifications/{id}")]
        public async Task<IActionResult> GetNotification(int id)
        {
            var notification = await _context.notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);
        }

        [HttpPut("notifications/markAsRead/{id}")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            try
            {
                var notification = await _context.notifications.FindAsync(id);
                if (notification == null)
                {
                    return NotFound();
                }

                notification.IsRead = true;
                _context.Entry(notification).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Notification marked as read successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking notification as read: {ex.Message}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _context.companies.ToListAsync();
            return Ok(companies);
        }

        [HttpGet("articles/{articleId}")]
        public async Task<IActionResult> GetSpecificArticle(int articleId)
        {
            var articles = await _context.articulos
                .Where(a => a.Id == articleId)
                .ToListAsync();
            return Ok(articles);
        }

        [HttpGet("companies/{companyId}/articles")]
        public async Task<IActionResult> GetArticlesByCompany(int companyId)
        {
            var articles = await _context.articulos
                .Where(a => a.ParentId == companyId)
                .ToListAsync();
            return Ok(articles);
        }

        [HttpGet("companies/{id}")]
        public async Task<IActionResult> GetCompany(int id)
        {
            var company = await _context.companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return Ok(company);
        }

        [HttpGet("articles")]
        public async Task<IActionResult> GetAllArticles()
        {
            try
            {
                var articles = await _context.articulos.ToListAsync();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.usuarios.FirstOrDefaultAsync(u => u.username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.password_hash))
            {
                return Ok(new { Message = "Login successful" });
            }
            else
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var usernames = await _context.usuarios.Select(u => u.username).ToListAsync();
            return Ok(usernames);
        }

        [HttpGet("articles/{articleId}/comments")]
        public async Task<IActionResult> GetArticleComments(int articleId)
        {
            try
            {
                var comments = await _context.Comments
                    .Where(c => c.ArticleId == articleId)
                    .OrderByDescending(c => c.PostedDate)
                    .ToListAsync();

                if (comments == null || !comments.Any())
                    return NotFound(new { Message = "No comments found for this article." });

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("articles/{articleId}/comments")]
        public async Task<IActionResult> PostComment(int articleId, [FromBody] Comment comment)
        {
            _logger.LogInformation($"Received comment with ParentId: {comment.ParentId}");

            if (comment == null || comment.ArticleId != articleId)
            {
                return BadRequest("Invalid comment data");
            }

            try
            {
                comment.PostedDate = DateTime.UtcNow;
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                if (comment.ParentId.HasValue)
                {
                    var parentComment = await _context.Comments.FindAsync(comment.ParentId.Value);
                    if (parentComment != null)
                    {
                        var replyNotificationRequest = new ReplyNotificationRequest
                        {
                            Title = "New Reply to Your Comment",
                            Message = $"{comment.Author} replied to your comment.",
                            ToUsername = parentComment.Author,
                            FromUsername = comment.Author
                        };

                        await SendReplyNotification(replyNotificationRequest);
                    }
                }

                return CreatedAtAction("GetComment", new { id = comment.CommentId }, comment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error posting comment: {ex}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("comments/{id}")]
        public async Task<IActionResult> GetComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }

        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            DeleteCommentAndReplies(id);

            await _context.SaveChangesAsync();
            return Ok();
        }

        private void DeleteCommentAndReplies(int commentId)
        {
            var comment = _context.Comments.Find(commentId);
            if (comment == null) return;

            var replies = _context.Comments.Where(c => c.ParentId == commentId).ToList();
            foreach (var reply in replies)
            {
                DeleteCommentAndReplies(reply.CommentId);
            }

            _context.Comments.Remove(comment);
        }

        [HttpPost("users/password")]
        public async Task<IActionResult> GetUserPassword([FromBody] CheckPasswordRequest request)
        {
            try
            {
                if (request.AdminPassword != "Blanco+Pino#34")
                {
                    return Unauthorized("Invalid admin password.");
                }

                var user = await _context.usuarios.FirstOrDefaultAsync(u => u.username == request.Username);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                string decodedPassword = EncryptionUtils.Decrypt(user.password_hash);

                return Ok(decodedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user password.");
                return StatusCode(500, new { Message = "Internal server error. Please try again later.", Detail = ex.Message });
            }
        }
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
        public string Title { get; set; }
    }

    public class CheckPasswordRequest
    {
        public string Username { get; set; }
        public string AdminPassword { get; set; }
    }
}
