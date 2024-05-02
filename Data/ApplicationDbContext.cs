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

         public DbSet<User> usuarios { get; set; }
         public DbSet<Article> articulos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>().ToTable("articulos");
            modelBuilder.Entity<Article>().HasKey(a => a.Id);
            modelBuilder.Entity<Article>().Property(a => a.Id).HasColumnName("id");
            modelBuilder.Entity<Article>().Property(a => a.Title).HasColumnName("title");
            modelBuilder.Entity<Article>().Property(a => a.Text).HasColumnName("text");
            modelBuilder.Entity<Article>().Property(a => a.Image).HasColumnName("image");
        }



    }

}
