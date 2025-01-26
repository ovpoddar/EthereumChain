using Nethereum.Model;
using Shared.Helpers;
using System.Diagnostics;
using System.Text;
using static Shared.Core.Transaction;

namespace Shared.Core;
public class Transaction : BaseTransaction
{
    private readonly ulong _gasPrice;
    private readonly ulong _gasLimit;
    private readonly ulong _value;
    public string Nonce { get; }
    public string GasPrice { get => _gasPrice.ToString("x"); }
    public string GasLimit { get => _gasLimit.ToString("X"); }
    public string To { get; }
    public string Value { get => _value.ToString("x"); }
    public string Data { get; }
    public string V { get; }
    public string R { get; }
    public string S { get; }

    public Transaction(Guid id, Span<byte> transaction) : base(id, Encoding.UTF8.GetString(transaction))
    {
        Span<byte> decimalArray = stackalloc byte[transaction.Length / 2];
        transaction.HexArrayToDecimalArray(decimalArray);
        if (decimalArray[0] >= 0 && decimalArray[0] <= 127) throw new ArgumentException();
        // TODO: replace with custom implementation
        var transactionDetails = (SignedLegacyTransaction)TransactionFactory.CreateTransaction(decimalArray.ToArray());
        Nonce = Encoding.UTF8.GetString(transactionDetails.Nonce);
        _gasPrice = Utilities.GetLongFromHexArray(transactionDetails.GasPrice);
        _gasLimit = Utilities.GetLongFromHexArray(transactionDetails.GasLimit);
        To = Encoding.UTF8.GetString(transactionDetails.ReceiveAddress);
        _value = Utilities.GetLongFromHexArray(transactionDetails.Value);
        Data = Encoding.UTF8.GetString(transactionDetails.Data ?? []);
        V = Encoding.UTF8.GetString(transactionDetails.Signature.V);
        R = Encoding.UTF8.GetString(transactionDetails.Signature.R);
        S = Encoding.UTF8.GetString(transactionDetails.Signature.S);
    }

    public Transaction(ReadOnlySpan<byte> data) : base(data) 
    {
        var transaction = data[17..^1];
        Span<byte> decimalArray = stackalloc byte[transaction.Length / 2];
        transaction.HexArrayToDecimalArray(decimalArray);
        if (decimalArray[0] >= 0 && decimalArray[0] <= 127) throw new ArgumentException();
        // TODO: replace with custom implementation
        var transactionDetails = (SignedLegacyTransaction)TransactionFactory.CreateTransaction(decimalArray.ToArray());
        Nonce = Encoding.UTF8.GetString(transactionDetails.Nonce);
        _gasPrice = Utilities.GetLongFromHexArray(transactionDetails.GasPrice);
        _gasLimit = Utilities.GetLongFromHexArray(transactionDetails.GasLimit);
        To = Encoding.UTF8.GetString(transactionDetails.ReceiveAddress);
        _value = Utilities.GetLongFromHexArray(transactionDetails.Value);
        Data = Encoding.UTF8.GetString(transactionDetails.Data ?? []);
        V = Encoding.UTF8.GetString(transactionDetails.Signature.V);
        R = Encoding.UTF8.GetString(transactionDetails.Signature.R);
        S = Encoding.UTF8.GetString(transactionDetails.Signature.S);
    }
}

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

    public override RequestEvent GetRequestData(Span<byte> context)
    {
        context[16] = 0;
        context[^1] = 0;
        var response = this.TransactionId.TryWriteBytes(context);
        Debug.Assert(response);
        Encoding.UTF8.GetBytes(this.RawTransaction, context[17..]);
        return new RequestEvent(MinerEventsTypes.TransactionAdded, context);
    }
}
