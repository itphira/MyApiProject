using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Debug;
using MyApiProject.Data;
using MyApiProject.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyApiProject.Controllers
{
    [ApiController]
    [Route("api")]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult GetRoot()
        {
            return Ok("API is running.");
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
        public async Task<IActionResult> PostComment(int articleId, int? parentId, [FromBody] Comment comment)
        {
            if (comment == null || comment.ArticleId != articleId)
            {
                return BadRequest("Invalid comment data");
            }

            try
            {
                comment.PostedDate = DateTime.UtcNow; // Set the posted date to the current UTC time
                comment.ParentId = parentId; // Set the parent ID
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetComment), new { id = comment.CommentId }, comment);
            }
            catch (Exception ex)
            {
                // Log the exception
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
    }
}
