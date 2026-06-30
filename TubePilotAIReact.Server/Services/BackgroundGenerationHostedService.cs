using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TubePilotAI.Application.Services;

namespace TubePilotAIReact.Server.Services
{
    /// <summary>
    /// Hosted service placeholder for background generation tasks. Level 0: monitors job states and writes logs.
    /// Future levels: dequeue jobs and execute long-running generation.
    /// </summary>
    public class BackgroundGenerationHostedService : BackgroundService
    {
        private readonly ILogger<BackgroundGenerationHostedService> _logger;

        public BackgroundGenerationHostedService(ILogger<BackgroundGenerationHostedService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundGenerationHostedService starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Level 0: simply sleep and allow HTTP endpoints to perform generation synchronously.
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // shutting down
                }
            }

            _logger.LogInformation("BackgroundGenerationHostedService stopping.");
        }
    }
}
