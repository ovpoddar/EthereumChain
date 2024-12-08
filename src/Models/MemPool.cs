using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    internal MemPool(string transaction)
    {
        Nonce = transaction;
        GasLimit = transaction;
        GasPrice = transaction;
        To = transaction;
        Value = transaction;
        Data = transaction;
        V = transaction;
        R = transaction;
        S = transaction;
    }
    private MemPool()
    {
        Nonce = string.Empty;
        GasLimit = string.Empty;
        GasPrice = string.Empty;
        To = string.Empty;
        Value = string.Empty;
        Data = string.Empty;
        V = string.Empty;
        R = string.Empty;
        S = string.Empty;
    }

    internal byte[]? IdentifyerAsHex()
    {
        return Encoding.UTF8.GetBytes("hellowworld");
    }
}
