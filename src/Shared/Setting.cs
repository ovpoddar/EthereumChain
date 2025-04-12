using Shared.Helpers;
using System.Text;

namespace Shared;
public static class Setting
{
    private const ushort _netWorkId = 6;
    // check all the chain list in https://chainid.network/
    private const ushort _chainId = 6;
    private const long _gasPrice = 20000000000;

    public const int RPCPort = 9546;
    public const int MinerNetworkCount = 20;
    public const decimal MinimumProfit = 0.5M;
    public const int SharedMemorySize = 1024 * 2;
    public const int ThreadUsage = 1; // 0 for all cores, negative will remove the total number of cores, posative will use the number of cores
    public const string DefaultMinerAddress = "0x5561035012fCB5d4AF49A200412De5545087d3D6";

    public static readonly string WorkingRpcVersion = "\"2.0\"";
    public static readonly string EthereumChainStoragePath = "./storage.sqlite";
    public static readonly byte[] WorkingRpcVersionByte = Encoding.UTF8.GetBytes(WorkingRpcVersion);
    public static readonly byte[] NetWorkIdFormattedByte = Encoding.UTF8.GetBytes($"\"{_netWorkId}\"");
    public static readonly byte[] ChainIdFormattedByte = Encoding.UTF8.GetBytes($"\"0x{_chainId:x}\"");
    public static readonly byte[] GasPriceFormattedByte = Encoding.UTF8.GetBytes($"\"0x{_gasPrice:x}\"");

    public static bool VerifyBlockExecution(ReadOnlySpan<char> computedHash)
    {
        computedHash = computedHash.EnsureNotStartsWith("0x");
        var prefix = new string('0', 2);
        return computedHash.StartsWith(prefix, StringComparison.InvariantCulture);
    }
}
