using API.Handlers;
using API.Models;
using Shared.Processors.Communication;
using NBitcoin.Secp256k1;
using Shared.Models;
using System.Buffers;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using Shared;

namespace API.Processors.WebSocket;
internal class MinerSocketProcessor : IAsyncDisposable
{
    private readonly List<System.Net.WebSockets.WebSocket> _minerConnections;
    private readonly SQLiteConnection _sqlConnection;
    private readonly IApplicationCommunication _communication;

    public MinerSocketProcessor(SQLiteConnection sqlConnection, IApplicationCommunication communication)
    {
        this._minerConnections = new(Setting.MinerNetworkCount);
        this._sqlConnection = sqlConnection;
        _communication = communication;
    }

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
        var bucket = new List<byte>(1024 * 2);
        int offset = 0;
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

                    if (status.EndOfMessage)
                    {
                        if (status.MessageType == WebSocketMessageType.Binary)
                        {
                            var memory = offset == 0 
                                ? maximumRead.AsSpan(0..status.Count) 
                                : CollectionsMarshal.AsSpan(bucket);
                            RequestEvent response = new(memory);
                            var data = response.EventValue;
                            // received new events form network
                            switch (response.EventType)
                            {
                                case MinerEventsTypes.TransactionAdded:
                                    RequestHandler.ProcessEthSendRawTransaction(ref data, _sqlConnection);
                                    break;
                                case MinerEventsTypes.TransactionUpdated:
                                    // MinerEvents.RaisedMinerEvent(response.EventType, response.EventValue);
                                    break;
                                case MinerEventsTypes.BlockGenerated:
                                    RequestHandler.ProcessGeneratedBlock(ref data, _communication);
                                    break;
                                case MinerEventsTypes.BlockConfirmed:
                                    // MinerEvents.RaisedMinerEvent(response.EventType, response.EventValue);
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        bucket.Clear();
                        offset = 0;
                    }
                    else
                    {
                        bucket.AddRange(maximumRead);
                        offset += status.Count;
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
