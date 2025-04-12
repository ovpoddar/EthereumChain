using Nethereum.ABI.Util;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Core;

public class Block : BaseInternalBlock
{
    public string NumberToHex { get => $"0x{Number:x}"; }
    public string TimeStamp { get; init; }
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

    public string CalculateHash()
    {
        var rawData = $"{Number} {ParentHash} {Nonce} {Sha3Uncles} {LogsBloom} {TransactionsRoot} {StateRoot} {ReceiptsRoot} {Miner} {Difficulty} {TotalDifficulty} {ExtraData} {Size} {GasLimit} {GasUsed} {TimeStamp} {string.Join(' ', Transactions.OrderBy(a => a._transactionIndex).Select(a => a.RawTransaction))} {string.Join(' ', Uncles)}";
        return base.CalculateHash(rawData);
    }


    public static implicit operator Block(BaseBlock baseBlock)
    {
        var block = new Block(baseBlock.Miner, baseBlock.ParentHash)
        {
            Difficulty = baseBlock.Difficulty,
            LogsBloom = baseBlock.LogsBloom,
            ExtraData = baseBlock.ExtraData,
            GasLimit = baseBlock.GasLimit,
            GasUsed = baseBlock.GasUsed,
            Hash = baseBlock.Hash,
            Nonce = baseBlock.Nonce,
            Number = baseBlock.Number,
            ParentHash = baseBlock.ParentHash,
            ReceiptsRoot = baseBlock.ReceiptsRoot,
            Sha3Uncles = baseBlock.Sha3Uncles,
            Size = baseBlock.Size,
            StateRoot = baseBlock.StateRoot,
            TotalDifficulty = baseBlock.TotalDifficulty,
            Uncles = baseBlock.Uncles.Split(' '),
            TransactionsRoot = baseBlock.TransactionsRoot
        };
        for (var i = 0; i < baseBlock.Transactions.Count; i++)
        {
            var transaction = ((Transaction)baseBlock.Transactions[i]);
            transaction.SetTransactionIndex(i);
            block.Transactions.Add(transaction);
        }
        return block;
    }
}
