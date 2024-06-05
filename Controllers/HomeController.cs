using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Debug;
using MyApiProject.Data;
using MyApiProject.Models;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MyApiProject.Controllers
{
    [ApiController]
    [Route("api")]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;  // Add a logger
        private readonly IConfiguration _configuration;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger; // Initialize the logger
            _configuration = configuration;
        }

        [HttpGet("")]
        public IActionResult GetRoot()
        {
            return Ok("API is running.");
        }

        [HttpPost("articles")]
        public async Task<IActionResult> CreateArticle([FromBody] Article article)
        {
            if (article == null)
            {
                return BadRequest("Invalid article data");
            }

            try
            {
                _context.articulos.Add(article);
                await _context.SaveChangesAsync();

                // Send notification
                var notificationService = new NotificationService(_configuration);
                await notificationService.SendNotificationAsync("New Article", "A new article has been added.");

                return CreatedAtAction("GetArticle", new { id = article.Id }, article);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating article: {ex}");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // Test notification
        [HttpPost("notification")]
        public async Task<IActionResult> SendNotification()
        {
            var notificationService = new NotificationService(_configuration);
            await notificationService.SendNotificationAsync("Test Notification", "This is a test notification from the API.");
            return Ok(new { Message = "Notification sent successfully" });
        }

        // Get all companies
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _context.companies.ToListAsync();
            return Ok(companies);
        }

        // Get articles by company
        [HttpGet("companies/{companyId}/articles")]
        public async Task<IActionResult> GetArticlesByCompany(int companyId)
        {
            var articles = await _context.articulos
                .Where(a => a.ParentId == companyId)
                .ToListAsync();
            return Ok(articles);
        }

        // Get a specific company
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
                // Log the exception details to help with debugging
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


        [HttpGet("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Check if a user with the given username and password exists
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

        [HttpPost("users/change-password")]
        public async Task<IActionResult> ChangePassword([FromForm] string username, [FromForm] string currentPassword, [FromForm] string newPassword, [FromForm] string confirmPassword)
        {
            var user = await _context.usuarios.FirstOrDefaultAsync(u => u.username == username);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username" });
            }

            if (user.password != currentPassword) // Adjust this line based on your password hashing/salting mechanism
            {
                return Unauthorized(new { Message = "Invalid current password" });
            }

            if (newPassword != confirmPassword)
            {
                return BadRequest(new { Message = "New password and confirm password do not match" });
            }

            user.password = newPassword; // Ensure you hash the password if it's production code
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password successfully changed" });
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
            _logger.LogInformation($"Received comment with ParentId: {comment.ParentId}"); // Log the ParentId

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

            DeleteCommentAndReplies(id);  // Recursive deletion function

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
                DeleteCommentAndReplies(reply.CommentId);  // Recursive call to handle nested replies
            }

            _context.Comments.Remove(comment);
        }

    }
}
