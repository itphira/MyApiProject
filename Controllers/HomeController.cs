using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiProject.Data;  // Adjust this using directive based on where your DbContext is located.
using MyApiProject.Models; // This includes your User model
using System.Linq;

namespace MyApiProject.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            // Fetch all usernames from the database
            var usernames = _context.usuarios.Select(u => u.username).ToList();
            return Ok(usernames);
        }
    }
}
