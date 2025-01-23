// See https://aka.ms/new-console-template for more information
// check if chain is setup or not
// Pull the block chain from API some how or from mem pool or possibly from other nodes
//
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class SyncChainWorker : IHostedService
{
    private readonly ILogger<SyncChainWorker> _logger;

    public SyncChainWorker(ILogger<SyncChainWorker> logger)
    {
        this._logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("SyncChainWorker running at: {1}", DateTimeOffset.Now.Ticks);
            await Task.Delay(1000, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Done syncing...");
        return Task.CompletedTask;

    }
}