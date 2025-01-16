using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal class TransactionAddedEventArgs : MinerEventArgs
{
    public Guid TransactionId { get; }
    public string Transaction { get; }

    public TransactionAddedEventArgs(Guid transactionId, string transaction)
    {
        Transaction = transaction;
        TransactionId = transactionId;
    }

    public override void ParseFromPacket(Span<byte> data)
    {
        throw new NotImplementedException();
    }

    public override void WriteToByte(Span<byte> data)
    {
        data[16] = 0; data[data.Length - 1] = 0;
        this.TransactionId.TryWriteBytes(data);
        Encoding.UTF8.GetBytes(this.Transaction, data[17..]);
    }

    public override ushort GetWrittenByteSize() =>
        (ushort)(18 + Encoding.UTF8.GetByteCount(this.Transaction));
}
