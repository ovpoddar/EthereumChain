using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace src;
internal static class RequestProcessor
{
    const string METHOD = "POST";
    const string ALLOWEDCONTENTTYPE = "application/json";
    const string TARGETEDPATH = "/";

    internal static bool TryProcessForBlockChainNetworks(HttpListenerRequest request, Span<byte> requestContext)
    {
        if (!string.Equals(request.HttpMethod, METHOD, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(request.ContentType, ALLOWEDCONTENTTYPE, StringComparison.OrdinalIgnoreCase)
            || request.Url == null
            || !string.Equals(request.Url.AbsolutePath, TARGETEDPATH, StringComparison.OrdinalIgnoreCase)
            || !request.IsWebSocketRequest)
            return false;

        request.InputStream.ReadExactly(requestContext);
        return true;

    }
}
