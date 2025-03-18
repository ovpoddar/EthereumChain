using Shared;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Processors.Database; 
public static class StructureProcessor
{
    // TODO: ADD REQUIERED ANNONTATIONS
    // TODO: TRY TO ADD RELATION MAPPING
    public static async Task MigrationStructure(SQLiteConnection connection)
    {
        if (connection == null)
            throw new Exception("could not connect to database.");

        await connection.OpenAsync();

        using var transactionCommand = new SQLiteCommand("""
            CREATE TABLE IF NOT EXISTS [MemPool] (
                [Id] TEXT NOT NULL,
                [RawTransaction] Text NOT NULL
            );

            CREATE TABLE IF NOT EXISTS [ChainDB] (
                [Number] INTEGER NOT NULL,
                [NumberToHex] TEXT NOT NULL,
                [Hash] TEXT NOT NULL,
                [ParentHash] TEXT NOT NULL,
                [Nonce] INTEGER NOT NULL,
                [Sha3Uncles] TEXT,
                [LogsBloom] TEXT,
                [TransactionsRoot] TEXT,
                [StateRoot] TEXT,
                [ReceiptsRoot] TEXT,
                [Miner] TEXT NOT NULL,
                [Difficulty] TEXT,
                [TotalDifficulty] TEXT,
                [ExtraData] TEXT,
                [Size] TEXT,
                [GasLimit] TEXT,
                [GasUsed] TEXT,
                [TimeStamp] TEXT NOT NULL,
                [TransactionsId] TEXT,
                [Uncles] TEXT
            );
        
            CREATE TABLE IF NOT EXISTS [Transaction](
                [Id] TEXT NOT NULL,
                [Nonce] TEXT NOT NULL,
                [GasPrice] TEXT,
                [GasLimit] TEXT, 
                [To] TEXT NOT NULL,
                [From] TEXT NOT NULL,
                [Value] TEXT,
                [Data] TEXT NOT NULL,
                [V] TEXT,
                [R] TEXT,
                [S] TEXT,
                [RawTransaction] TEXT NOT NULL,
                [TransactionIndex] TEXT,
                [BlockNumber] INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS [Accounts](
                [OrderIndex] INTEGER PRIMARY KEY AUTOINCREMENT,
                [WalletId] TEXT NOT NULL,
                [NormalizeWalletId] TEXT NOT NULL,
                [Amount] TEXT NOT NULL,
                [BlockNumber] INTEGER NOT NULL,
                [TransactionId] TEXT NOT NULL
            );
        """, connection);
        await transactionCommand.ExecuteNonQueryAsync();

        await connection.CloseAsync();
    }


    public static SQLiteConnection InitializedDatabase()
    {
        var file = Setting.EthereumChainStoragePath.EnsureEndsWith(".sqlite", StringComparison.OrdinalIgnoreCase);
        if (!File.Exists(file))
            SQLiteConnection.CreateFile(file);
        return new SQLiteConnection($"Data Source={file};Version=3;");
    }
}
