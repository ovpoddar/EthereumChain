using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public static class Setting
{
    private static readonly ushort _netWorkId = 6;
    // check all the chain list in https://chainid.network/
    private static readonly ushort _chainId = 6;
    private static readonly long _gasPrice = 20000000000;

    public const int RPCPort = 9546;
    public const int MinerNetworkCount = 20;
    public static readonly string WorkingRpcVersion = "\"2.0\"";
    public static readonly string EthereumChainStoragePath = "./storage.sqlite";
    public static readonly byte[] WorkingRpcVersionByte = Encoding.UTF8.GetBytes(WorkingRpcVersion);
    public static readonly byte[] NetWorkIdFormattedByte = Encoding.UTF8.GetBytes($"\"{_netWorkId}\"");
    public static readonly byte[] ChainIdFormattedByte = Encoding.UTF8.GetBytes($"\"0x{_chainId:x}\"");
    public static readonly byte[] GasPriceFormattedByte = Encoding.UTF8.GetBytes($"\"0x{_gasPrice:x}\"");

    public const int SharedMemorySize = 1024 * 2;
}
