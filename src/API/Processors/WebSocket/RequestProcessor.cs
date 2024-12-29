using System.Collections.Specialized;
using System.Net;

namespace API.Processors.WebSocket;
internal class RequestProcessor
{
    public static bool CanProcessAsBlockChainResponse(NameValueCollection headers) =>
        headers.Get("Sec-WebSocket-Key") != string.Empty;

    public static async ValueTask VerifyRequest(List<MinerSocketProcessor> minerConnections, HttpListenerContext context)
    {
        if (minerConnections.Count == Setting.MinerNetworkCount)
            context.Response.Close();

        var connection = await context.AcceptWebSocketAsync(null);
        var connectionProcesser = new MinerSocketProcessor(minerConnections, connection.WebSocket);
        minerConnections.Add(connectionProcesser);
        connectionProcesser.Run();
    }
}
