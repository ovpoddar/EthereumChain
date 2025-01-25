 using API.Handlers;
using API.Models;
using API.Processors;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Processors.WebSocket;
internal class ResponseProcessor
{
    // as miner i'm responding on raising events
    private readonly MinerSocketProcessor _webSocketListener;

    public ResponseProcessor(MinerSocketProcessor webSocketListener) =>
        this._webSocketListener = webSocketListener;

    public void HookEventHandlers()
    {
        MinerEvents.Transaction_Added += MinerEvents_Transaction_Added;
        MinerEvents.Transaction_Updated += MinerEvents_Transaction_Updated;
        MinerEvents.Block_Generated += MinerEvents_Block_Generated;
        MinerEvents.Block_Confirmed += MinerEvents_Block_Confirmed;
    }

    private void MinerEvents_Block_Confirmed(object? sender, EventArgs e)
    {
        Console.WriteLine("Something happend.");
    }

    private void MinerEvents_Block_Generated(object? sender, EventArgs e)
    {
        Console.WriteLine("Something happend.");
    }

    private void MinerEvents_Transaction_Updated(object? sender, EventArgs e)
    {
        Console.WriteLine("Something happend.");
    }

    private async void MinerEvents_Transaction_Added(object? sender, TransactionAddedEventArgs e)
    {
        Span<byte> context = stackalloc byte[e.GetWrittenByteSize()];
        var requestEvent = e.GetRequestData(context);
        await _webSocketListener.NotifyAll(requestEvent);
    }

}
