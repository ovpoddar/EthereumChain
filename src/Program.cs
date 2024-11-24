using src;
using src.Processors;
using System.Net;
using System.Runtime.CompilerServices;

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

[SkipLocalsInit]
void ReceivedRequest(IAsyncResult ar)
{
    var listener = ar.AsyncState as HttpListener;
    if (listener == null)
        return;
    var context = listener.EndGetContext(ar);
    var requestLength = (int)context.Request.ContentLength64;
    Span<byte> requestContext = stackalloc byte[requestLength < 1024 ? requestLength : 1024];

    if (RequestProcessor.TryProcessForBlockChainNetworks(context.Request, requestContext))
    {
        ResponseProcessor.ProcessRequest(ref requestContext, context.Response);
        return;
    }

    context.Response.OutputStream.Write("hello World! it's listening"u8);
    context.Response.OutputStream.Close();
    listener.BeginGetContext(ReceivedRequest, listener);
}