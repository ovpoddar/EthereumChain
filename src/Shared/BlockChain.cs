using Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public class BlockChain
{
    private readonly SQLiteConnection _connection;

    public BlockChain(SQLiteConnection connection) => 
        _connection = connection ?? throw new NullReferenceException();

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
                    INSERT INTO [Transaction] ([Id], [Nonce], [GasPrice], [GasLimit], [To], [Value],
                        [Data], [V], [R], [S], [RawTransaction], [ChainDBId])
                    VALUES (@Id, @Nonce, @GasPrice, @GasLimit, @To, @Value,
                        @Data, @V, @R, @S, @RawTransaction, @ChainDBId);
                """, _connection, transaction);
                transactionCommand.Parameters.AddWithValue("@Id", blockTransaction.Id);
                transactionCommand.Parameters.AddWithValue("@Nonce", blockTransaction.Nonce);
                transactionCommand.Parameters.AddWithValue("@GasPrice", blockTransaction.GasPrice);
                transactionCommand.Parameters.AddWithValue("@GasLimit", blockTransaction.GasLimit);
                transactionCommand.Parameters.AddWithValue("@To", blockTransaction.To);
                transactionCommand.Parameters.AddWithValue("@Value", blockTransaction.Value);
                transactionCommand.Parameters.AddWithValue("@Data", blockTransaction.Data);
                transactionCommand.Parameters.AddWithValue("@V", blockTransaction.V);
                transactionCommand.Parameters.AddWithValue("@R", blockTransaction.R);
                transactionCommand.Parameters.AddWithValue("@S", blockTransaction.S);
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
