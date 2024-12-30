using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal class MinerEvents
{
    public static event EventHandler? Transaction_Added;
    public static event EventHandler? Transaction_Updated;
    public static event EventHandler? Block_Generated;
    public static event EventHandler? Block_Confirmed;

    public enum Transaction
    {
        Added,
        Updated
    }
    public enum Block
    {
        Generated,
        Confirmed
    }

    public static void RaisedEvent(Transaction transaction, EventArgs eventArgs)
    {
        switch(transaction)
        {
            case Transaction.Added:
                Transaction_Added?.Invoke(null, eventArgs);
                break;
            case Transaction.Updated:
                Transaction_Updated?.Invoke(null, eventArgs);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public static void RaisedEvent(Block transaction, EventArgs eventArgs)
    {
        switch (transaction)
        {
            case Block.Generated:
                Block_Generated?.Invoke(null, eventArgs);
                break;
            case Block.Confirmed:
                Block_Confirmed?.Invoke(null, eventArgs);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
