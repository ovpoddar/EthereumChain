using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models;
internal enum MinerEventsTypes
{
    TransactionAdded,
    TransactionUpdated,
    BlockGenerated,
    BlockConfirmed
}
