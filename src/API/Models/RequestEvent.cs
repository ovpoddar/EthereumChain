using API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal ref struct RequestEvent
{
    public MinerEventsTypes EventType { get; }
    public ReadOnlySpan<byte> EventValue { get; }
    public RequestEvent(Span<byte> readBytes)
    {
        EventType = readBytes[0..1].ToStruct<MinerEventsTypes>();
        EventValue = readBytes[1..];
    }

    public readonly int GetEventSizeInByte() =>
        EventValue.Length + 1 * sizeof(byte);

    public readonly void Copy(Span<byte> destination)
    {
        if (destination.Length != GetEventSizeInByte()) throw new ArgumentNullException(nameof(destination));
        destination[0] = (byte)this.EventType;
        EventValue.CopyTo(destination[1..]);
    }
}
