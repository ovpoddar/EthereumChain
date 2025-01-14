using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal abstract class MinerEventArgs : EventArgs
{
    protected abstract void ParseFromPacket(Span<byte> data);
}
