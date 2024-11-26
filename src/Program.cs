using src;
using src.Processors;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

const int LISTENERPORT = 9546;

using (var listener = new HttpListener())
{
    listener.Prefixes.Add($"http://localhost:{LISTENERPORT}/");
    listener.Prefixes.Add($"http://127.0.0.1:{LISTENERPORT}/");
    listener.Start();
    foreach (var item in listener.Prefixes)
        Console.WriteLine($"application is listening on {item}");

    listener.BeginGetContext(ReceivedRequest, listener);
    Console.ReadLine();
}

void ReceivedRequest(IAsyncResult ar)
{
    if (ar.AsyncState is not HttpListener listener)
        return;

    var context = listener.EndGetContext(ar);
    if (RequestProcessor.TryProcessForBlockChainNetworks(context.Request, out var request))
    {
        context.Response.OutputStream.Write("{"u8);

        context.Response.OutputStream.Write("\"jsonrpc\":\""u8);
        context.Response.OutputStream.Write(Setting.WorkingRpcVersionByte);
        context.Response.OutputStream.Write("\","u8);

        if (request.Id != null)
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes($"\"id\":{request.Id},"));

        ResponseProcessor.ProcessRequest(ref request, context.Response.OutputStream);

        context.Response.OutputStream.Write("}"u8);
        context.Response.OutputStream.Close();
        listener.BeginGetContext(ReceivedRequest, listener);
        return;
    }

    context.Response.OutputStream.Write("hello World! it's listening"u8);
    context.Response.OutputStream.Close();
    listener.BeginGetContext(ReceivedRequest, listener);
}