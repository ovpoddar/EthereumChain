using System.Diagnostics;
using System.Text;

namespace Shared.Core;

public class BaseTransaction : MinerEventArgs
{
    public Guid TransactionId { get; }
    public string RawTransaction { get; }


    public BaseTransaction(Guid transactionId, string transaction)
    {
        TransactionId = transactionId;
        RawTransaction = transaction;
    }

    public BaseTransaction(ReadOnlySpan<byte> data)
    {
        Debug.Assert(data.Length > 2048);
        this.TransactionId = new Guid(data[..16]);
        this.RawTransaction = Encoding.UTF8.GetString(data[17..^1]);
    }

    public override ushort GetWrittenByteSize() =>
        (ushort)(18 + Encoding.UTF8.GetByteCount(this.RawTransaction));

    public override RequestEvent GetRequestEvent(Span<byte> context)
    {
        context[16] = 0;
        context[^1] = 0;
        var response = this.TransactionId.TryWriteBytes(context);
        Debug.Assert(response);
        Encoding.UTF8.GetBytes(this.RawTransaction, context[17..]);
        return new RequestEvent(MinerEventsTypes.TransactionAdded, context);
    }

    public static implicit operator Transaction(BaseTransaction transaction) =>
        new Transaction(transaction.TransactionId, Encoding.UTF8.GetBytes(transaction.RawTransaction));
}
