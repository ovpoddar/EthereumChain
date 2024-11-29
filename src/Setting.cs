using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src;
internal static class Setting
{
    public static readonly string WorkingRpcVersion = "\"2.0\"";
    private static readonly ushort NetWorkId = 6;
    // check all the chain list in https://chainid.network/
    private static readonly ushort ChainId = 6;
    private static readonly long GASPrice = 20000000000;

    public static readonly byte[] WorkingRpcVersionByte = Encoding.UTF8.GetBytes(WorkingRpcVersion);
    public static readonly byte[] NetWorkIdFormattedByte = Encoding.UTF8.GetBytes($"\"{NetWorkId}\"");
    public static readonly byte[] ChainIdFormattedByte = Encoding.UTF8.GetBytes($"\"0x{ChainId:x}\"");
    public static readonly byte[] GasPriceFormattedByte = Encoding.UTF8.GetBytes($"\"0x{GASPrice:x}\"");
}
