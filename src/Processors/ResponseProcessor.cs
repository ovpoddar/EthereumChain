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
    internal static void ProcessRequest(ref Request requestContext, Stream response)
    {
        response.Write("\"result\":"u8);
        switch (requestContext.Method)
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
                Debug.Assert(requestContext.Params != null);
                Debug.Assert(requestContext.Params.Length == 2);

                var accountAddress = requestContext.Params[0];
                var targetedBlockType = requestContext.Params[1];
                response.Write(RequestHandler.ProcessEthGetCode(accountAddress, targetedBlockType));
                break;

            case "eth_gasPrice":
                response.Write(Setting.GasPriceFormattedByte);
                break;
            case "eth_estimateGas":
                Debug.Assert(requestContext.Params != null);
                var estGas = new EstimateGas();
                response.Write(RequestHandler.ProcessEthEstimateGas(ref estGas));
                break;
        }
    }
}
