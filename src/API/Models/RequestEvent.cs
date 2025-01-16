using API.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal readonly ref struct RequestEvent
{
    public MinerEventsTypes EventType { get; }
    public ReadOnlySpan<byte> EventValue { get; }
    public RequestEvent(Span<byte> readBytes)
    {
        Debug.Assert(readBytes.Length < 2048);
        EventType = readBytes[0..1].ToStruct<MinerEventsTypes>();
        EventValue = readBytes[1..];
    }

    public RequestEvent(MinerEventsTypes minerEvents, ReadOnlySpan<byte> bytes)
    {
        EventType = minerEvents;
        EventValue = bytes;
    }

    public static implicit operator byte[](RequestEvent requestEvent)
    {
        Span<byte> response = stackalloc byte[requestEvent.EventValue.Length + 1 * sizeof(byte)];
        response[0] = (byte)requestEvent.EventType;
        requestEvent.EventValue.CopyTo(response[1..]);
        return response.ToArray();
    }
}
