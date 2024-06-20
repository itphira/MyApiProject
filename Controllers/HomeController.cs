﻿using Microsoft.AspNetCore.Mvc;
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

        // Get all notifications for a specific user
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] int userId)
        {
            var notifications = await _context.notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            return Ok(notifications);
        }

        [HttpPost("notifications")]
        public async Task<IActionResult> PostNotification([FromBody] Notification notification)
        {
            if (notification == null || notification.UserId <= 0)
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

        // Mark a notification as read for a specific user
        [HttpPut("notifications/markAsRead/{id}")]
        public async Task<IActionResult> MarkNotificationAsRead(int id, [FromQuery] int userId)
        {
            try
            {
                var notification = await _context.notifications
                    .Where(n => n.Id == id && n.UserId == userId)
                    .FirstOrDefaultAsync();
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
                // Log the exception details to help with debugging
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.usuarios
                .FirstOrDefaultAsync(u => u.username == username && u.password == password);

            if (user != null)
            {
                return Ok(new { Message = "Login successful", UserId = user.id });
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
}
