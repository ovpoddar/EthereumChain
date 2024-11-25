using src.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace src;
internal static class RequestProcessor
{
    const string METHOD = "POST";
    const string ALLOWEDCONTENTTYPE = "application/json";
    const string TARGETEDPATH = "/";

    [SkipLocalsInit]
    internal static bool TryProcessForBlockChainNetworks(HttpListenerRequest request, out Request requestObject)
    {
        requestObject = new();
        if (!string.Equals(request.HttpMethod, METHOD, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(request.ContentType, ALLOWEDCONTENTTYPE, StringComparison.OrdinalIgnoreCase)
            || request.Url == null
            || !string.Equals(request.Url.AbsolutePath, TARGETEDPATH, StringComparison.OrdinalIgnoreCase)
            || request.IsWebSocketRequest
            )
            return false;

        var requestLength = (int)request.ContentLength64;
        Span<byte> requestContext = stackalloc byte[requestLength < 1024 ? requestLength : 1024];
        request.InputStream.ReadExactly(requestContext);
        requestObject = JsonSerializer.Deserialize<Request>(requestContext);

        if (requestObject.RPCVersion != null && requestObject.RPCVersion != "2.0")
            return false;

        return true;
    }
}
