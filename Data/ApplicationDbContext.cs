using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyApiProject.Models;
using MyApiProject.Services;

namespace MyApiProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IConfiguration configuration,
            ILogger<NotificationService> logger)
            : base(options)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public DbSet<User> usuarios { get; set; }
        public DbSet<Article> articulos { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Company> companies { get; set; }

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
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var addedArticles = ChangeTracker.Entries<Article>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            if (addedArticles.Any())
            {
                var notificationService = new NotificationService(_configuration, _logger);
                foreach (var article in addedArticles)
                {
                    await notificationService.SendNotificationAsync(
                        "New Article Added",
                        $"A new article titled '{article.Title}' has been added.");
                }
            }

            return result;
        }
    }
}
