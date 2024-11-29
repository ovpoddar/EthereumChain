﻿using src;
using src.Handlers;
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
    var requestLength = (int)context.Request.ContentLength64;
    Span<byte> requestContext = stackalloc byte[requestLength < 1024 ? requestLength : 1024];
    context.Request.InputStream.ReadExactly(requestContext);

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

        ResponseProcessor.ProcessRequest(ref requestContext, context.Response.OutputStream);

        context.Response.OutputStream.Write("}"u8);
        context.Response.OutputStream.Close();
        listener.BeginGetContext(ReceivedRequest, listener);
        return;
    }

    context.Response.OutputStream.Write("hello World! it's listening"u8);
    context.Response.OutputStream.Close();
    listener.BeginGetContext(ReceivedRequest, listener);
}