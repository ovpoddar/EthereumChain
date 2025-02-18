using Nethereum.ABI.Util;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Core;

public class Block
{
    public int Number { get; set; }
    public string NumberToHex { get => $"0x{Number:x}"; }
    public string Hash { get; set; }
    public string ParentHash { get; set; }
    public long Nonce { get; set; }
    public required string Sha3Uncles { get; set; }
    public required string LogsBloom { get; set; }
    public required string TransactionsRoot { get; set; }
    public required string StateRoot { get; set; }
    public required string ReceiptsRoot { get; set; }
    public string Miner { get; }
    public required string Difficulty { get; set; }
    public required string TotalDifficulty { get; set; }
    public required string ExtraData { get; set; }
    public required string Size { get; set; }
    public ulong GasLimit { get; set; }
    public ulong GasUsed { get; set; }
    public string TimeStamp { get; }
    public List<Transaction> Transactions { get; }
    public string[] Uncles { get; set; }

    public Block(string minerAddress, string parentHash)
    {
        Miner = minerAddress;
        TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Transactions = [];
        ParentHash = parentHash;
        Nonce = 0;
        Uncles = [];
        Hash = CalculateHash();
    }

    public Block(byte[] block)
    {

    }

    public string CalculateHash()
    {
        var rawData = $"{NumberToHex} {Hash} {ParentHash} {Nonce} {Sha3Uncles} {LogsBloom} {TransactionsRoot} {StateRoot} {ReceiptsRoot} {Miner} {Difficulty} {TotalDifficulty} {ExtraData} {Size} {GasLimit} {GasUsed} {TimeStamp} {string.Join(' ', Transactions.OrderBy(a => a._transactionIndex).Select(a => a.RawTransaction))} {string.Join(' ', Uncles)}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(bytes).Replace("-", "");
    }

}
