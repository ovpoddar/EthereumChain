using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Processors.MinerEvents;
internal class EventProcesser
{
    public void Hanlde()
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

    private void MinerEvents_Transaction_Added(object? sender, Models.TransactionAddedEventArgs e)
    {
        Console.WriteLine("Something happend.");
    }
}
