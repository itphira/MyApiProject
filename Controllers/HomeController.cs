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

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var notifications = await _context.notifications.ToListAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching notifications: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
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

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                var companies = await _context.companies.ToListAsync();
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching companies: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("articles/{articleId}")]
        public async Task<IActionResult> GetSpecificArticle(int articleId)
        {
            try
            {
                var articles = await _context.articulos
                    .Where(a => a.Id == articleId)
                    .ToListAsync();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching specific article: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("companies/{companyId}/articles")]
        public async Task<IActionResult> GetArticlesByCompany(int companyId)
        {
            try
            {
                var articles = await _context.articulos
                    .Where(a => a.ParentId == companyId)
                    .ToListAsync();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching articles by company: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("companies/{id}")]
        public async Task<IActionResult> GetCompany(int id)
        {
            try
            {
                var company = await _context.companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound();
                }
                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching company: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
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
                _logger.LogError($"Error fetching all articles: {ex.Message}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                var userExists = await _context.usuarios
                    .AnyAsync(u => u.username == username && u.password == password);

                if (userExists)
                {
                    return Ok(new { Message = "Login successful" });
                }
                else
                {
                    return Unauthorized(new { Message = "Invalid username or password" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost("users/change-password")]
        public async Task<IActionResult> ChangePassword([FromForm] string username, [FromForm] string currentPassword, [FromForm] string newPassword, [FromForm] string confirmPassword)
        {
            var user = await _context.usuarios.FirstOrDefaultAsync(u => u.username == username);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username" });
            }

            if (user.password != currentPassword)
            {
                return Unauthorized(new { Message = "Invalid current password" });
            }

            if (newPassword != confirmPassword)
            {
                return BadRequest(new { Message = "New password and confirm password do not match" });
            }

            user.password = newPassword;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password successfully changed" });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var usernames = await _context.usuarios.Select(u => u.username).ToListAsync();
                return Ok(usernames);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching users: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
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
                _logger.LogError($"Error fetching article comments: {ex.Message}");
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
            try
            {
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    return NotFound();
                }
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching comment: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting comment: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
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
    }

    public class NotificationRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
