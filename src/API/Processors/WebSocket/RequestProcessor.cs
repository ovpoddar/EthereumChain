using System.Collections.Specialized;

namespace API.Processors.WebSocket;
internal class RequestProcessor
{
    public static bool CanProcessAsBlockChainResponse(NameValueCollection headers) =>
        !string.IsNullOrWhiteSpace(headers.Get("Sec-WebSocket-Key"));

}
