using FirebaseAdmin.Messaging;
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
         public DbSet<Comment> Comments { get; set; }
         public DbSet<Company> companies { get; set; }
         public DbSet<Notification> notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Article>().ToTable("articulos");
            modelBuilder.Entity<Article>().HasKey(a => a.Id);
            modelBuilder.Entity<Article>().Property(a => a.Id).HasColumnName("id");
            modelBuilder.Entity<Article>().Property(a => a.Title).HasColumnName("title");
            modelBuilder.Entity<Article>().Property(a => a.Text).HasColumnName("text");
            modelBuilder.Entity<Article>().Property(a => a.Image).HasColumnName("image");

            modelBuilder.Entity<Comment>().ToTable("comments");
            modelBuilder.Entity<Comment>().HasKey(c => c.CommentId);
            modelBuilder.Entity<Comment>().Property(c => c.CommentId).HasColumnName("comment_id");
            modelBuilder.Entity<Comment>().Property(c => c.ArticleId).HasColumnName("articulo_id");
            modelBuilder.Entity<Comment>().Property(c => c.ParentId).HasColumnName("parent_id").IsRequired(false);
            modelBuilder.Entity<Comment>().Property(c => c.Author).HasColumnName("author");
            modelBuilder.Entity<Comment>().Property(c => c.CommentText).HasColumnName("comment");
            modelBuilder.Entity<Comment>().Property(c => c.PostedDate).HasColumnName("posted_date");
            modelBuilder.Entity<Comment>().HasOne<Article>().WithMany().HasForeignKey(c => c.ArticleId);

            modelBuilder.Entity<Notification>().ToTable("notifications");
            modelBuilder.Entity<Notification>().HasKey(n => n.Id);
            modelBuilder.Entity<Notification>().Property(n => n.Id).HasColumnName("id");
            modelBuilder.Entity<Notification>().Property(n => n.Title).HasColumnName("title");
            modelBuilder.Entity<Notification>().Property(n => n.Text).HasColumnName("text");
            modelBuilder.Entity<Notification>().Property(n => n.ArticleId).HasColumnName("article_id");
            modelBuilder.Entity<Notification>().Property(n => n.CompanyId).HasColumnName("company_id");
        }
    }
}
