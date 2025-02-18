using API.Models;
using Shared.Processors.Communication;
using System.Text.Json;
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
    internal static ReadOnlySpan<byte> ProcessEthGetCode(string accountAddress, string targetBlock)
    {
        // process how ever see fit
        return "\"0x\""u8;
    }

    internal static ReadOnlySpan<byte> ProcessEthEstimateGas(ref EstimateGas estimateGas)
    {
        return new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"\"0x{21000:x}\""));
    }


    // pending -        for the pending state/transactions,
    // safe -           for the most recent secure block,
    // finalized -      for the most recent secure block accepted by more than 2/3 of validators
    internal static ReadOnlySpan<byte> ProcessEthGetTransactionCount(string accountAddress, string tag, SQLiteConnection sqLiteConnection)
    {
        try
        {
            sqLiteConnection.Open();
            using var fetchCommand = new SQLiteCommand(tag switch
            {
                "earliest" => "SELECT count(*) FROM [Transaction] WHERE BlockNumber = 0 AND [From] = @sender",
                _ => "SELECT COUNT(*) FROM [Transaction] WHERE BlockNumber = (SELECT MAX(BlockNumber) FROM [Transaction]) AND [From] = @sender",
            }, sqLiteConnection);

            fetchCommand.Parameters.AddWithValue("@blockNumber", accountAddress);
            using var reader = fetchCommand.ExecuteReader();
            reader.Read();
            var result = reader.GetInt32(0);
            return Encoding.UTF8.GetBytes($"\"0x{result:x}\"");
        }
        finally
        {
            sqLiteConnection.Close();
        }
    }

    internal static ReadOnlySpan<byte> ProcessEthSendTransaction(ref Span<byte> requestContext, SQLiteConnection sqLiteConnection)
    {
        var transaction = new BaseTransaction(Guid.NewGuid(), Encoding.UTF8.GetString(requestContext[1..^1]));
        return ProcessEthSendRawTransaction(sqLiteConnection, transaction);
    }

    internal static ReadOnlySpan<byte> ProcessEthSendRawTransaction(ref Span<byte> requestContext, SQLiteConnection sqLiteConnection)
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

    internal static void ProcessEthGetBlockByNumber(string tag, bool fullData, SQLiteConnection sqLiteConnection, Utf8JsonWriter writer)
    {
        try
        {
            sqLiteConnection.Open();
            using var command = BuildCommand(sqLiteConnection, tag);
            command.Parameters.AddWithValue("@Number", tag);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                writer.WriteStartObject();
                var number = reader.GetString(0);
                writer.WriteString("number", number);
                writer.WriteString("difficulty", reader.GetString(10));
                writer.WriteString("extraData", reader.GetString(12));
                writer.WriteString("gasLimit", reader.GetString(14));
                writer.WriteString("gasUsed", reader.GetString(15));
                var blockHash = reader.GetString(1);
                writer.WriteString("hash", blockHash);
                writer.WriteString("logsBloom", reader.GetString(5));
                writer.WriteString("miner", reader.GetString(9));
                writer.WriteString("nonce", reader.GetString(3));
                writer.WriteString("parentHash", reader.GetString(2));
                writer.WriteString("receiptsRoot", reader.GetString(8));
                writer.WriteString("sha3Uncles", reader.GetString(5));
                writer.WriteString("size", reader.GetString(13));
                writer.WriteString("stateRoot", reader.GetString(7));
                writer.WriteString("timestamp", reader.GetString(16));
                writer.WriteString("totalDifficulty", reader.GetString(11));
                writer.WriteString("transactionsRoot", reader.GetString(6));
                writer.WriteString("uncles", reader.GetString(17));


                using var transactionData = new SQLiteCommand(fullData
                    ? "SELECT [Nonce], [GasPrice], [GasLimit], [To], [From], [Value], [Data], [V], [R], [S], [RawTransaction], [TransactionIndex], [BlockNumber] FROM [Transaction] WHERE [BlockNumber] = @NUMBER"
                    : "SELECT [RawTransaction] FROM [Transaction] WHERE [BlockNumber] = @NUMBER", sqLiteConnection);
                using var transactionReader = transactionData.ExecuteReader();
                writer.WriteStartArray("transactions");
                if (!fullData)
                    while (transactionReader.Read())
                        writer.WriteStringValue(transactionReader.GetString(0));
                else
                {
                    while (transactionReader.Read())
                    {
                        writer.WriteString("blockHash", blockHash);
                        writer.WriteString("blockNumber", number);
                        writer.WriteString("hash", transactionReader.GetString(10));
                        writer.WriteString("transactionIndex", transactionReader.GetString(11));
                        writer.WriteString("nonce", transactionReader.GetString(0));
                        writer.WriteString("r", transactionReader.GetString(8));
                        writer.WriteString("s", transactionReader.GetString(9));
                        writer.WriteString("v", transactionReader.GetString(7));
                        writer.WriteString("from", transactionReader.GetString(4));
                        writer.WriteString("to", transactionReader.GetString(3));
                        writer.WriteString("value", transactionReader.GetString(5));
                        writer.WriteString("gas", transactionReader.GetString(2));
                        writer.WriteString("gasPrice", transactionReader.GetString(1));
                    }
                }
                writer.WriteEndArray();
            }
        }
        finally
        {
            sqLiteConnection.Close();
            writer.WriteEndObject();
            writer.Flush();
        }
    }

    private static SQLiteCommand BuildCommand(SQLiteConnection sqLiteConnection, string tag)
    {
        var command = sqLiteConnection.CreateCommand();
        StringBuilder sb = new StringBuilder();
        sb.Append("SELECT [ChainDB] ([NumberToHex], [Hash], [ParentHash], [Nonce], [Sha3Uncles], [LogsBloom], [TransactionsRoot], [StateRoot], [ReceiptsRoot], [Miner], [Difficulty], [TotalDifficulty], [ExtraData], [Size], [GasLimit], [GasUsed], [TimeStamp], [Uncles] FROM [ChainDB] WHERE [NumberToHex] = ");
        sb.Append(tag.ToLower() switch
        {
            "earliest" => "0x0",
            "latest" or "pending" => "(SELECT COUNT(*) FROM ChainDB)",
            _ => "@Number"
        });
        command.CommandText = sb.ToString();
        if (tag.StartsWith("0x"))
            command.Parameters.AddWithValue("@Number", tag);
        return command;
    }
}
