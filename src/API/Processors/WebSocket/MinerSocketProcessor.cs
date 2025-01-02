﻿using API.Handlers;
using System.Net.WebSockets;

namespace API.Processors.WebSocket;
public class MinerSocketProcessor
{
    private readonly List<MinerSocketProcessor> _minerConnections;
    private readonly System.Net.WebSockets.WebSocket _webSocket;

    public MinerSocketProcessor(List<MinerSocketProcessor> minerConnections, System.Net.WebSockets.WebSocket webSocket)
    {
        this._minerConnections = minerConnections;
        this._webSocket = webSocket;
    }

    internal void Run()
    {
        var maximumRead = new byte[1024];
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var status = await _webSocket.ReceiveAsync(maximumRead, default);

                    if (status.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, " a normal closure", default);
                        _webSocket.Dispose();
                        _minerConnections.Remove(this);
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
                    await _webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, 
                        "server is terminating the connection because it encountered an unexpected condition that prevented it from fulfilling the request",
                        default);
                    _webSocket.Dispose();
                    _minerConnections.Remove(this);
                    break;
                }
            }
        });
        return;

    }

}
