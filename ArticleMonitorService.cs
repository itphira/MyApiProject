// Services/ArticleMonitorService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MyApiProject.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyApiProject.Services
{
    public class ArticleMonitorService : BackgroundService
    {
        private readonly ILogger<ArticleMonitorService> _logger;
        private readonly IServiceProvider _services;
        private DateTime _lastCheckedTime;

        public ArticleMonitorService(IServiceProvider services, ILogger<ArticleMonitorService> logger)
        {
            _services = services;
            _logger = logger;
            _lastCheckedTime = DateTime.UtcNow;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckForNewArticles();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckForNewArticles()
        {
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

                var newArticles = context.articulos
                    .Where(a => a.CreatedDate > _lastCheckedTime)
                    .ToList();

                foreach (var article in newArticles)
                {
                    await notificationService.SendNotificationAsync("New Article Added", $"Article '{article.Title}' was added.");
                }

                if (newArticles.Any())
                {
                    _lastCheckedTime = newArticles.Max(a => a.CreatedDate);
                }
            }
        }
    }
}
