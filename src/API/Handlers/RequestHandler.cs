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
            // todo: need to work on query for tag
            using var command = new SQLiteCommand("SELECT [ChainDB] ([Number], [Hash], [ParentHash], [Nonce], [Sha3Uncles], [LogsBloom], [TransactionsRoot], [StateRoot], [ReceiptsRoot], [Miner], [Difficulty], [TotalDifficulty], [ExtraData], [Size], [GasLimit], [GasUsed], [TimeStamp], [Uncles] FROM [ChainDB] WHERE [Number] = @Number", sqLiteConnection);
            command.Parameters.AddWithValue("@Number", tag);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                writer.WriteStartObject();
                //"number": "0x1b4",
                writer.WriteString("number", $"0x{reader.GetInt32(0):x}");
                //"difficulty": "0x4ea3f27bc",
                writer.WriteString("difficulty", reader.GetString(10));
                //"extraData": "0x476574682f4c5649562f76312e302e302f6c696e75782f676f312e342e32",
                //"gasLimit": "0x1388",
                //"gasUsed": "0x0",
                //"hash": "0xdc0818cf78f21a8e70579cb46a43643f78291264dda342ae31049421c82d21ae",
                //"logsBloom": "0x00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
                //"miner": "0xbb7b8287f3f0a933474a79eae42cbca977791171",
                //"mixHash": "0x4fffe9ae21f1c9e15207b1f472d5bbdd68c9595d461666602f2be20daf5e7843",
                //"nonce": "0x689056015818adbe",
                //"parentHash": "0xe99e022112df268087ea7eafaf4790497fd21dbeeb6bd7a1721df161a6657a54",
                //"receiptsRoot": "0x56e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421",
                //"sha3Uncles": "0x1dcc4de8dec75d7aab85b567b6ccd41ad312451b948a7413f0a142fd40d49347",
                //"size": "0x220",
                //"stateRoot": "0xddc8b0234c2e0cad087c8b389aa7ef01f7d79b2570bccb77ce48648aa61c904d",
                //"timestamp": "0x55ba467c",
                //"totalDifficulty": "0x78ed983323d",
                //"transactions": [],
                //"transactionsRoot": "0x56e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421",
                //"uncles": []


                using var transactionData = new SQLiteCommand(fullData
                    ? "SELECT [Id], [Nonce], [GasPrice], [GasLimit], [To], [From], [Value], [Data], [V], [R], [S], [RawTransaction], [BlockNumber] FROM [Transaction] WHERE [BlockNumber] = @NUMBER"
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
                        // todo: finish this.
                        //blockHash: "0xb68d48c458864ea976066e530afb1e0a75dfa06c28aee28a4ccfe2e49a9fcb17",
                        writer.WriteString("blockHash", "comming from first reader. hash value");
                        //blockNumber: "0x14dac9b",
                        writer.WriteString("blockNumber", "comming from first reader. number value");
                        writer.WriteString("hash", transactionReader.GetString(12));
                        //yParity: "0x0",
                        //accessList: [],
                        //transactionIndex: "0x0",
                        writer.WriteString("transactionIndex", "WIP");
                        //type: "0x2",

                        writer.WriteString("nonce", transactionReader.GetString(1));
                        //input: "0x2b2f5d83beeb58351b598e7a017dace2534bc3f7496124c89425b1e1650d939e1db08e870d67f660d95d5be530380d0ec0bd388289e1b6ca978a0528116dda3cba9acd3e68bc6191ca53d05b0d939e1d7b0d93b300000100b0f939e0a03fb07f59a73314e73794be0e57ac1b4eb84ebdf703948ddcea3b11f675b4d1fba9d2414a145b0d93b3227c01440b0000010066c7bbec68d12a0d1830360f8ec58fa599ba1b0e9b5f10fc855bbc170f0000c02aaa39b223fe8d0a0e5c4f27ead9083c756cc2dac17f958d2ee523a2206206994597c13d831ec7006406",
                        writer.WriteString("input", "WIP");
                        writer.WriteString("r", transactionReader.GetString(9));
                        writer.WriteString("r", transactionReader.GetString(10));
                        //chainId: "0x1",

                        writer.WriteString("v", transactionReader.GetString(8));
                        //gas: "0x93166",
                        //maxPriorityFeePerGas: "0x1",

                        writer.WriteString("from", transactionReader.GetString(5));
                        writer.WriteString("to", transactionReader.GetString(4));
                        writer.WriteString("to", transactionReader.GetString(3));
                        writer.WriteString("value", transactionReader.GetString(6));
                        writer.WriteString("gasPrice", transactionReader.GetString(2));
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
}
