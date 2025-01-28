using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public abstract class MinerEventArgs : EventArgs
{
    public abstract ushort GetWrittenByteSize();
    public abstract RequestEvent GetRequestEvent(Span<byte> context);
}
