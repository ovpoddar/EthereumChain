using API;
using API.Handlers;
using API.Helpers;
using API.Models;
using API.Processors.Database;
using HTTP = API.Processors.HTTP;
using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;
using API.Processors.WebSocket;
using API.Processors.MinerEvents;

var eventProcesser = new EventProcesser();

using (var sqlConnection = InitializedDatabase())
using (var httpListener = new HttpListener())
await using (var webSocketListener = new MinerSocketProcessor())
{
    eventProcesser.Hanlde();
    httpListener.Prefixes.Add($"http://localhost:{Setting.RPCPort}/");
    httpListener.Prefixes.Add($"http://127.0.0.1:{Setting.RPCPort}/");
    httpListener.Start();
    foreach (var item in httpListener.Prefixes)
        Console.WriteLine($"RPC Application is listening on {item}");
    Console.WriteLine($"Miner application listening on ws://http://127.0.0.1:{Setting.RPCPort}");

    await StructureProcesser.MigrationStructure(sqlConnection);
    httpListener.BeginGetContext(ReceivedRequest, new ProcessorModel(httpListener, webSocketListener, sqlConnection));
    Console.ReadLine();
}

async void ReceivedRequest(IAsyncResult ar)
{
    if (ar.AsyncState is not ProcessorModel requestProcesser)
        return;

    var context = requestProcesser.Listener.EndGetContext(ar);
    if (RequestProcessor.CanProcessAsBlockChainResponse(context.Request.Headers))
    {
        var newMiner = await requestProcesser.WebSocketListener.HandleExpandNetworkAsync(context);
        if (newMiner != null) requestProcesser.WebSocketListener.StartReadResponse(newMiner);
        requestProcesser.Listener.BeginGetContext(ReceivedRequest, requestProcesser);
        return;
    }
    var requestLength = (int)context.Request.ContentLength64;
    Span<byte> requestContext = stackalloc byte[requestLength < 1024 ? requestLength : 1024];
    context.Request.InputStream.ReadExactly(requestContext);

    HTTP.ResponseProcessor.SetUpHeaders(context);

    if (HTTP.RequestProcessor.CanProcessAsBlockChainRequest(context.Request, ref requestContext))
    {
        context.Response.OutputStream.Write("{"u8);

        context.Response.OutputStream.Write("\"jsonrpc\":"u8);
        context.Response.OutputStream.Write(Setting.WorkingRpcVersionByte);
        context.Response.OutputStream.Write(","u8);

        var idRange = RequestSerializer.GetValueAs<Range>(ref requestContext, "id");
        if (idRange.Start.Value != 0 && idRange.End.Value != 0)
        {
            context.Response.OutputStream.Write("\"id\":"u8);
            context.Response.OutputStream.Write(requestContext[idRange.Start..idRange.End]);
            context.Response.OutputStream.Write(","u8);
        }

        HTTP.ResponseProcessor.ProcessRequest(ref requestContext, context.Response.OutputStream, requestProcesser.SQLiteConnection, requestProcesser.WebSocketListener);

        context.Response.OutputStream.Write("}"u8);
        context.Response.OutputStream.Close();
        requestProcesser.Listener.BeginGetContext(ReceivedRequest, requestProcesser);
        return;
    }

    context.Response.OutputStream.Write("hello World! it's listening"u8);
    context.Response.OutputStream.Close();
    requestProcesser.Listener.BeginGetContext(ReceivedRequest, requestProcesser);
}

static SQLiteConnection InitializedDatabase()
{
    var file = Setting.EthereumChainStoragePath.EnsureEndsWith(".sqlite", StringComparison.OrdinalIgnoreCase);
    if (!File.Exists(file))
        SQLiteConnection.CreateFile(file);
    return new SQLiteConnection($"Data Source={file};Version=3;");
}