using Shared.Helpers;
using Nethereum.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Shared;
public class MemPool
{
    public Guid Identity { get; }
    public string Nonce { get; }
    public string GasPrice { get; }
    public string GasLimit { get; }
    public string To { get; }
    public string Value { get; }
    public string Data { get; }
    public string V { get; }
    public string R { get; }
    public string S { get; }
    public MemPool(Span<byte> transaction)
    {
        Identity = Guid.NewGuid();
        Span<byte> decimalArray = stackalloc byte[transaction.Length / 2];
        transaction.HexArrayToDecimalArray(decimalArray);
        if (decimalArray[0] >= 0 && decimalArray[0] <= 127) throw new ArgumentException();
        // TODO: replace with custom implementation
        var transactionDetails = (SignedLegacyTransaction)TransactionFactory.CreateTransaction(decimalArray.ToArray());
        Nonce = Encoding.UTF8.GetString(transactionDetails.Nonce);
        GasPrice = Encoding.UTF8.GetString(transactionDetails.GasPrice);
        GasLimit = Encoding.UTF8.GetString(transactionDetails.GasLimit);
        To = Encoding.UTF8.GetString(transactionDetails.ReceiveAddress);
        Value = Encoding.UTF8.GetString(transactionDetails.Value);
        Data = Encoding.UTF8.GetString(transactionDetails.Data ?? []);
        V = Encoding.UTF8.GetString(transactionDetails.Signature.V);
        R = Encoding.UTF8.GetString(transactionDetails.Signature.R);
        S = Encoding.UTF8.GetString(transactionDetails.Signature.S);
    }

    public byte[] IdentifierAsHex() =>
        Identity.ToByteArray();
}
