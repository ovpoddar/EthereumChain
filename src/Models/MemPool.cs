using Nethereum.Model;
using src.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace src.Models;
internal class MemPool
{
    public string Nonce { get; }
    public string GasPrice { get; }
    public string GasLimit { get; }
    public string To { get; }
    public string Value { get; }
    public string Data { get; }
    public string V { get; }
    public string R { get; }
    public string S { get; }
    internal MemPool(Span<byte> transaction)
    {
        Span<byte> decimalArray = stackalloc byte[transaction.Length / 2];
        transaction.HexArrayToDecimalArray(decimalArray);
        if (decimalArray[0] >= 0 && decimalArray[0] <= 127) throw new ArgumentException();
        // todo: replace with custom implementation
        var transactionDetails = (SignedLegacyTransaction)TransactionFactory.CreateTransaction(transaction.ToArray());
        this.Nonce = Encoding.UTF8.GetString(transactionDetails.Nonce);
        this.GasPrice = Encoding.UTF8.GetString(transactionDetails.GasPrice);
        this.GasLimit = Encoding.UTF8.GetString(transactionDetails.GasLimit);
        this.To = Encoding.UTF8.GetString(transactionDetails.ReceiveAddress);
        this.Value = Encoding.UTF8.GetString(transactionDetails.Value);
        this.Data = Encoding.UTF8.GetString(transactionDetails.Data);
        this.V = Encoding.UTF8.GetString(transactionDetails.Signature.V);
        this.R = Encoding.UTF8.GetString(transactionDetails.Signature.R);
        this.S = Encoding.UTF8.GetString(transactionDetails.Signature.S);
    }

    internal byte[]? IdentifyerAsHex()
    {
        return Encoding.UTF8.GetBytes("hellowworld");
    }
}
