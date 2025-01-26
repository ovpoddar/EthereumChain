using System.Security.Cryptography;
using System.Text;

namespace Shared.Core;

public class Block
{
    public int Number { get; set; }
    public string Hash { get; set; }
    public string ParentHash { get; set; }
    public long Nonce { get; set; }
    public string Sha3Uncles { get; set; }
    public string LogsBloom { get; set; }
    public string TransactionsRoot { get; set; }
    public string StateRoot { get; set; }
    public string ReceiptsRoot { get; set; }
    public string Miner { get; }
    public string Difficulty { get; set; }
    public string TotalDifficulty { get; set; }
    public string ExtraData { get; set; }
    public string Size { get; set; }
    public string GasLimit { get; set; }
    public string GasUsed { get; set; }
    public string TimeStamp { get; }
    public List<BaseTransaction> Transactions { get; }
    public string[] Uncles { get; set; }

    public Block(string minerAddress, string parentHash)
    {
        Miner = minerAddress;
        TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Transactions = [];
        ParentHash = parentHash;
        Nonce = 0;
    }

    public Block(byte[] block)
    {

    }
    public string CalculateHash()
    {
        var rawData = $"{Number} {Hash} {ParentHash} {Nonce} {Sha3Uncles} {LogsBloom} {TransactionsRoot} {StateRoot} {ReceiptsRoot} {Miner} {Difficulty} {TotalDifficulty} {ExtraData} {Size} {GasLimit} {GasUsed} {TimeStamp} {string.Join(' ', Transactions.Select(a => a.RawTransaction))} {string.Join(' ', Uncles)}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(bytes).Replace("-", "");
    }

}
