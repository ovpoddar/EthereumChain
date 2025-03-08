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
                INSERT INTO [ChainDB] ([Number], [NumberToHex], [Hash], [ParentHash], [Nonce], [Sha3Uncles], [LogsBloom],
                    [TransactionsRoot], [StateRoot], [ReceiptsRoot], [Miner], [Difficulty], [TotalDifficulty],
                    [ExtraData], [Size], [GasLimit], [GasUsed], [TimeStamp], [Uncles])
                VALUES (@Number, @NumberToHex, @Hash, @ParentHash, @Nonce, @Sha3Uncles, @LogsBloom,
                    @TransactionsRoot, @StateRoot, @ReceiptsRoot, @Miner, @Difficulty, @TotalDifficulty,
                    @ExtraData, @Size, @GasLimit, @GasUsed, @TimeStamp, @Uncles);
            """, _connection, transaction);
            chainCommand.Parameters.AddWithValue("@Number", block.Number);
            chainCommand.Parameters.AddWithValue("@NumberToHex", block.NumberToHex);
            chainCommand.Parameters.AddWithValue("@Hash", block.Hash);
            chainCommand.Parameters.AddWithValue("@ParentHash", block.ParentHash);
            chainCommand.Parameters.AddWithValue("@Nonce", block.Nonce);
            chainCommand.Parameters.AddWithValue("@Sha3Uncles", block.Sha3Uncles);
            chainCommand.Parameters.AddWithValue("@LogsBloom", block.LogsBloom);
            chainCommand.Parameters.AddWithValue("@TransactionsRoot", block.TransactionsRoot);
            chainCommand.Parameters.AddWithValue("@StateRoot", block.StateRoot);
            chainCommand.Parameters.AddWithValue("@ReceiptsRoot", block.ReceiptsRoot);
            chainCommand.Parameters.AddWithValue("@Miner", block.Miner);
            chainCommand.Parameters.AddWithValue("@Difficulty", block.Difficulty);
            chainCommand.Parameters.AddWithValue("@TotalDifficulty", block.TotalDifficulty);
            chainCommand.Parameters.AddWithValue("@ExtraData", block.ExtraData);
            chainCommand.Parameters.AddWithValue("@Size", block.Size);
            chainCommand.Parameters.AddWithValue("@GasLimit", block.GasLimit);
            chainCommand.Parameters.AddWithValue("@GasUsed", block.GasUsed);
            chainCommand.Parameters.AddWithValue("@TimeStamp", block.TimeStamp);
            chainCommand.Parameters.AddWithValue("@Uncles", block.Uncles);
            await chainCommand.ExecuteNonQueryAsync();

            foreach (var blockTransaction in block.Transactions)
            {
                var transactionCommand = new SQLiteCommand("""
                    INSERT INTO [Transaction] ([Id], [Nonce], [GasPrice], [GasLimit], [To], [From], [Value],
                        [Data], [V], [R], [S], [RawTransaction], [TransactionIndex], [BlockNumber])
                    VALUES (@Id, @Nonce, @GasPrice, @GasLimit, @To, @From, @Value,
                        @Data, @V, @R, @S, @RawTransaction, @TransactionIndex, @BlockNumber);
                """, _connection, transaction);
                transactionCommand.Parameters.AddWithValue("@Id", blockTransaction.Id);
                transactionCommand.Parameters.AddWithValue("@Nonce", blockTransaction.Nonce);
                transactionCommand.Parameters.AddWithValue("@GasPrice", blockTransaction.GasPrice);
                transactionCommand.Parameters.AddWithValue("@GasLimit", blockTransaction.GasLimit);
                transactionCommand.Parameters.AddWithValue("@To", blockTransaction.To);
                transactionCommand.Parameters.AddWithValue("@From", blockTransaction.From);
                transactionCommand.Parameters.AddWithValue("@Value", blockTransaction.Value);
                transactionCommand.Parameters.AddWithValue("@Data", blockTransaction.Data);
                transactionCommand.Parameters.AddWithValue("@V", blockTransaction.V);
                transactionCommand.Parameters.AddWithValue("@R", blockTransaction.R);
                transactionCommand.Parameters.AddWithValue("@S", blockTransaction.S);
                transactionCommand.Parameters.AddWithValue("@RawTransaction", blockTransaction.RawTransaction);
                transactionCommand.Parameters.AddWithValue("@TransactionIndex", blockTransaction.TransactionIndex);
                transactionCommand.Parameters.AddWithValue("@BlockNumber", block.Number);

                await transactionCommand.ExecuteNonQueryAsync();

                // todo: add balance as transaction manner
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
