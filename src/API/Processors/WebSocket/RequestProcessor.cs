using System.Collections.Specialized;
using System.Net;
using WS = System.Net.WebSockets;

namespace API.Processors.WebSocket;
internal class RequestProcessor
{
    public static bool CanProcessAsBlockChainResponce(NameValueCollection headers) =>
        headers.Get("Sec-WebSocket-Key") != string.Empty;

    public static async ValueTask VerifyRequest(List<WS.WebSocket> minerConnections, HttpListenerContext context)
    {
        if (minerConnections.Count == Setting.MinerNetworkCount)
            context.Response.Close();

        var connection = await context.AcceptWebSocketAsync(null);
        minerConnections.Add(connection.WebSocket);
    }
}
