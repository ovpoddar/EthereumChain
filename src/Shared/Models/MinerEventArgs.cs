using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;
public interface IMinerEventArgs
{
    ushort GetWrittenByteSize();
    RequestEvent GetRequestEvent(Span<byte> context);
}
