﻿using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Processors.WebSocket;
internal static class ResponseProcessor
{
    public static MinerEvents MinerEvents = new();

    public static void ProcessRequest(Span<byte> response)
    {
        // start the process and of miner events or handle it here.
        MinerEvents.RaisedEvent(MinerEvents.Block.Confirmed, EventArgs.Empty);

    }
}
