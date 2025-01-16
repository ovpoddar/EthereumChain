using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public TransactionAddedEventArgs(ReadOnlySpan<byte> data)
    {
        this.TransactionId = new Guid(data[..16]);
        this.Transaction = Encoding.UTF8.GetString(data[17..^1]);
    }

    public override ushort GetWrittenByteSize() =>
        (ushort)(18 + Encoding.UTF8.GetByteCount(this.Transaction));

    public override RequestEvent GetRequestData(Span<byte> context)
    {
        context[16] = 0;
        context[^1] = 0;
        var response = this.TransactionId.TryWriteBytes(context);
        Debug.Assert(response);
        Encoding.UTF8.GetBytes(this.Transaction, context[17..]);
        return new RequestEvent(MinerEventsTypes.TransactionAdded, context);
    }
}
