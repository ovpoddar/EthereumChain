using Shared.Models;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
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
    public ulong GasLimit { get; set; }
    public ulong GasUsed { get; set; }
    public long TimeStamp { get; }
    public List<BaseTransaction> Transactions { get; }
    public string Uncles { get; set; }


    public override ushort GetWrittenByteSize()
    {
        // really don't want it but don't have a good idea to avoid it
        var transactionStr = string.Join(' ', Transactions.Select(a => $"{a.TransactionId}: {a.RawTransaction}"));
        return (ushort)(55
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
            + Encoding.UTF8.GetByteCount(transactionStr)
            + Encoding.UTF8.GetByteCount(Uncles));
        ;
    }

    // todo: test this writing
    public override RequestEvent GetRequestEvent(Span<byte> context)
    {
        Debug.Assert(context.Length > 55);

        var writingIndex = 0;
        BinaryPrimitives.WriteInt32BigEndian(context[writingIndex..], Number);
        writingIndex += sizeof(int);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(Hash, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(ParentHash, context[writingIndex..]);
        context[writingIndex++] = 0;

        BinaryPrimitives.WriteInt64BigEndian(context[writingIndex..], Nonce);
        writingIndex += sizeof(long);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(Sha3Uncles, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(LogsBloom, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(TransactionsRoot, context[writingIndex..]);
        context[writingIndex++] = 0; 

        writingIndex += Encoding.UTF8.GetBytes(StateRoot, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(ReceiptsRoot, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(Miner, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(Difficulty, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(TotalDifficulty, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(ExtraData, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(Size, context[writingIndex..]);
        context[writingIndex++] = 0;

        BinaryPrimitives.WriteUInt64BigEndian(context[writingIndex..], GasLimit);
        writingIndex += sizeof(ulong);
        context[writingIndex++] = 0;

        BinaryPrimitives.WriteUInt64BigEndian(context[writingIndex..], GasUsed);
        writingIndex += sizeof(ulong);
        context[writingIndex++] = 0;

        BinaryPrimitives.WriteInt64BigEndian(context[writingIndex..], TimeStamp);
        writingIndex += sizeof(long);
        context[writingIndex++] = 0;

        var transactionStr = string.Join(' ', Transactions.Select(a => $"{a.TransactionId}: {a.RawTransaction}"));
        writingIndex += Encoding.UTF8.GetBytes(transactionStr, context[writingIndex..]);
        context[writingIndex++] = 0;

        writingIndex += Encoding.UTF8.GetBytes(Uncles, context[writingIndex..]);
        context[writingIndex++] = 0;

        return new RequestEvent(MinerEventsTypes.BlockGenerated, context);

    }

    public string CalculateHash()
    {
        var rawData = $"{Number} {Hash} {ParentHash} {Nonce} {Sha3Uncles} {LogsBloom} {TransactionsRoot} {StateRoot} {ReceiptsRoot} {Miner} {Difficulty} {TotalDifficulty} {ExtraData} {Size} {GasLimit} {GasUsed} {TimeStamp} {string.Join(' ', Transactions.Select(a => a.RawTransaction))} {string.Join(' ', Uncles)}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(bytes).Replace("-", "");
    }

}
