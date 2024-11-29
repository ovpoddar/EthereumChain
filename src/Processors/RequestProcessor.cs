using src.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    internal static bool CanProcessAsBlockChainRequest(HttpListenerRequest request, ref Span<byte> requestContext)
    {
        if (!string.Equals(request.HttpMethod, METHOD, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(request.ContentType, ALLOWEDCONTENTTYPE, StringComparison.OrdinalIgnoreCase)
            || request.Url == null
            || !string.Equals(request.Url.AbsolutePath, TARGETEDPATH, StringComparison.OrdinalIgnoreCase)
            || request.IsWebSocketRequest)
            return false;

        var rpcVersionRange = RequestSerializer.GetValueAs<Range>(ref requestContext, "jsonrpc");
        var rpcVersion = requestContext[rpcVersionRange.Start..rpcVersionRange.End];

        if (!MemoryExtensions.SequenceEqual(rpcVersion, Setting.WorkingRpcVersionByte))
            return false;

        return true;
    }
}
