﻿using Shared.Helpers;
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
using System.Data.SQLite;

namespace Shared;
public class Transaction
{
    private readonly ulong _gasPrice;
    private readonly ulong _gasLimit;
    private readonly ulong _value;
    private readonly Guid _id;
    public string Id { get => _id.ToString("X"); }
    public string Nonce { get; }
    public string GasPrice { get => _gasPrice.ToString("x"); }
    public string GasLimit { get => _gasLimit.ToString("X"); }
    public string To { get; }
    public string Value { get => _value.ToString("x"); }
    public string Data { get; }
    public string V { get; }
    public string R { get; }
    public string S { get; }
    public Transaction(Guid id, Span<byte> transaction)
    {
        _id = id;
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
