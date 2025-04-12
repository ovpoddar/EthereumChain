using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core;
public abstract class BaseInternalBlock
{
    public int Number { get; set; }
    public string Hash { get; set; }
    public string ParentHash { get; set; }
    public long Nonce { get; set; }
    public string Sha3Uncles { get; set; } = "";
    public string LogsBloom { get; set; } = "";
    public string TransactionsRoot { get; set; } = "";
    public string StateRoot { get; set; } = "";
    public string ReceiptsRoot { get; set; } = "";
    public string Miner { get; init; }
    public string Difficulty { get; set; } = "";
    public string TotalDifficulty { get; set; } = "";
    public string ExtraData { get; set; } = "";
    public string Size { get; set; } = "";
    public ulong GasLimit { get; set; }
    public ulong GasUsed { get; set; }


    internal virtual string CalculateHash(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        var result = Convert.ToHexString(bytes);
        Hash = result;
        return result;
    }
}
