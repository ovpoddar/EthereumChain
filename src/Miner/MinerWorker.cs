// See https://aka.ms/new-console-template for more information
// check if chain is setup or not
//
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Core;
using Shared.Helpers;
using Shared.Models;
using Shared.Processors.Communication;
using System.Buffers;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Channels;

internal class MinerWorker : BackgroundService
{
    private readonly ILogger<MinerWorker> _logger;
    private readonly IApplicationCommunication _communication;
    private readonly SQLiteConnection _connection;
    private readonly ChannelReader<string> _reader;
    private CancellationTokenSource _cancellationTokenSource;
    // make a thread safe write this veritable to track the 
    // latest variable and on change cancel the current task
    private string _latestHash;

    public MinerWorker(ILogger<MinerWorker> logger, IApplicationCommunication _communication, SQLiteConnection connection, ChannelReader<string> reader)
    {
        _cancellationTokenSource = new();
        this._logger = logger;
        this._communication = _communication;
        _connection = connection;
        this._reader = reader;
        Task.Run(async () =>
        {
            _cancellationTokenSource.Cancel();
            _latestHash = await _reader.ReadAsync();
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var usableThreads = Setting.ThreadUsage == 0
            ? Environment.ProcessorCount
            : Setting.ThreadUsage < 0
                ? Environment.ProcessorCount + Setting.ThreadUsage
                : Setting.ThreadUsage;
        var workingSet = new List<Transaction>();
        var threads = new Thread[usableThreads];
        while (!stoppingToken.IsCancellationRequested)
        {
            // todo: declare a difficulty level which should be collect from the network
            // base on it mine the block and verify the hash
            // todo: need a sleeping time to avoid the cpu usage when no transaction found
            BaseBlock? generatedBlock = null;
            var totalChunks = int.MaxValue / usableThreads;

            for (var i = 0; i < usableThreads; i++)
            {
                var workingRangeStart = i * totalChunks;
                var workingRangeEnd = workingRangeStart + totalChunks;
                workingSet.AddRange(await GetProfitableTransactions(10));
                threads[i] = new Thread(() =>
                {
                    var block = new Block(Setting.DefaultMinerAddress, _latestHash)
                    {
                        Sha3Uncles = string.Empty,
                        LogsBloom = string.Empty,
                        TransactionsRoot = string.Empty,
                        StateRoot = string.Empty,
                        ReceiptsRoot = string.Empty,
                        Difficulty = string.Empty,
                        TotalDifficulty = string.Empty,
                        ExtraData = string.Empty,
                        Size = string.Empty
                    };
                    block.Transactions.AddRange(workingSet);
                    for (; workingRangeStart < workingRangeEnd; workingRangeStart++)
                    {
                        block.Nonce = workingRangeStart;
                        var hash = block.CalculateHash();
                        if (_cancellationTokenSource.IsCancellationRequested)
                            break;

                        if (Setting.VerifyBlockExecution(hash))
                        {
                            _cancellationTokenSource.Cancel();
                            generatedBlock = block;
                            break;
                        }
                    }
                });
                threads[i].Start();
            }

            foreach (var thread in threads)
                thread.Join();

            if (generatedBlock is not null)
            {
                var blockToArray = generatedBlock.ToByteArray();
                var request = ArrayPool<byte>.Shared.Rent(blockToArray.Length + 1);
                request[0] = (byte)CommunicationDataType.BaseBlock;
                blockToArray.CopyTo(request, 1);
                _communication.SendData(request);
                ResetCheck();
                Thread.Sleep(1000);
            }
        }
    }


    private static int _counter;

    private void ResetCheck()
    {
        _counter = 0;
        _cancellationTokenSource = new();
    }

    private async Task<Transaction[]> GetProfitableTransactions(int count)
    {
        var transactions = new Transaction[count];
        var index = 0;
        while (index <= count)
        {
            var transaction = await GetProfitableTransaction();
            if (transaction == null)
                break;
            transactions[index] = transaction;
        }
        return transactions;
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