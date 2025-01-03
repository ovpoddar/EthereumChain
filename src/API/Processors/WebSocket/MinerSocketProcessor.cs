using API.Handlers;
using API.Models;
using NBitcoin.Secp256k1;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;

namespace API.Processors.WebSocket;
public class MinerSocketProcessor : IAsyncDisposable
{
    private readonly List<System.Net.WebSockets.WebSocket> _minerConnections;

    public MinerSocketProcessor() =>
        this._minerConnections = new(Setting.MinerNetworkCount);

    public async Task<System.Net.WebSockets.WebSocket?> HandleExpandNetworkAsync(HttpListenerContext context)
    {
        if (_minerConnections.Count == Setting.MinerNetworkCount)
        {
            context.Response.Close();
            return null;
        }
        var connection = await context.AcceptWebSocketAsync(null);
        _minerConnections.Add(connection.WebSocket);
        return connection.WebSocket;
    }

    public void StartReadResponse(System.Net.WebSockets.WebSocket webSocket)
    {
        var maximumRead = new byte[1024];
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var status = await webSocket.ReceiveAsync(maximumRead, default);

                    if (status.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, " a normal closure", default);
                        webSocket.Dispose();
                        _minerConnections.Remove(webSocket);
                        break;
                    }

                    if (status.MessageType == WebSocketMessageType.Binary)
                    {
                        var response = new Span<byte>(maximumRead, 0, status.Count);
                        var data = RequestSerializer.GetRequestEvent(ref response);
                        ResponseProcessor.ProcessRequest(data);
                    }
                }
                catch
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError,
                        "server is terminating the connection because it encountered an unexpected condition that prevented it from fulfilling the request",
                        default);
                    webSocket.Dispose();
                    _minerConnections.Remove(webSocket);
                    break;
                }
            }
        });

    }

    //public Task NotifyAll(RequestEvent requestEvent)
    //{

    //    Parallel.ForEachAsync(_minerConnections, (a, b) =>
    //    {
    //        a.SendAsync()
    //    });
    //}

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in _minerConnections)
        {
            await connection.CloseAsync(default, null, default);
            connection.Dispose();
        }
    }
}
