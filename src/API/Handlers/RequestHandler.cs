using API.Models;
using src.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Handlers;
internal static class RequestHandler
{
    public static ReadOnlySpan<byte> ProcessEthGetCode(string accountAddress, string targetBlock)
    {
        // process how ever see fit
        return "\"0x\""u8;
    }

    public static ReadOnlySpan<byte> ProcessEthEstimateGas(ref EstimateGas estimateGas)
    {
        return new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"\"0x{21000:x}\""));
    }

    public static ReadOnlySpan<byte> ProcessEthSendRawTransaction(ref Span<byte> requestContext, SQLiteConnection sqLiteConnection)
    {
        try
        {
            sqLiteConnection.Open();
            var memPoolTransaction = new MemPool(requestContext[1..^1]);

            using var processCommand = new SQLiteCommand("""
                insert into MemPool (Id, Nonce, GasPrice, GasLimit, "To", "Value", Data, V, R, S)
                values (@Id, @Nonce, @GasPrice, @GasLimit, @ToVal, @ValueVal, @Data, @V, @R, @S);
                """, sqLiteConnection);

            processCommand.Parameters.AddWithValue("@Id", memPoolTransaction.Identity);
            processCommand.Parameters.AddWithValue("@Nonce", memPoolTransaction.Nonce);
            processCommand.Parameters.AddWithValue("@GasPrice", memPoolTransaction.GasPrice);
            processCommand.Parameters.AddWithValue("@GasLimit", memPoolTransaction.GasLimit);
            processCommand.Parameters.AddWithValue("@ToVal", memPoolTransaction.To);
            processCommand.Parameters.AddWithValue("@ValueVal", memPoolTransaction.Value);
            processCommand.Parameters.AddWithValue("@Data", memPoolTransaction.Data);
            processCommand.Parameters.AddWithValue("@V", memPoolTransaction.V);
            processCommand.Parameters.AddWithValue("@R", memPoolTransaction.R);
            processCommand.Parameters.AddWithValue("@S", memPoolTransaction.S);
            var c = processCommand.ExecuteNonQuery();
            return new ReadOnlySpan<byte>(memPoolTransaction.IdentifierAsHex());
        }
        finally
        {
            sqLiteConnection.Close();
        }
    }
}
