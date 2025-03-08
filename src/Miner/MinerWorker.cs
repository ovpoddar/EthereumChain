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
    private readonly IApplicationCommunication _communication;
    private readonly ChannelReader<string> _reader;
    // make a thread safe write this veritable to track the 
    // latest variable and on change cancel the current task
    private string _latestHash;

    public MinerWorker(ILogger<MinerWorker> logger, IApplicationCommunication _communication, ChannelReader<string> reader)
    {
        this._logger = logger;
        this._communication = _communication;
        this._reader = reader;
        Task.Run(async () =>
        {
            _latestHash = await _reader.ReadAsync();
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // implement the core for processing the block chain
            // the work to calculate the hash and added to the block chain 
            // and publish it to network
            _logger.LogCritical("MinerWorker running at: {0}", DateTimeOffset.Now.Ticks);
            await Task.Delay(1000, stoppingToken);
        }
    }
}