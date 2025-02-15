using API.Models;
using Shared.Processors.Communication;
using API.Processors.WebSocket;
using Nethereum.Merkle.Patricia;
using Newtonsoft.Json.Linq;
using Shared.Core;
using Shared.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using API.Helpers;

namespace API.Handlers;
internal static class RequestHandler
{
    public static ReadOnlySpan<byte> ProcessEthGetCode(string accountAddress, string targetBlock)
    {
        // process how ever see fit
        return "\"0x\""u8;
    }

    public static ReadOnlySpan<byte> ProcessEthEstimateGas(ref EstimateGas estimateGas)
    {
        return new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"\"0x{21000:x}\""));
    }


    // earliest -       For the earliest/genesis block,
    // latest -         for the latest mined block,
    // pending -        for the pending state/transactions,
    // safe -           for the most recent secure block,
    // finalized -      for the most recent secure block accepted by more than 2/3 of validators
    public static ReadOnlySpan<byte> ProcessEthGetTransactionCount(string accountAddress, string tag, SQLiteConnection sqLiteConnection)
    {
        try
        {
            sqLiteConnection.Open();
            var query = tag switch
            {
                "earliest" => "SELECT COUNT(*) FROM [Transaction] WHERE BlockNumber = 0",
                "finalized" or "safe" or "pending" or "latest" => "SELECT COUNT(*) FROM [Transaction] WHERE BlockNumber = (SELECT MAX(BlockNumber) FROM [Transaction])",
                _ => ""
            };
            using var fetchCommand = new SQLiteCommand(query, sqLiteConnection);
            fetchCommand.ExecuteNonQuery();
            return [];
        }
        finally
        {
            sqLiteConnection.Close();
        }
    }

    public static ReadOnlySpan<byte> ProcessEthSendTransaction(ref Span<byte> requestContext, SQLiteConnection sqLiteConnection)
    {
        var transaction = new BaseTransaction(Guid.NewGuid(), Encoding.UTF8.GetString(requestContext[1..^1]));
        return ProcessEthSendRawTransaction(sqLiteConnection, transaction);
    }

    public static ReadOnlySpan<byte> ProcessEthSendRawTransaction(ref Span<byte> requestContext, SQLiteConnection sqLiteConnection)
    {
        var transaction = new BaseTransaction(requestContext);
        return ProcessEthSendRawTransaction(sqLiteConnection, transaction);
    }

    private static ReadOnlySpan<byte> ProcessEthSendRawTransaction(SQLiteConnection sqLiteConnection, BaseTransaction transaction)
    {
        try
        {
            sqLiteConnection.Open();
            using var processCommand = new SQLiteCommand("""
                insert into MemPool (Id, RawTransaction)
                values (@Id, @RawTransaction);
            """, sqLiteConnection);
            processCommand.Parameters.AddWithValue("@Id", transaction.TransactionId);
            processCommand.Parameters.AddWithValue("@RawTransaction", transaction.RawTransaction);
            var response = processCommand.ExecuteNonQuery();
            Debug.Assert(response != 0);
            MinerEvents.RaisedMinerEvent(MinerEventsTypes.TransactionAdded, transaction);
            return new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(transaction.TransactionId.ToString()));
        }
        finally
        {
            sqLiteConnection.Close();
        }
    }

    internal static void ProcessGeneratedBlock(ref Span<byte> data, ICommunication communication)
    {
        var baseBlock = new BaseBlock(data);
        MinerEvents.RaisedMinerEvent(MinerEventsTypes.BlockGenerated, baseBlock);

        using var sendingDataWithContext = new ArrayPoolUsing<byte>(data.Length + 1);
        sendingDataWithContext[0] = (byte)CommunicationDataType.BaseBlock;
        data.CopyTo(sendingDataWithContext);
        communication.SendData(sendingDataWithContext);
    }
}
