using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyApiProject.Data;
using MyApiProject.Services;
using Microsoft.EntityFrameworkCore;

namespace MyApiProject
{
    public class ArticleMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ArticleMonitorService> _logger;

        public ArticleMonitorService(IServiceScopeFactory scopeFactory, ILogger<ArticleMonitorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckForNewArticles();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Check every minute
            }
        }

        private async Task CheckForNewArticles()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

            var recentArticles = await context.articulos
                .Where(a => a.CreatedDate >= DateTime.UtcNow.AddMinutes(-1))
                .ToListAsync();

            foreach (var article in recentArticles)
            {
                await notificationService.SendNotificationAsync("New Article Added", $"A new article titled '{article.Title}' has been added.");
                _logger.LogInformation($"Sent notification for article: {article.Title}");
            }
        }
    }
}
