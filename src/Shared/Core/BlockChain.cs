using Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core;
public class BlockChain
{
    private readonly SQLiteConnection _connection;

    public BlockChain(SQLiteConnection connection) =>
        _connection = connection ?? throw new NullReferenceException();

    /// <summary>
    /// Add block to chain
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    /// <exception cref="Exception">throw a exception if it failed to put items in database.</exception>
    public async Task AddBlock(Block block)
    {
        await _connection.OpenAsync();
        using var transaction = _connection.BeginTransaction();
        try
        {
            using var chainCommand = new SQLiteCommand("""
                INSERT INTO [ChainDB] ([Number], [Hash], [ParentHash], [Nonce], [Sha3Uncles], [LogsBloom],
                    [TransactionsRoot], [StateRoot], [ReceiptsRoot], [Miner], [Difficulty], [TotalDifficulty],
                    [ExtraData], [Size], [GasLimit], [GasUsed], [TimeStamp], [TransactionsId], [Uncles])
                VALUES (@Number, @Hash, @ParentHash, @Nonce, @Sha3Uncles, @LogsBloom,
                    @TransactionsRoot, @StateRoot, @ReceiptsRoot, @Miner, @Difficulty, @TotalDifficulty,
                    @ExtraData, @Size, @GasLimit, @GasUsed, @TimeStamp, @TransactionsId, @Uncles);
            """, _connection, transaction);
            await chainCommand.ExecuteNonQueryAsync();

            foreach (var blockTransaction in block.Transactions)
            {
                var transactionCommand = new SQLiteCommand("""
                    INSERT INTO [Transaction] ([Id], [RawTransaction], [BlockNumber])
                    VALUES (@Id, @RawTransaction, @ChainDBId);
                """, _connection, transaction);
                transactionCommand.Parameters.AddWithValue("@Id", blockTransaction.Id);
                transactionCommand.Parameters.AddWithValue("@RawTransaction", blockTransaction.RawTransaction);
                transactionCommand.Parameters.AddWithValue("@ChainDBId", block.Number);

                await transactionCommand.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new Exception(ex.Message);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
}
