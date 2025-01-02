using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal class MinerEvents
{
    public static event EventHandler<TransactionAddedEventArgs>? Transaction_Added;
    public static event EventHandler? Transaction_Updated;
    public static event EventHandler? Block_Generated;
    public static event EventHandler? Block_Confirmed;

}
