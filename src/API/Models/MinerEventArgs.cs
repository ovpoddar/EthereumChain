using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal abstract class MinerEventArgs : EventArgs
{
    public abstract ushort GetWrittenByteSize();
    public abstract RequestEvent GetRequestData(Span<byte> context);
}
