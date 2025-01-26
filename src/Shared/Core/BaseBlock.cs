using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core;
public class BaseBlock : MinerEventArgs
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
    public List<Transaction> Transactions { get; }
    public string Uncles { get; set; }


    public override ushort GetWrittenByteSize()
    {
        return (ushort)(31 
            + Encoding.UTF8.GetByteCount(Hash)
            + Encoding.UTF8.GetByteCount(ParentHash)
            + Encoding.UTF8.GetByteCount(Sha3Uncles)
            + Encoding.UTF8.GetByteCount(LogsBloom)
            + Encoding.UTF8.GetByteCount(TransactionsRoot)
            + Encoding.UTF8.GetByteCount(StateRoot)
            + Encoding.UTF8.GetByteCount(ReceiptsRoot)
            + Encoding.UTF8.GetByteCount(Miner)
            + Encoding.UTF8.GetByteCount(Difficulty)
            + Encoding.UTF8.GetByteCount(TotalDifficulty)
            + Encoding.UTF8.GetByteCount(ExtraData)
            + Encoding.UTF8.GetByteCount(Size)
            + Encoding.UTF8.GetByteCount(GasLimit)
            + Encoding.UTF8.GetByteCount(GasUsed)
            + Encoding.UTF8.GetByteCount(TimeStamp)
            + // todo Find a way to calculate the byte size of the transactions
            + Encoding.UTF8.GetByteCount(Uncles))
;
    }

    public override RequestEvent GetRequestData(Span<byte> context)
    {
        // todo: Implement GetRequestData
    }

    public string CalculateHash()
    {
        var rawData = $"{Number} {Hash} {ParentHash} {Nonce} {Sha3Uncles} {LogsBloom} {TransactionsRoot} {StateRoot} {ReceiptsRoot} {Miner} {Difficulty} {TotalDifficulty} {ExtraData} {Size} {GasLimit} {GasUsed} {TimeStamp} {string.Join(' ', Transactions.Select(a => a.RawTransaction))} {string.Join(' ', Uncles)}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(bytes).Replace("-", "");
    }

}
