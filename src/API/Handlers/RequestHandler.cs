using API.Models;
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
            processCommand.Parameters.AddWithValue("@Id", transactionId);
            processCommand.Parameters.AddWithValue("@RawTransaction", Encoding.UTF8.GetString(requestContext[1..^1]));

            var response = processCommand.ExecuteNonQuery();
            Debug.Assert(response != 0);
            MinerEvents.RaisedEvent(MinerEventsTypes.TransactionAdded, new TransactionAddedEventArgs(transactionId, requestContext[1..^1].ToString()));
            return new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(transactionId.ToString()));
        }
        finally
        {
            sqLiteConnection.Close();
        }
    }
}
