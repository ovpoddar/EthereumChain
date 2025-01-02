using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal ref struct RequestEvent
{
    public MinerEventsTypes EventType { get; set; }
    public ReadOnlySpan<byte> EventValue { get; set; }
}
