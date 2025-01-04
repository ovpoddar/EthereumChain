using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Processors.MinerEvents;
internal struct MinerEvents
{
    public static event EventHandler<TransactionAddedEventArgs>? Transaction_Added;
    public static event EventHandler? Transaction_Updated;
    public static event EventHandler? Block_Generated;
    public static event EventHandler? Block_Confirmed;

    public static void RaisedMinerEvent(MinerEventsTypes minerEvents, object eventArgs)
    {
        switch (minerEvents)
        {
            case MinerEventsTypes.TransactionAdded:
                Transaction_Added?.Invoke(null, eventArgs as TransactionAddedEventArgs ?? throw new Exception("invalid data found"));
                break;
            case MinerEventsTypes.TransactionUpdated:
                Transaction_Added?.Invoke(null, null);
                break;
            case MinerEventsTypes.BlockGenerated:
                Transaction_Added?.Invoke(null, null);
                break;
            case MinerEventsTypes.BlockConfirmed:
                Transaction_Added?.Invoke(null, null);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
