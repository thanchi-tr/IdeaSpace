namespace PersistGateKeeper.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var status = false;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information) && !status)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    status = true;
                }
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
