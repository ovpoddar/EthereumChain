using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Processors.Database;
internal static class StructureProcesser
{
    public static async Task MigrationStructure(SQLiteConnection connection)
    {
        if (connection == null)
            throw new Exception("could not connect to database.");

        await connection.OpenAsync();

        await using var pendingTransactionCommand = new SQLiteCommand("""
            CREATE TABLE IF NOT EXISTS PendingTransaction (
            [Hash] VARCHAR(256),
            [Nonce] VARCHAR(256),
            [BlockHash] VARCHAR(256),
            [BlockNumber] VARCHAR(256),
            [TransactionIndex] VARCHAR(256),
            [From] VARCHAR(256),
            [To] VARCHAR(256),
            [Value] VARCHAR(256),
            [Gas] VARCHAR(256),
            [GasPrice] VARCHAR(256),
            [Input] VARCHAR(256)
        )
        """, connection);
        await pendingTransactionCommand.ExecuteNonQueryAsync();

        await connection.CloseAsync();
    }
}
