using src.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace src.Processors;
internal static class ResponseProcessor
{
    internal static void ProcessRequest(ref Request requestContext, HttpListenerResponse response)
    {
        response.OutputStream.Write("{"u8);
        response.OutputStream.Write("\"jsonrpc\":\"2.0\""u8);
        if (requestContext.Id != null)
            response.OutputStream.Write(Encoding.UTF8.GetBytes($"\"id\":{requestContext.Id}"));

        switch(requestContext.Method)
        {
            case "eth_currencySymbol":
                response.OutputStream.Write("\"result\":\"ETH\""u8);
                break;
            case "net_version":
                // check all the chain list in https://chainid.network/
                response.OutputStream.Write("\"result\":\"1\""u8);
                break;
        }
        response.OutputStream.Write("}"u8);
        response.OutputStream.Close();
    }
}
