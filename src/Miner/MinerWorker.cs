// See https://aka.ms/new-console-template for more information
// check if chain is setup or not
//
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Processors.Communication;
using System.Threading.Channels;

internal class MinerWorker : BackgroundService
{
    private readonly ILogger<MinerWorker> _logger;
    private readonly ICommunication _communication;
    private readonly ChannelReader<string> _reader;
    // make a thread safe write this veritable to track the 
    // latest variable and on change cancel the current task
    private readonly string _latestHash;

    public MinerWorker(ILogger<MinerWorker> logger, ICommunication _communication, ChannelReader<string> reader)
    {
        this._logger = logger;
        this._communication = _communication;
        this._reader = reader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogCritical("MinerWorker running at: {0}", DateTimeOffset.Now.Ticks);
            await Task.Delay(1000, stoppingToken);
        }
    }
}