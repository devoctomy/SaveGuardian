using SaveGuardian.Services;

namespace SaveGuardian;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IGuardianService _guardianService;

    public Worker(
        ILogger<Worker> logger,
        IGuardianService guardianService)
    {
        _logger = logger;
        _guardianService = guardianService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(! await _guardianService.InitialiseAsync(stoppingToken))
        {
            _logger.LogError("Failed to initialise guardian service.");
            return;
        }

        _guardianService.SetupWatchers();
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}