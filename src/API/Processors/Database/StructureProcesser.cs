using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Processors.Database;
internal static class StructureProcesser
{
    public static async Task MigrationStructure(SQLiteConnection connection)
    {
        if (connection == null)
            throw new Exception("could not connect to database.");

        await connection.OpenAsync();

        using var transactionCommand = new SQLiteCommand("""
            CREATE TABLE IF NOT EXISTS [MemPool] (
                [Id] TEXT,
                [RawTransaction] Text
            );

            CREATE TABLE IF NOT EXISTS [ChainDB] (
                [Number] INTEGER,
                [Hash] TEXT,
                [ParentHash] TEXT,
                [Nonce] INTEGER,
                [Sha3Uncles] TEXT,
                [LogsBloom] TEXT,
                [TransactionsRoot] TEXT,
                [StateRoot] TEXT,
                [ReceiptsRoot] TEXT,
                [Miner] TEXT,
                [Difficulty] TEXT,
                [TotalDifficulty] TEXT,
                [ExtraData] TEXT,
                [Size] TEXT,
                [GasLimit] TEXT,
                [GasUsed] TEXT,
                [TimeStamp] TEXT,
                [TransactionsId] TEXT,
                [Uncles] TEXT
            );
        
            CREATE TABLE IF NOT EXISTS [Transaction](
                [Id] TEXT, 
                [Nonce] TEXT, 
                [GasPrice] TEXT, 
                [GasLimit] TEXT, 
                [To] TEXT,
                [Value] TEXT,
                [Data] TEXT,
                [V] TEXT,
                [R] TEXT,
                [S] TEXT,
                [RawTransaction] TEXT
            );
        """, connection);
        await transactionCommand.ExecuteNonQueryAsync();

        await connection.CloseAsync();
    }
}
