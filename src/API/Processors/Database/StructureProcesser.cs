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
                [Nonce] VARCHAR(100),
                [GasPrice] INTEGER,
                [GasLimit] INTEGER,
                [To] VARCHAR(250),
                [Value] INTEGER,
                [Data] VARCHAR(1000),
                [V] VARCHAR(50),
                [R] VARCHAR(50),
                [S] VARCHAR(50)
            )
        """, connection);
        await transactionCommand.ExecuteNonQueryAsync();

        await connection.CloseAsync();
    }
}
