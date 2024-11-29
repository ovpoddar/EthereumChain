using src.Handlers;
using src.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace src.Processors;
internal static class ResponseProcessor
{
    internal static void ProcessRequest(ref Span<byte> requestContext, Stream response)
    {
        response.Write("\"result\":"u8);
        var method = RequestSerializer.GetValueAs<string>(ref requestContext, "method");
        switch (method)
        {
            case "eth_currencySymbol":
                response.Write("\"ETH\""u8);
                break;

            case "net_version":
                response.Write(Setting.NetWorkIdFormattedByte);
                break;

            case "eth_chainId":
                response.Write(Setting.ChainIdFormattedByte);
                break;

            case "eth_getCode":
                var codeDet = RequestSerializer.GetArrayAs<string>(ref requestContext, "params", 2);
                response.Write(RequestHandler.ProcessEthGetCode(codeDet[0], codeDet[1]));
                break;

            case "eth_gasPrice":
                response.Write(Setting.GasPriceFormattedByte);
                break;

            case "eth_estimateGas":
                var estGas = RequestSerializer.GetArrayAs<EstimateGas>(ref requestContext, "params", 1);
                response.Write(RequestHandler.ProcessEthEstimateGas(ref estGas[0]));
                break;
        }
    }
}
