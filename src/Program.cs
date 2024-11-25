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

void ReceivedRequest(IAsyncResult ar)
{
    var listener = ar.AsyncState as HttpListener;
    if (listener == null)
        return;
    var context = listener.EndGetContext(ar);
    if (RequestProcessor.TryProcessForBlockChainNetworks(context.Request, out var request))
    {
        ResponseProcessor.ProcessRequest(ref request, context.Response);
        listener.BeginGetContext(ReceivedRequest, listener);
        return;
    }

    context.Response.OutputStream.Write("hello World! it's listening"u8);
    context.Response.OutputStream.Close();
    listener.BeginGetContext(ReceivedRequest, listener);
}