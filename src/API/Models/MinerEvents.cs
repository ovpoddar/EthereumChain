using Shared.Core;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal struct MinerEvents
{
    public static event EventHandler<BaseTransaction>? Transaction_Added;
    public static event EventHandler? Transaction_Updated;
    public static event EventHandler? Block_Generated;
    public static event EventHandler? Block_Confirmed;

    public static void RaisedMinerEvent(MinerEventsTypes minerEvents, MinerEventArgs eventArgs)
    {
        switch (minerEvents)
        {
            case MinerEventsTypes.TransactionAdded:
                Transaction_Added?.Invoke(null, eventArgs as BaseTransaction ?? throw new Exception("invalid data found"));
                break;
            case MinerEventsTypes.TransactionUpdated:
                Transaction_Updated?.Invoke(null, null);
                break;
            case MinerEventsTypes.BlockGenerated:
                Block_Generated?.Invoke(null, null);
                break;
            case MinerEventsTypes.BlockConfirmed:
                Block_Confirmed?.Invoke(null, null);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
