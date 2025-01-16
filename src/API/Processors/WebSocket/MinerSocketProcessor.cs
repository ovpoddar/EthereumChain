using API.Handlers;
using API.Models;
using NBitcoin.Secp256k1;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;

namespace API.Processors.WebSocket;
internal class MinerSocketProcessor : IAsyncDisposable
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
        var maximumRead = new byte[1024 * 2];
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
                        RequestEvent data = new(maximumRead.AsSpan().Slice(0, status.Count));
                        // received new events form network
                        switch (data.EventType)
                        {
                            case MinerEventsTypes.TransactionAdded:
                                 MinerEvents.RaisedMinerEvent(data.EventType, new TransactionAddedEventArgs(data.EventValue));
                                break;
                            case MinerEventsTypes.TransactionUpdated:
                                // MinerEvents.RaisedMinerEvent(data.EventType, data.EventValue);
                                break;
                            case MinerEventsTypes.BlockGenerated:
                                // MinerEvents.RaisedMinerEvent(data.EventType, data.EventValue);
                                break;
                            case MinerEventsTypes.BlockConfirmed:
                                // MinerEvents.RaisedMinerEvent(data.EventType, data.EventValue);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
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


    public async Task NotifyAll(byte[] requestEvent)
    {
        await Parallel.ForEachAsync(_minerConnections, async (a, b) =>
        {
            await a.SendAsync(requestEvent, WebSocketMessageType.Binary, true, b);
        });
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in _minerConnections)
        {
            await connection.CloseAsync(default, null, default);
            connection.Dispose();
        }
    }
}
