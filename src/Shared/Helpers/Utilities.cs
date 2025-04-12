using Shared.Core;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Shared.Helpers;
public static class Utilities
{
    private const int _gweiMultiplier = 1000000000;
    public static string EnsureEndsWith(this string value, string suffix, StringComparison comparison = StringComparison.InvariantCulture) =>
        value.EndsWith(suffix, comparison)
            ? value
            : value + suffix;

    public static string EnsureStartsWith(this string value, string prefix, StringComparison comparison = StringComparison.InvariantCulture) =>
        value.StartsWith(prefix, comparison)
            ? value
            : prefix + value;

    public static string EnsureNotStartsWith(this string value, string prefix, StringComparison comparison = StringComparison.InvariantCulture) =>
            value.StartsWith(prefix, comparison)
                ? value[prefix.Length..]
                : value;


    public static ReadOnlySpan<char> EnsureNotStartsWith(this ReadOnlySpan<char> value, ReadOnlySpan<char> prefix, StringComparison comparison = StringComparison.InvariantCulture) =>
            value.StartsWith(prefix, comparison)
                ? value[prefix.Length..]
                : value;

    public static void HexArrayToDecimalArray(this Span<byte> hexArray, Span<byte> decimalArray)
    {
        if (hexArray.Length == 0 || hexArray.Length == 2 && (hexArray.SequenceEqual("0x"u8)
            || hexArray.SequenceEqual("0X"u8)))
            return;

        if (hexArray.Length / 2 != decimalArray.Length) throw new Exception("Not enough data to write.");

        var processIndex = 0;
        if (hexArray[0] == '0' && hexArray[1] == 'X' || hexArray[1] == 'x')
            processIndex = 2;

        for (var writeIndex = 0; processIndex < hexArray.Length; processIndex += 2)
            decimalArray[writeIndex++] = ColasHexToDDecimal(hexArray.Slice(processIndex, 2));
    }

    public static void HexArrayToDecimalArray(this ReadOnlySpan<byte> hexArray, Span<byte> decimalArray)
    {
        if (hexArray.Length == 0 || hexArray.Length == 2 && (hexArray.SequenceEqual("0x"u8)
            || hexArray.SequenceEqual("0X"u8)))
            return;

        if (hexArray.Length / 2 != decimalArray.Length) throw new Exception("Not enough data to write.");

        var processIndex = 0;
        if (hexArray[0] == '0' && hexArray[1] == 'X' || hexArray[1] == 'x')
            processIndex = 2;

        for (var writeIndex = 0; processIndex < hexArray.Length; processIndex += 2)
            decimalArray[writeIndex++] = ColasHexToDDecimal(hexArray.Slice(processIndex, 2));
    }

    static byte ColasHexToDDecimal(Span<byte> hexArray)
    {
        Debug.Assert(hexArray.Length == 2);
        var first = hexArray[0] switch
        {
            > 0x40 and < 0x47 or > 0x60 and < 0x67 =>
                (0x20 & hexArray[0]) == 0x20 ? (byte)(hexArray[0] + 0xA - 0x61) : (byte)(hexArray[0] + 0xA - 0x41),
            > 0x29 and < 0x40 => (byte)(hexArray[0] - 0x30),
            _ => throw new NotImplementedException()
        };

        var second = hexArray[1] switch
        {
            > 0x40 and < 0x47 or > 0x60 and < 0x67 =>
                (0x20 & hexArray[1]) == 0x20 ? (byte)(hexArray[1] + 0xA - 0x61) : (byte)(hexArray[1] + 0xA - 0x41),
            > 0x29 and < 0x40 => (byte)(hexArray[1] - 0x30),
            _ => throw new NotImplementedException()
        };
        return (byte)(first << 4 | second);
    }

    static byte ColasHexToDDecimal(ReadOnlySpan<byte> hexArray)
    {
        Debug.Assert(hexArray.Length == 2);
        var first = hexArray[0] switch
        {
            > 0x40 and < 0x47 or > 0x60 and < 0x67 =>
                (0x20 & hexArray[0]) == 0x20 ? (byte)(hexArray[0] + 0xA - 0x61) : (byte)(hexArray[0] + 0xA - 0x41),
            > 0x29 and < 0x40 => (byte)(hexArray[0] - 0x30),
            _ => throw new NotImplementedException()
        };

        var second = hexArray[1] switch
        {
            > 0x40 and < 0x47 or > 0x60 and < 0x67 =>
                (0x20 & hexArray[1]) == 0x20 ? (byte)(hexArray[1] + 0xA - 0x61) : (byte)(hexArray[1] + 0xA - 0x41),
            > 0x29 and < 0x40 => (byte)(hexArray[1] - 0x30),
            _ => throw new NotImplementedException()
        };
        return (byte)(first << 4 | second);
    }

    public static ulong GetLongFromHexArray(ReadOnlySpan<byte> hexArray)
    {
        Debug.Assert(hexArray.Length <= 8);

        ulong result = 0;
        foreach (var b in hexArray)
            result = (result << 8)
                   + ((ulong)(b >> 4) * 16)
                   + (ulong)(b & 0xF);
        return result;
    }

    public static void ConvertHexValuesTobyte(Span<byte> hexArray)
    {
        Debug.Assert(hexArray.Length == 8);
        Span<byte> normalArray = stackalloc byte[hexArray.Length * 2];
        var i = 0;
        var j = 0;
        byte b;

        while (i < hexArray.Length)
        {
            b = hexArray[i++];
            normalArray[j++] = ToUpper(b >> 4);
            normalArray[j++] = ToUpper(b);
        }
    }

    static byte ToUpper(int value)
    {
        value &= 0xF;
        value += '0';

        if (value > '9')
        {
            value += ('A' - ('9' + 1));
        }

        return (byte)value;
    }

    public static T ToStruct<T>(this byte[] @bytes) where T : struct =>
       Unsafe.As<byte, T>(ref @bytes[0]);

    public static T ToStruct<T>(this Span<byte> @bytes) where T : struct =>
        Unsafe.As<byte, T>(ref @bytes[0]);

    public static string EncodingForNetworkTransfer(this string value) =>
        value.Replace(" ", "&nbsp;");

    public static string DecodingFormNetworkTransfer(this string value) =>
        value.Replace("&nbsp;", " ");

    public static BigInteger ConvertAmountToWei(this decimal value) =>
        new BigInteger((double)(value * _gweiMultiplier) * _gweiMultiplier);

    public static decimal ConvertGWeiToAmount(this decimal value) =>
        value / _gweiMultiplier;

    public static decimal ConvertToEtherAmount(this BigInteger value) =>
        (decimal)(value / _gweiMultiplier) / _gweiMultiplier;

    public static decimal ToEtherBalance(this string value) =>
        BigInteger.Parse(value, System.Globalization.NumberStyles.HexNumber)
            .ConvertToEtherAmount();

    public static byte[] ToByteArray(this BaseBlock block)
    {
        var requiredSize = block.GetWrittenByteSize();
        var rent = ArrayPool<byte>.Shared.Rent(requiredSize);
        var context = rent.AsSpan(0, requiredSize);
        try
        {
            var writingIndex = 0;
            BinaryPrimitives.WriteInt32BigEndian(context[writingIndex..], block.Number);
            writingIndex += sizeof(int);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.Hash, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.ParentHash, context[writingIndex..]);
            context[writingIndex++] = 0;

            BinaryPrimitives.WriteInt64BigEndian(context[writingIndex..], block.Nonce);
            writingIndex += sizeof(long);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.Sha3Uncles, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.LogsBloom, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.TransactionsRoot, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.StateRoot, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.ReceiptsRoot, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.Miner, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.Difficulty, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.TotalDifficulty, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.ExtraData, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.Size, context[writingIndex..]);
            context[writingIndex++] = 0;

            BinaryPrimitives.WriteUInt64BigEndian(context[writingIndex..], block.GasLimit);
            writingIndex += sizeof(ulong);
            context[writingIndex++] = 0;

            BinaryPrimitives.WriteUInt64BigEndian(context[writingIndex..], block.GasUsed);
            writingIndex += sizeof(ulong);
            context[writingIndex++] = 0;

            BinaryPrimitives.WriteInt64BigEndian(context[writingIndex..], block.TimeStamp);
            writingIndex += sizeof(long);
            context[writingIndex++] = 0;

            var transactionStr = block.ComposeTransactionString();
            writingIndex += Encoding.UTF8.GetBytes(transactionStr, context[writingIndex..]);
            context[writingIndex++] = 0;

            writingIndex += Encoding.UTF8.GetBytes(block.Uncles, context[writingIndex..]);
            context[writingIndex++] = 0;

            return context.ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }
}
