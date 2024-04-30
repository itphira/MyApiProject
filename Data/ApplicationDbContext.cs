using Microsoft.EntityFrameworkCore;
using MyApiProject.Models; // Adjust the namespace to match your project structure

namespace MyApiProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

         public DbSet<User> usuarios { get; set; } // This will be your entity set for Users
    }
}
