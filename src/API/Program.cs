using System.Data.SQLite;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using API.Helpers;
using API.Processors.Database;
using API.Models;
using API.Processors.HTTP;
using API;
using API.Handlers;
using System.Net.WebSockets;
using API.Processors.WebSocket;

using (var sqlConnection = InitializedDatabase())
using (var minerListener = new WebSocketServer(IPAddress.Parse("127.0.0.1"), Setting.MinerListenerPort))
using (var listener = new HttpListener())
{
    listener.Prefixes.Add($"http://localhost:{Setting.RPCPort}/");
    listener.Prefixes.Add($"http://127.0.0.1:{Setting.RPCPort}/");
    listener.Start();
    minerListener.Start();
    foreach (var item in listener.Prefixes)
        Console.WriteLine($"RPC Application is listening on {item}");

    await StructureProcesser.MigrationStructure(sqlConnection);
    listener.BeginGetContext(ReceivedRequest, new ProcesserModels(listener, sqlConnection));
    minerListener.BeginReceived(HandlingResponse);
    Console.ReadLine();
}

void GeneratedBlock()
{
    Console.WriteLine("Text");
}

static void ReceivedRequest(IAsyncResult ar)
{
    if (ar.AsyncState is not ProcesserModels requestProcesser)
        return;

    var context = requestProcesser.Listener.EndGetContext(ar);
    var requestLength = (int)context.Request.ContentLength64;
    Span<byte> requestContext = stackalloc byte[requestLength < 1024 ? requestLength : 1024];
    context.Request.InputStream.ReadExactly(requestContext);

    ResponseProcessor.SetUpHeaders(context);

    if (RequestProcessor.CanProcessAsBlockChainRequest(context.Request, ref requestContext))
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

        ResponseProcessor.ProcessRequest(ref requestContext, context.Response.OutputStream, requestProcesser.SQLiteConnection);

        context.Response.OutputStream.Write("}"u8);
        context.Response.OutputStream.Close();
        requestProcesser.Listener.BeginGetContext(ReceivedRequest, requestProcesser);
        return;
    }

    context.Response.OutputStream.Write("hello World! it's listening"u8);
    context.Response.OutputStream.Close();
    requestProcesser.Listener.BeginGetContext(ReceivedRequest, requestProcesser);
}

static void HandlingResponse(byte[] responce)
{
}

static SQLiteConnection InitializedDatabase()
{
    var file = Setting.EthereumChainStoragePath.EnsureEndsWith(".sqlite", StringComparison.OrdinalIgnoreCase);
    if (!File.Exists(file))
        SQLiteConnection.CreateFile(file);
    return new SQLiteConnection($"Data Source={file};Version=3;");
}