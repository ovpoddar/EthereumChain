using Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.JsonRpc.Client;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace API.Processors.WebSocket;
public class WebSocketServer : IDisposable
{
    private readonly TcpListener _socket;
    private bool disposedValue;

    public WebSocketServer(IPAddress address, int port)
    {
        _socket = new(new IPEndPoint(address, port));
        _socket.Start();
        var listeningPort = (IPEndPoint?)_socket.LocalEndpoint ?? throw new Exception("Unexpected behavior: Some thing went wrong. expecting the port.");
        Console.WriteLine($"Miner application listening on {_socket.LocalEndpoint}");
        Debug.Assert(listeningPort.Port == port);
    }

    public void ListenForClients() =>
        _socket.BeginAcceptTcpClient(AcceptConnection, null);

    private void AcceptConnection(IAsyncResult ar)
    {
        var clientSocket = _socket.EndAcceptTcpClient(ar);
        try
        {
            var clientStream = clientSocket.GetStream();
            var bytes = new byte[1024];
            clientSocket.GetStream().BeginRead(bytes, 0, bytes.Length, BeginHandling, (bytes, clientSocket));
        }
        catch
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
                clientSocket.Dispose();
            }
        }
    }

    private void BeginHandling(IAsyncResult ar)
    {
        if (ar.AsyncState is not (byte[] bytes, TcpClient clientSocket))
            return;

        var totalReceived = clientSocket.GetStream().EndRead(ar);
        var request = Encoding.UTF8.GetString(bytes, 0, totalReceived);
        MakeHandShake(clientSocket.GetStream(), request, () =>
        {
            while (true)
            {
                while (!clientSocket.GetStream().DataAvailable) ;
                while (clientSocket.Available < 3) ;
                clientSocket.GetStream().ReadExactly(bytes, 0, clientSocket.Available);
                bool fin = (bytes[0] & 0b10000000) != 0, mask = (bytes[1] & 0b10000000) != 0;
                int opcode = bytes[0] & 0b00001111;
                ulong offset = 2,
                      msglen = bytes[1] & (ulong)0b01111111;

                switch (msglen)
                {
                    case 126:
                        msglen = BitConverter.ToUInt16([bytes[3], bytes[2]], 0);
                        offset = 4;
                        break;
                    case 127:
                        msglen = BitConverter.ToUInt64([bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2]], 0);
                        offset = 10;
                        break;
                }

                if (mask)
                {
                    byte[] decoded = new byte[msglen];
                    byte[] masks = [bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3]];
                    offset += 4;

                    for (ulong i = 0; i < msglen; ++i)
                        decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);

                    string text = Encoding.UTF8.GetString(decoded);
                    Console.WriteLine("{0}", text);
                    FrameData(clientSocket, Encoding.UTF8.GetBytes("Hellow world"));
                }
                // TODO: close connection when asked for.
            }
        });



    }

    string? GetAcceptKey(ReadOnlySpan<char> buffer)
    {
        const string GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        const string KeyHeader = "Sec-WebSocket-Key:";
        int start = buffer.IndexOf(KeyHeader.AsSpan());
        if (start == -1)
            return null;
        start += KeyHeader.Length;

        var end = buffer.Slice(start).IndexOf("\r\n".AsSpan());
        if (end == -1)
            return null;

        var keySpan = buffer.Slice(start, end).Trim();
        var combined = $"{keySpan.ToString()}{GUID}";
        Span<byte> combinedBytes = stackalloc byte[Encoding.UTF8.GetByteCount(combined)];
        Encoding.UTF8.GetBytes(combined, combinedBytes);
        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(combinedBytes, hash);
        return Convert.ToBase64String(hash);
    }

    void MakeHandShake(NetworkStream connection, ReadOnlySpan<char> buffer, Action afterConnected)
    {
        try
        {
            var acceptKey = GetAcceptKey(buffer);
            if (acceptKey == null) return;
            var response = Encoding.UTF8.GetBytes($"HTTP/1.1 101 Switching Protocols\r\nUpgrade: web Client\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: {acceptKey}\r\n\r\n");
            Send(connection, response, afterConnected, (e) =>
            {
                Console.WriteLine("proocol exchange fail");
            });
        }
        catch (Exception e)
        {
            Console.Write(buffer.ToString());
        }
    }

    Task? Send(NetworkStream stream, byte[] buffer, Action callback, Action<Exception> error)
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

    public static void FrameData(TcpClient connection, byte[] payload)
    {
        byte op = 129;

        connection.GetStream().WriteByte(op);

        if (payload.Length > UInt16.MaxValue)
        {
            connection.GetStream().WriteByte(127);
            var lengthBytes = BitConverter.GetBytes((ulong)payload.Length);
            connection.GetStream().Write(lengthBytes, 0, lengthBytes.Length);
        }
        else if (payload.Length > 125)
        {
            connection.GetStream().WriteByte(126);
            var lengthBytes = BitConverter.GetBytes((ushort)payload.Length);
            connection.GetStream().Write(lengthBytes, 0, lengthBytes.Length);
        }
        else
        {
            connection.GetStream().WriteByte((byte)payload.Length);
        }
        connection.GetStream().Write(payload, 0, payload.Length);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _socket.Dispose();
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