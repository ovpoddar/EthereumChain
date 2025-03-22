// See https://aka.ms/new-console-template for more information
// check if chain is setup or not
//
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Core;
using Shared.Helpers;
using Shared.Processors.Communication;
using System.Data.Common;
using System.Data.SQLite;
using System.Reflection.PortableExecutable;
using System.Threading.Channels;

internal class MinerWorker : BackgroundService
{
    private readonly ILogger<MinerWorker> _logger;
    private readonly IApplicationCommunication _communication;
    private readonly SQLiteConnection _connection;
    private readonly ChannelReader<string> _reader;
    // make a thread safe write this veritable to track the 
    // latest variable and on change cancel the current task
    private string _latestHash;

    public MinerWorker(ILogger<MinerWorker> logger, IApplicationCommunication _communication, SQLiteConnection connection, ChannelReader<string> reader)
    {
        this._logger = logger;
        this._communication = _communication;
        _connection = connection;
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


    private static int _counter;

    private void ResetCheck()
    {
        _counter = 0;
    }

    private async Task<Transaction?> GetProfitableTransaction()
    {
        try
        {
            await _connection.OpenAsync();
            while (true)
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT Id, RawTransaction FROM MemPool LIMIT 10 OFFSET @SkippedValue";
                command.Parameters.AddWithValue("@SkippedValue", _counter);
                using var reader = await command.ExecuteReaderAsync();
                if (!reader.Read())
                    return null;

                var result = await GetTransactionFromList(reader);
                if (result != null)
                    return result;
                _counter += 10;
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    private static async Task<Transaction?> GetTransactionFromList(DbDataReader reader)
    {
        do
        {
            var memPool = new BaseTransaction(reader.GetGuid(0), reader.GetString(1));
            Transaction transaction = memPool;
            if (transaction.GasPrice.ToEtherBalance() > Setting.MinimumProfit)
            {
                return transaction;
            }
        }
        while (await reader.ReadAsync());
        return null;
    }
}