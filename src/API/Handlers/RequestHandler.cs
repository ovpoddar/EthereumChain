using API.Helpers;
using API.Models;
using Shared.Core;
using Shared.Helpers;
using Shared.Models;
using Shared.Processors.Communication;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

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
                "earliest" => "SELECT count(*) FROM [Transaction] WHERE BlockNumber = (SELECT [NumberToHex] FROM [ChainDB] ORDER BY [Number] ASC) AND [From] = @sender LIMIT 1",
                "latest" => "SELECT count(*) FROM [Transaction] WHERE BlockNumber = (SELECT [NumberToHex] FROM [ChainDB] ORDER BY [Number] DESC) AND [From] = @sender LIMIT 1",
                _ => "SELECT COUNT(*) FROM [Transaction] WHERE BlockNumber = (SELECT MAX(BlockNumber) FROM [Transaction]) AND [From] = @sender",
            }, sqLiteConnection);

            fetchCommand.Parameters.AddWithValue("@sender", accountAddress);
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
            return new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"\"{transaction.TransactionId}\""));
        }
        finally
        {
            sqLiteConnection.Close();
        }
    }

    internal static void ProcessGeneratedBlock(ref Span<byte> data, IApplicationCommunication communication)
    {
        var baseBlock = new BaseBlock(data);
        MinerEvents.RaisedMinerEvent(MinerEventsTypes.BlockGenerated, baseBlock);

        using var sendingDataWithContext = new ArrayPoolUsing<byte>(data.Length + 1);
        sendingDataWithContext[0] = (byte)CommunicationDataType.BaseBlock;
        data.CopyTo(sendingDataWithContext);
        communication.SendData(sendingDataWithContext);
    }

    internal static void ProcessEthGetBlockByNumber(string tag, bool fullData, bool isHash, SQLiteConnection sqLiteConnection, Utf8JsonWriter writer)
    {
        try
        {
            sqLiteConnection.Open();
            using var command = BuildCommandToGetByNumber(sqLiteConnection, tag, isHash);
            using var reader = command.ExecuteReader();
            writer.WriteStartObject();

            if (reader.Read())
            {
                var numberHex = reader.GetString(0);
                writer.WriteString("numberHex", numberHex);
                var blockHash = reader.GetString(1);
                writer.WriteString("hash", blockHash);
                writer.WriteString("parentHash", reader.GetString(2));
                writer.WriteNumber("nonce", reader.GetInt32(3));
                writer.WriteString("sha3Uncles", reader.GetString(4));
                writer.WriteString("logsBloom", reader.GetString(5));
                writer.WriteString("transactionsRoot", reader.GetString(6));
                writer.WriteString("stateRoot", reader.GetString(7));
                writer.WriteString("receiptsRoot", reader.GetString(8));
                writer.WriteString("miner", reader.GetString(9));
                writer.WriteString("difficulty", reader.GetString(10));
                writer.WriteString("totalDifficulty", reader.GetString(11));
                writer.WriteString("extraData", reader.GetString(12));
                writer.WriteString("size", reader.GetString(13));
                writer.WriteString("gasLimit", reader.GetString(14));
                writer.WriteString("gasUsed", reader.GetString(15));
                writer.WriteString("timestamp", reader.GetString(16));
                writer.WriteString("uncles", reader.GetString(17));
                var number = reader.GetInt32(18);

                using var transactionData = new SQLiteCommand(fullData
                    ? "SELECT [Nonce], [GasPrice], [GasLimit], [To], [From], [Value], [Data], [V], [R], [S], [RawTransaction], [TransactionIndex], [BlockNumber] FROM [Transaction] WHERE [BlockNumber] = @NUMBER"
                    : "SELECT [RawTransaction] FROM [Transaction] WHERE [BlockNumber] = @NUMBER", sqLiteConnection);
                transactionData.Parameters.AddWithValue("@NUMBER", number);
                using var transactionReader = transactionData.ExecuteReader();
                writer.WriteStartArray("transactions");
                if (!fullData)
                    while (transactionReader.Read())
                        writer.WriteStringValue(transactionReader.GetString(0));
                else
                {
                    while (transactionReader.Read())
                    {
                        writer.WriteStartObject();
                        writer.WriteString("blockHash", blockHash);
                        writer.WriteString("blockNumber", numberHex);
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
                        writer.WriteEndObject();
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

    private static SQLiteCommand BuildCommandToGetByNumber(SQLiteConnection sqLiteConnection, string tag, bool isHash)
    {
        var command = sqLiteConnection.CreateCommand();
        var sb = new StringBuilder();
        sb.Append("SELECT [NumberToHex], [Hash], [ParentHash], [Nonce], [Sha3Uncles], [LogsBloom], [TransactionsRoot], [StateRoot], [ReceiptsRoot], [Miner], [Difficulty], [TotalDifficulty], [ExtraData], [Size], [GasLimit], [GasUsed], [TimeStamp], [Uncles], [Number] FROM [ChainDB] WHERE ");
        sb.Append(isHash ? "[Hash] = " : "[NumberToHex] = ");
        sb.Append(tag.ToLower() switch
        {
            "earliest" => "(SELECT [NumberToHex] FROM [ChainDB] ORDER BY [Number] ASC LIMIT 1)",
            "latest" or "pending" => "(SELECT [NumberToHex] FROM [ChainDB] ORDER BY [Number] DESC LIMIT 1)",
            _ => "@Number"
        });
        command.CommandText = sb.ToString();
        if (tag.StartsWith("0x"))
            command.Parameters.AddWithValue("@Number", tag);
        return command;
    }

    internal static ReadOnlySpan<byte> ProcessEthBlockNumber(SQLiteConnection sqLiteConnection)
    {
        try
        {
            sqLiteConnection.Open();
            using var command = sqLiteConnection.CreateCommand();
            command.CommandText = "SELECT [NumberToHex] FROM [ChainDB] ORDER BY [Number] DESC LIMIT 1";
            using var reader = command.ExecuteReader();
            if (reader.Read())
                return new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"\"{reader.GetString(0)}\""));
            return [];
        }
        finally
        {
            sqLiteConnection.Close();
        }
    }

    internal static ReadOnlySpan<byte> ProcessEthGetBalance(string walletAddress, string blockNumber, SQLiteConnection sqLiteConnection)
    {
        try
        {
            sqLiteConnection.Open();
            using var command = BuildCommandToGetBalance(sqLiteConnection, walletAddress, blockNumber);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var amount = reader.GetDecimal(0).ConvertToWei();
                return new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"\"{amount:x}\""));
            }
            return "\"0x0\""u8;
        }
        finally
        {
            sqLiteConnection.Close();
        }
    }

    private static SQLiteCommand BuildCommandToGetBalance(SQLiteConnection sqLiteConnection, string walletAddress, string blockNumber)
    {
        var command = sqLiteConnection.CreateCommand();
        command.CommandText = """
            SELECT [Amount] 
            FROM [ChainDB] AS C 
            LEFT JOIN [Accounts] AS A 
            ON C.Number = A.BlockNumber
            WHERE A.WalletId = @WalletAddress 
            AND C.Number <= @BlockNumber 
            ORDER BY A.OrderIndex DESC 
            LIMIT 1            
        """;
        command.Parameters.AddWithValue("@WalletAddress", walletAddress);
        command.Parameters.AddWithValue("@BlockNumber", blockNumber switch
        {
            "earliest" => "(SELECT [Number] FROM [ChainDB] ORDER BY [Number] ASC LIMIT 1)",
            "latest" or "pending" => "(SELECT [Number] FROM [ChainDB] ORDER BY [Number] DESC LIMIT 1)",
            _ => int.Parse(blockNumber.EnsureNotStartsWith("0x"), NumberStyles.HexNumber)
        });
        return command;
    }
}
