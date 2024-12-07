using src.Handlers;
using src.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace src.Processors.HTTP;
internal static class ResponseProcessor
{
    internal static void ProcessRequest(ref Span<byte> requestContext, Stream response, SQLiteConnection sqLiteConnection)
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
                var codeDetails = RequestSerializer.GetArrayAs<string>(ref requestContext, "params", 2);
                response.Write(RequestHandler.ProcessEthGetCode(codeDetails[0], codeDetails[1]));
                break;

            case "eth_gasPrice":
                response.Write(Setting.GasPriceFormattedByte);
                break;

            case "eth_estimateGas":
                var estimateGas = RequestSerializer.GetArrayAs<EstimateGas>(ref requestContext, "params", 1);
                response.Write(RequestHandler.ProcessEthEstimateGas(ref estimateGas[0]));
                break;

            //TODO: implement own custom chain and infrastructure.

            default:
                Console.WriteLine(method);
                break;
        }
    }

    internal static void SetUpHeaders(HttpListenerContext context)
    {
        context.Response.Headers.Clear();
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "POST");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "content-type");
        context.Response.Headers.Add("content-type", "application/json");

    }
}
