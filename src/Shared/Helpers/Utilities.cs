﻿using Nethereum.Hex.HexConvertors;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers;
internal static class Utilities
{
    public static string EnsureEndsWith(this string value,
        string suffix,
        StringComparison comparison = StringComparison.InvariantCulture) =>
        value.EndsWith(suffix, comparison)
            ? value
            : value + suffix;

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


    public static ulong GetLongFromHexArray(ReadOnlySpan<byte> hexArray)
    {
        Debug.Assert(hexArray.Length == 8);

        ulong result = 0;
        foreach (var b in hexArray)
        {
            byte upperNibble = (byte)(b >> 4);
            if (upperNibble <= 9)
                upperNibble += (byte)'0';
            else
                upperNibble += (byte)('A' - 10);

            result = (result << 4) + (ulong)(upperNibble > '9' ? upperNibble - 'A' + 10 : upperNibble - '0');

            byte lowerNibble = (byte)(b & 0xF);
            if (lowerNibble <= 9)
                lowerNibble += (byte)'0';
            else
                lowerNibble += (byte)('A' - 10);

            result = (result << 4) + (ulong)(lowerNibble > '9' ? lowerNibble - 'A' + 10 : lowerNibble - '0');
        }

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
}