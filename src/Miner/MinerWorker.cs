// See https://aka.ms/new-console-template for more information
// check if chain is setup or not
// Pull the block chain from API some how or from mem pool or possibly from other nodes
//
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class MinerWorker : BackgroundService
{
    private readonly ILogger<MinerWorker> _logger;

    public MinerWorker(ILogger<MinerWorker> logger)
    {
        this._logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Worker running at: {1}", DateTimeOffset.Now.Ticks);
            await Task.Delay(1000, stoppingToken);
        }
    }
}