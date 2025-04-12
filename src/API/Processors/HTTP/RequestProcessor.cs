using API.Handlers;
using Shared;
using System.Net;
using System.Runtime.CompilerServices;

namespace API.Processors.HTTP;
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

        if (!rpcVersion.SequenceEqual(Setting.WorkingRpcVersionByte))
            return false;

        return true;
    }
}
