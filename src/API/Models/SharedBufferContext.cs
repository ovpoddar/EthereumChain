using System.Runtime.InteropServices;

namespace API.Models;

[StructLayout(LayoutKind.Sequential)]
public struct SharedBufferContext
{
    // 0 - safe to write, 1 - safe to read
    public byte State;
    public int Size;
    // 0 - false, 1 - true
    public byte IsContinues;
    public ushort ProcessId;
}
