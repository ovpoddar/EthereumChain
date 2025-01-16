using API.Models;
using API.Processors.MinerEvents;
using API.Processors.WebSocket;
using Nethereum.Merkle.Patricia;
using Newtonsoft.Json.Linq;
using Shared;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public static ReadOnlySpan<byte> ProcessEthSendRawTransaction(ref Span<byte> requestContext, SQLiteConnection sqLiteConnection, MinerSocketProcessor webSocketListener)
    public static ReadOnlySpan<byte> ProcessEthSendRawTransaction(ref Span<byte> requestContext, SQLiteConnection sqLiteConnection)
    {
        try
        {
            sqLiteConnection.Open();

            using var processCommand = new SQLiteCommand("""
                insert into MemPool (Id, RawTransaction)
                values (@Id, @RawTransaction);
                """, sqLiteConnection);
            var transactionId = Guid.NewGuid();
            var transaction = Encoding.UTF8.GetString(requestContext[1..^1]);
            processCommand.Parameters.AddWithValue("@Id", transactionId);
            processCommand.Parameters.AddWithValue("@RawTransaction", transaction);

            var response = processCommand.ExecuteNonQuery();
            Debug.Assert(response != 0);
            MinerEvents.RaisedMinerEvent(MinerEventsTypes.TransactionAdded, new TransactionAddedEventArgs(transactionId, transaction));
            return new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(transactionId.ToString()));
        }
        finally
        {
            sqLiteConnection.Close();
        }
    }
}
