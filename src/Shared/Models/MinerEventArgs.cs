namespace Shared.Models;
public interface IMinerEventArgs
{
    ushort GetWrittenByteSize();
    RequestEvent GetRequestEvent(Span<byte> context);
}
