using API;
using API.Handlers;
using API.Helpers;
using API.Models;
using API.Processors.Database;
using HTTP = API.Processors.HTTP;
using WebSocket = API.Processors.WebSocket;
using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;

List<System.Net.WebSockets.WebSocket> minerConnections = new(Setting.MinerNetworkCount);
using (var sqlConnection = InitializedDatabase())
using (var listener = new HttpListener())
{
    listener.Prefixes.Add($"http://localhost:{Setting.RPCPort}/");
    listener.Prefixes.Add($"http://127.0.0.1:{Setting.RPCPort}/");
    listener.Start();
    foreach (var item in listener.Prefixes)
        Console.WriteLine($"RPC Application is listening on {item}");
    Console.WriteLine($"Miner application listening on ws://http://127.0.0.1:{Setting.RPCPort}");

    await StructureProcesser.MigrationStructure(sqlConnection);
    listener.BeginGetContext(ReceivedRequest, new ProcessorModel(listener, sqlConnection));
    Console.ReadLine();
}

async void ReceivedRequest(IAsyncResult ar)
{
    if (ar.AsyncState is not ProcessorModel requestProcesser)
        return;

    var context = requestProcesser.Listener.EndGetContext(ar);
    if (WebSocket.RequestProcessor.CanProcessAsBlockChainResponce(context.Request.Headers))
    {
        await WebSocket.RequestProcessor.VerifyRequest(minerConnections, context);
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

        HTTP.ResponseProcessor.ProcessRequest(ref requestContext, context.Response.OutputStream, requestProcesser.SQLiteConnection);

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