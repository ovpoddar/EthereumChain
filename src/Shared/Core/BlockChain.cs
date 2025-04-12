using Shared.Helpers;
using System.Data.SQLite;
using System.Globalization;
using System.Numerics;

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
    /// <returns>bool if the adding block is a invalid one</returns>
    public async Task<bool> AddBlock(Block block)
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

                var updateBalance = await UpdateTransaction(_connection,
                    transaction,
                    blockTransaction,
                    block.Number,
                    blockTransaction.Id);
                if (!updateBalance) throw new Exception("Failed to update balance");
            }
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            transaction.Rollback();
            return false;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    private static async Task<bool> UpdateTransaction(SQLiteConnection connection,
        SQLiteTransaction transaction,
        Transaction blockTransaction,
        int blockNumber,
        string transactionId)
    {
        var fromBalance = await GetBalance(connection, blockTransaction.From);
        var transactionAmount = BigInteger.Parse(blockTransaction.Value, NumberStyles.AllowHexSpecifier).ConvertToEtherAmount();
        if (fromBalance >= transactionAmount)
        {
            return false;
        }
        var toBalance = await GetBalance(connection, blockTransaction.To);

        using var command = new SQLiteCommand("""
            INSERT INTO [Accounts] (WalletId, NormalizeWalletId, Amount, BlockNumber, TransactionId)
            VALUES (@FormWalletId, @FormNormalizeWalletId, @FormAmount, @BlockNumber, @TransactionId);

            INSERT INTO [Accounts] (WalletId, NormalizeWalletId, Amount, BlockNumber, TransactionId)
            VALUES (@ToWalletId, @ToNormalizeWalletId, @ToAmount, @BlockNumber, @TransactionId);
        """, connection, transaction);

        command.Parameters.AddWithValue("@FormWalletId", blockTransaction.From);
        command.Parameters.AddWithValue("@FormNormalizeWalletId", blockTransaction.From.ToUpper());
        command.Parameters.AddWithValue("@FormAmount", fromBalance - transactionAmount);
        command.Parameters.AddWithValue("@ToWalletId", blockTransaction.To);
        command.Parameters.AddWithValue("@ToNormalizeWalletId", blockTransaction.To.ToUpper());
        command.Parameters.AddWithValue("@ToAmount", toBalance + transactionAmount);
        command.Parameters.AddWithValue("@BlockNumber", blockNumber);
        command.Parameters.AddWithValue("@TransactionId", transactionId);
        await command.ExecuteNonQueryAsync();
        return true;
    }

    private static async Task<decimal> GetBalance(SQLiteConnection connection, string address)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "select [Amount] from [Accounts] where NormalizeWalletId = @WalletAddressNormalize order by [OrderIndex] desc limit 1";
        command.Parameters.AddWithValue("@WalletAddressNormalize", address.ToUpper());
        using var reader = await command.ExecuteReaderAsync();
        return reader.Read()
            ? BigInteger.Parse(reader.GetString(0), NumberStyles.AllowHexSpecifier).ConvertToEtherAmount()
            : 0;
    }
}
