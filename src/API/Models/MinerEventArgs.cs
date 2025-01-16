using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal abstract class MinerEventArgs : EventArgs
{
    public abstract void ParseFromPacket(Span<byte> data);
    public abstract void WriteToByte(Span<byte> data);
    public abstract ushort GetWrittenByteSize();
}
