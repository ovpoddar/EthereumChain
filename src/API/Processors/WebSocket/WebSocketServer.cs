using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace API.Processors.WebSocket;
public class WebSocketServer : IDisposable
{
    private readonly Socket _socket;
    private bool disposedValue;

    public WebSocketServer(IPAddress address, int port)
    {
        _socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.IP);
        _socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        _socket.Bind(new IPEndPoint(address, port));
        _socket.Listen(50);
        var listeningPort = (IPEndPoint?)_socket.LocalEndPoint ?? throw new Exception("Unexpected behavior: Some thing went wrong. expecting the port.");
        Console.WriteLine($"Miner application listening on {listeningPort.Port}");
        Debug.Assert(listeningPort.Port == port);
    }

    public void ListenForClients() =>
        _socket.BeginAccept(BeginAccept, _socket);

    private void BeginAccept(IAsyncResult asyncResult)
    {
        if (asyncResult.AsyncState is not Socket serverSocket)
            return;
        var clientSocket = serverSocket.EndAccept(asyncResult);
        try
        {
            clientSocket.Blocking = true;
            var buffers = new byte[1024];
            clientSocket.BeginReceive(buffers, 0, buffers.Length, SocketFlags.None, Receving, new DTO(buffers, clientSocket));
        }
        catch
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
                clientSocket.Dispose();
            }
        }
        finally
        {
            serverSocket.BeginAccept(BeginAccept, serverSocket);
        }
    }

    private void Receving(IAsyncResult asyncResult)
    {
        var received = asyncResult.AsyncState as DTO;
        if (received == null)
            return;
        var totalReceived = received.socket.EndReceive(asyncResult);
        var request = Encoding.UTF8.GetString(received.data, 0, totalReceived);
        MakeHandShake(received.socket, request);
        Console.WriteLine("Connection open");
    }


    string? GetAcceptKey(ReadOnlySpan<char> buffer)
    {
        const string GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        const string KeyHeader = "Sec-WebSocket-Key:";
        var start = buffer.IndexOf(KeyHeader.AsSpan()) != -1 ? buffer.IndexOf(KeyHeader.AsSpan()) + KeyHeader.Length : throw new Exception("the header is not valid");
        var end = buffer.Slice(start).IndexOf("\r\n".AsSpan());
        var keySpan = buffer.Slice(start, end).Trim();
        Console.WriteLine(keySpan.ToString());
        var combined = $"{keySpan.ToString()}{GUID}";
        Span<byte> combinedBytes = stackalloc byte[Encoding.UTF8.GetByteCount(combined)];
        Encoding.UTF8.GetBytes(combined, combinedBytes);
        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(combinedBytes, hash);
        return Convert.ToBase64String(hash);
    }

    void MakeHandShake(Socket connection, ReadOnlySpan<char> buffer)
    {
        try
        {
            var acceptKey = GetAcceptKey(buffer);
            using var s = new NetworkStream(connection);
            var responce = Encoding.UTF8.GetBytes($"HTTP/1.1 101 Switching Protocols\r\nUpgrade: web socket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: {acceptKey}\r\n\r\n");
            Send(s,
                responce,
                () =>
                {
                    //connection.Send("Hello world"u8);
                    Console.WriteLine("opened Connection");
                    var read = new byte[1024];
                    connection.BeginReceive(read, 0, read.Length, SocketFlags.None, GettingData, new DTO(read, connection));
                },
                (e) =>
                {
                    Console.WriteLine(e.Message);
                });
            s.Flush();
        }
        catch (Exception e)
        {
            Console.Write(buffer.ToString());
        }
    }

    private void GettingData(IAsyncResult asyncResult)
    {
        var received = asyncResult.AsyncState as DTO;
        if (received == null)
            return;
        var totalReceived = received.socket.EndReceive(asyncResult);
        Console.WriteLine(totalReceived);
        Console.WriteLine(Encoding.UTF8.GetString(received.data, 0, totalReceived));
        received.socket.BeginReceive(received.data, 0, received.data.Length, SocketFlags.None, GettingData, received);
    }

    public Task Send(Stream stream, byte[] buffer, Action callback, Action<Exception> error)
    {
        try
        {
            Func<AsyncCallback, object, IAsyncResult> begin =
                (cb, s) => stream.BeginWrite(buffer, 0, buffer.Length, cb, s);

            Task task = Task.Factory.FromAsync(begin, stream.EndWrite, null);
            task.ContinueWith(t => callback(), TaskContinuationOptions.NotOnFaulted)
                .ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }
        catch (Exception e)
        {
            error(e);
            return null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _socket.Close();
            }
            _socket.Dispose();
            disposedValue = true;
        }
    }
   
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

file record DTO(byte[] data, Socket socket);
