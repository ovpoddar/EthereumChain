using API.Handlers;
using System.Collections.Specialized;
using System.Net;
using System.Net.WebSockets;

namespace API.Processors.WebSocket;
internal class RequestProcessor
{
    public static bool CanProcessAsBlockChainResponse(NameValueCollection headers) =>
        !string.IsNullOrWhiteSpace(headers.Get("Sec-WebSocket-Key"));

}
